using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace WeatherApi;

public class Headline
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public string? Source { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CollectedAt { get; set; }
}

public record HeadlineDto(string Title, string Url, string? Source, DateTime? PublishedAt, DateTime CollectedAt);

/// <summary>뉴스 RSS 헤드라인을 주기적으로 수집해 SQLite에 저장한다. 이미 저장된 URL은 건너뛴다.</summary>
public class NewsCollectorWorker(
    IServiceScopeFactory scopes,
    IHttpClientFactory httpFactory,
    IConfiguration config,
    ILogger<NewsCollectorWorker> logger) : BackgroundService
{
    private const string DefaultFeedUrl = "https://news.google.com/rss?hl=ko&gl=KR&ceid=KR:ko";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minutes = config.GetValue("NEWS_INTERVAL_MINUTES", 30);
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));
        do
        {
            await CollectAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CollectAsync(CancellationToken ct)
    {
        try
        {
            var feedUrl = config["NEWS_FEED_URL"] ?? DefaultFeedUrl;
            var xml = await httpFactory.CreateClient().GetStringAsync(feedUrl, ct);
            var items = Parse(xml);

            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var urls = items.Select(i => i.Url).ToList();
            var existing = (await db.Headlines.Where(h => urls.Contains(h.Url)).Select(h => h.Url).ToListAsync(ct)).ToHashSet();
            var now = DateTime.UtcNow;
            var fresh = items.Where(i => !existing.Contains(i.Url)).Select(i => new Headline
            {
                Title = i.Title,
                Url = i.Url,
                Source = i.Source,
                PublishedAt = i.PublishedAt,
                CollectedAt = now,
            }).ToList();

            db.Headlines.AddRange(fresh);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("뉴스 헤드라인 {Count}건 저장 (피드 {Total}건)", fresh.Count, items.Count);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or System.Xml.XmlException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(e, "뉴스 수집 실패");
        }
    }

    private static List<ParsedItem> Parse(string xml) =>
        XDocument.Parse(xml).Descendants("item")
            .Select(item => new ParsedItem(
                item.Element("title")?.Value.Trim() ?? "",
                item.Element("link")?.Value.Trim() ?? "",
                item.Element("source")?.Value.Trim(),
                DateTimeOffset.TryParse(item.Element("pubDate")?.Value, out var published) ? published.UtcDateTime : null))
            .Where(i => i.Title.Length > 0 && i.Url.Length > 0)
            .DistinctBy(i => i.Url)
            .ToList();

    private record ParsedItem(string Title, string Url, string? Source, DateTime? PublishedAt);
}

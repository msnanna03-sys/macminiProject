using Microsoft.EntityFrameworkCore;

namespace WeatherApi;

public class Reading
{
    public int Id { get; set; }
    public required string Metric { get; set; }
    public double Value { get; set; }
    public DateTime CollectedAt { get; set; }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<Headline> Headlines => Set<Headline>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reading>().HasIndex(r => new { r.Metric, r.CollectedAt });
        modelBuilder.Entity<Headline>().HasIndex(h => h.Url).IsUnique();
    }
}

public static class Metrics
{
    public const string UsdKrw = "usd_krw";
    public const string HomeTemperature = "home_temperature";
}

public record ReadingDto(DateTime CollectedAt, double Value);

/// <summary>주기적으로 환율과 실내 온도를 수집해 SQLite에 저장한다.</summary>
public class CollectorWorker(
    IServiceScopeFactory scopes,
    IHttpClientFactory httpFactory,
    IConfiguration config,
    ILogger<CollectorWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minutes = config.GetValue("HISTORY_INTERVAL_MINUTES", 10);
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));
        do
        {
            await CollectAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CollectAsync(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ha = scope.ServiceProvider.GetRequiredService<IHomeAssistantClient>();
        var now = DateTime.UtcNow;

        try
        {
            var rate = await FetchUsdKrwAsync(ct);
            if (rate is { } value)
                db.Readings.Add(new Reading { Metric = Metrics.UsdKrw, Value = value, CollectedAt = now });
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(e, "환율 수집 실패");
        }

        try
        {
            var temperature = await ha.GetTemperatureAsync(ct);
            db.Readings.Add(new Reading { Metric = Metrics.HomeTemperature, Value = temperature.Value, CollectedAt = now });
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(e, "실내 온도 수집 실패");
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task<double?> FetchUsdKrwAsync(CancellationToken ct)
    {
        var http = httpFactory.CreateClient();
        var response = await http.GetFromJsonAsync<ExchangeRateResponse>("https://open.er-api.com/v6/latest/USD", ct);
        return response?.Rates.TryGetValue("KRW", out var krw) == true ? krw : null;
    }

    private record ExchangeRateResponse(Dictionary<string, double> Rates);
}

using Microsoft.EntityFrameworkCore;
using WeatherApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient<WeatherService>();

if (string.IsNullOrWhiteSpace(builder.Configuration["HOME_ASSISTANT_URL"]))
    builder.Services.AddSingleton<IHomeAssistantClient, MockHomeAssistantClient>();
else
    builder.Services.AddHttpClient<IHomeAssistantClient, HomeAssistantClient>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("History") ?? "Data Source=data/history.db"));
builder.Services.AddHttpClient();
builder.Services.AddHostedService<CollectorWorker>();
builder.Services.AddHostedService<NewsCollectorWorker>();

var app = builder.Build();

Directory.CreateDirectory("data");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // EnsureCreated는 기존 DB에 새 테이블을 추가하지 않으므로 Headlines는 직접 보장한다.
    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "Headlines" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_Headlines" PRIMARY KEY AUTOINCREMENT,
            "Title" TEXT NOT NULL,
            "Url" TEXT NOT NULL,
            "Source" TEXT NULL,
            "PublishedAt" TEXT NULL,
            "CollectedAt" TEXT NOT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_Headlines_Url" ON "Headlines" ("Url");
        """);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "WeatherApi v1"));
}

app.MapGet("/api/weather", async (string? city, WeatherService weather, CancellationToken ct) =>
{
    try
    {
        var result = await weather.GetAsync(city ?? "Seoul", ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (WeatherKeyMissingException e)
    {
        return Results.Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (HttpRequestException e)
    {
        var status = e.StatusCode == System.Net.HttpStatusCode.NotFound
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status502BadGateway;
        return Results.Problem($"날씨 API 호출 실패: {(int?)e.StatusCode}", statusCode: status);
    }
})
.WithName("GetWeather")
.Produces<WeatherResult>()
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status502BadGateway);

var home = app.MapGroup("/api/home").WithTags("Home");

home.MapGet("/light", (IHomeAssistantClient ha, CancellationToken ct) => ha.GetLightAsync(ct))
    .WithName("GetLight");

home.MapPost("/light", (LightRequest request, IHomeAssistantClient ha, CancellationToken ct) =>
    ha.SetLightAsync(request.IsOn, ct))
    .WithName("SetLight");

home.MapGet("/temperature", (IHomeAssistantClient ha, CancellationToken ct) => ha.GetTemperatureAsync(ct))
    .WithName("GetTemperature");

app.MapGet("/api/history", async (string metric, int? hours, AppDbContext db, CancellationToken ct) =>
{
    var since = DateTime.UtcNow.AddHours(-(hours ?? 24));
    var readings = await db.Readings
        .Where(r => r.Metric == metric && r.CollectedAt >= since)
        .OrderBy(r => r.CollectedAt)
        .ToListAsync(ct);
    // SQLite는 DateTimeKind를 저장하지 않으므로 UTC로 지정해 응답에 'Z'가 붙게 한다.
    return Results.Ok(readings.Select(r =>
        new ReadingDto(DateTime.SpecifyKind(r.CollectedAt, DateTimeKind.Utc), r.Value)));
})
.WithName("GetHistory")
.Produces<List<ReadingDto>>();

app.MapGet("/api/news", async (int? limit, AppDbContext db, CancellationToken ct) =>
{
    var headlines = await db.Headlines
        .OrderByDescending(h => h.PublishedAt ?? h.CollectedAt)
        .Take(Math.Clamp(limit ?? 20, 1, 100))
        .ToListAsync(ct);
    return Results.Ok(headlines.Select(h => new HeadlineDto(
        h.Title, h.Url, h.Source,
        h.PublishedAt is { } p ? DateTime.SpecifyKind(p, DateTimeKind.Utc) : null,
        DateTime.SpecifyKind(h.CollectedAt, DateTimeKind.Utc))));
})
.WithName("GetNews")
.Produces<List<HeadlineDto>>();

app.Run();

record LightRequest(bool IsOn);

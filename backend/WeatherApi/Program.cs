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

var app = builder.Build();

Directory.CreateDirectory("data");
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

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

app.Run();

record LightRequest(bool IsOn);

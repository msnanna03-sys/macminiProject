using WeatherApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient<WeatherService>();

var app = builder.Build();

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

app.Run();

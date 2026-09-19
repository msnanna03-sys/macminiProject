using System.Text.Json.Serialization;

namespace WeatherApi;

public record WeatherResult(string City, string Description, double Temp, double FeelsLike, int Humidity, double WindSpeed);

public class WeatherKeyMissingException() : Exception("환경변수 WEATHER_API_KEY가 설정되지 않았습니다.");

public class WeatherService(HttpClient http, IConfiguration config)
{
    public async Task<WeatherResult?> GetAsync(string city, CancellationToken ct)
    {
        var apiKey = config["WEATHER_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new WeatherKeyMissingException();

        var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(city)}&appid={apiKey}&units=metric&lang=kr";
        var response = await http.GetFromJsonAsync<OwmResponse>(url, ct);
        if (response is null) return null;

        return new WeatherResult(
            response.Name,
            response.Weather.FirstOrDefault()?.Description ?? "-",
            response.Main.Temp,
            response.Main.FeelsLike,
            response.Main.Humidity,
            response.Wind.Speed);
    }

    private record OwmResponse(string Name, Main Main, Wind Wind, List<Desc> Weather);
    private record Main(double Temp, [property: JsonPropertyName("feels_like")] double FeelsLike, int Humidity);
    private record Wind(double Speed);
    private record Desc(string Description);
}

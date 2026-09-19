using System.Net.Http.Json;
using System.Text.Json.Serialization;

var apiKey = Environment.GetEnvironmentVariable("WEATHER_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("환경변수 WEATHER_API_KEY가 설정되지 않았습니다.");
    Console.Error.WriteLine("예: export WEATHER_API_KEY=your_openweathermap_key");
    return 1;
}

var city = args.Length > 0 ? string.Join(' ', args) : "Seoul";
var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(city)}&appid={apiKey}&units=metric&lang=kr";

using var http = new HttpClient();
try
{
    var weather = await http.GetFromJsonAsync<WeatherResponse>(url);
    if (weather is null)
    {
        Console.Error.WriteLine("날씨 응답을 해석하지 못했습니다.");
        return 1;
    }

    var description = weather.Weather.FirstOrDefault()?.Description ?? "-";
    Console.WriteLine($"오늘의 날씨 - {weather.Name}");
    Console.WriteLine($"  날씨: {description}");
    Console.WriteLine($"  기온: {weather.Main.Temp:F1}°C (체감 {weather.Main.FeelsLike:F1}°C)");
    Console.WriteLine($"  습도: {weather.Main.Humidity}%");
    Console.WriteLine($"  풍속: {weather.Wind.Speed:F1} m/s");
    return 0;
}
catch (HttpRequestException e)
{
    Console.Error.WriteLine($"API 호출 실패: {(int?)e.StatusCode} {e.Message}");
    return 1;
}

record WeatherResponse(
    string Name,
    MainInfo Main,
    WindInfo Wind,
    List<WeatherDescription> Weather);

record MainInfo(
    double Temp,
    [property: JsonPropertyName("feels_like")] double FeelsLike,
    int Humidity);

record WindInfo(double Speed);

record WeatherDescription(string Description);

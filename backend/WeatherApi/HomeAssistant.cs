using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace WeatherApi;

public record LightState(bool IsOn);

public record TemperatureState(double Value, string Unit);

public interface IHomeAssistantClient
{
    Task<LightState> GetLightAsync(CancellationToken ct);
    Task<LightState> SetLightAsync(bool isOn, CancellationToken ct);
    Task<TemperatureState> GetTemperatureAsync(CancellationToken ct);
}

/// <summary>Home Assistant REST API 클라이언트. HOME_ASSISTANT_URL / HOME_ASSISTANT_TOKEN 필요.</summary>
public class HomeAssistantClient : IHomeAssistantClient
{
    private readonly HttpClient _http;
    private readonly string _lightEntity;
    private readonly string _temperatureEntity;

    public HomeAssistantClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _http.BaseAddress = new Uri(config["HOME_ASSISTANT_URL"]!.TrimEnd('/') + "/");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", config["HOME_ASSISTANT_TOKEN"]);
        _lightEntity = config["HOME_ASSISTANT_LIGHT_ENTITY"] ?? "light.living_room";
        _temperatureEntity = config["HOME_ASSISTANT_TEMPERATURE_ENTITY"] ?? "sensor.living_room_temperature";
    }

    public async Task<LightState> GetLightAsync(CancellationToken ct)
    {
        var state = await _http.GetFromJsonAsync<HaState>($"api/states/{_lightEntity}", ct);
        return new LightState(state?.State == "on");
    }

    public async Task<LightState> SetLightAsync(bool isOn, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/services/light/{(isOn ? "turn_on" : "turn_off")}", new { entity_id = _lightEntity }, ct);
        response.EnsureSuccessStatusCode();
        return new LightState(isOn);
    }

    public async Task<TemperatureState> GetTemperatureAsync(CancellationToken ct)
    {
        var state = await _http.GetFromJsonAsync<HaState>($"api/states/{_temperatureEntity}", ct);
        var value = double.Parse(state!.State, System.Globalization.CultureInfo.InvariantCulture);
        return new TemperatureState(value, state.Attributes?.UnitOfMeasurement ?? "°C");
    }

    private record HaState(string State, HaAttributes? Attributes);
    private record HaAttributes([property: JsonPropertyName("unit_of_measurement")] string? UnitOfMeasurement);
}

/// <summary>Home Assistant가 없을 때 쓰는 메모리 기반 mock 센서.</summary>
public class MockHomeAssistantClient : IHomeAssistantClient
{
    private volatile bool _lightOn;

    public Task<LightState> GetLightAsync(CancellationToken ct) => Task.FromResult(new LightState(_lightOn));

    public Task<LightState> SetLightAsync(bool isOn, CancellationToken ct)
    {
        _lightOn = isOn;
        return Task.FromResult(new LightState(isOn));
    }

    public Task<TemperatureState> GetTemperatureAsync(CancellationToken ct)
    {
        var value = Math.Round(22 + Random.Shared.NextDouble() * 2 - 1, 1);
        return Task.FromResult(new TemperatureState(value, "°C"));
    }
}

using System.Net.Http.Json;

// 수집 대상(USD→KRW 환율)을 확인하는 크롤러. WeatherApi의 수집 워커가 같은 소스를 주기적으로 호출한다.
using var http = new HttpClient();
try
{
    var response = await http.GetFromJsonAsync<ExchangeRateResponse>("https://open.er-api.com/v6/latest/USD");
    if (response is null || !response.Rates.TryGetValue("KRW", out var krw))
    {
        Console.Error.WriteLine("환율 응답을 해석하지 못했습니다.");
        return 1;
    }

    Console.WriteLine($"USD/KRW {krw:F2} ({DateTime.Now:yyyy-MM-dd HH:mm:ss})");
    return 0;
}
catch (HttpRequestException e)
{
    Console.Error.WriteLine($"수집 실패: {e.Message}");
    return 1;
}

record ExchangeRateResponse(Dictionary<string, double> Rates);

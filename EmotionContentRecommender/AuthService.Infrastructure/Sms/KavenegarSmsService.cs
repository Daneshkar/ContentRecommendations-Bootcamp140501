using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Sms;

public interface ISmsService
{
    Task<bool> SendOtpAsync(string mobile, string code);
}

public class KavenegarSmsService : ISmsService
{
    private readonly HttpClient           _httpClient;
    private readonly KavenegarSettings    _settings;

    public KavenegarSmsService(HttpClient httpClient, IOptions<KavenegarSettings> settings)
    {
        _httpClient = httpClient;
        _settings   = settings.Value;
    }

    public async Task<bool> SendOtpAsync(string mobile, string code)
    {
        var url = $"https://api.kavenegar.com/v1/{_settings.ApiKey}/verify/lookup.json";

        var parameters = new Dictionary<string, string>
        {
            { "receptor", mobile },
            { "token",    code   },
            { "template", _settings.Template }
        };

        using var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<KavenegarResponse>();

        return result?.Return?.Status == 200;
    }

    private class KavenegarResponse
    {
        [JsonPropertyName("return")]
        public KavenegarReturn? Return { get; set; }
    }

    private class KavenegarReturn
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}

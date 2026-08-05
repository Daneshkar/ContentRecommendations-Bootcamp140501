using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EmotionClient.Models;

namespace EmotionClient.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AuthApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AuthService:BaseUrl"] ?? "https://localhost:7252";
    }

    public async Task<AuthApiResponse<LoginData>> LoginAsync(string username, string password)
    {
        var payload = new { username, password };
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/auth/login", payload);
        var result = await DeserializeAsync<LoginData>(response);
        result.Cookies = ExtractSetCookies(response);
        return result;
    }

    public async Task<AuthApiResponse<RegisterData>> RegisterAsync(RegisterViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/auth/register", model);
        return await DeserializeAsync<RegisterData>(response);
    }

    public async Task<AuthApiResponse> SendOtpAsync(string mobile)
    {
        var payload = new { mobile };
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/auth/send-otp", payload);
        return await DeserializeSimpleAsync(response);
    }

    public async Task<AuthApiResponse> VerifyOtpAsync(string mobile, string code)
    {
        var payload = new { mobile, code };
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/auth/verify-otp", payload);
        return await DeserializeSimpleAsync(response);
    }

    public async Task<AuthApiResponse<ProfileData>> GetProfileAsync(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/auth/profile");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        return await DeserializeAsync<ProfileData>(response);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static async Task<AuthApiResponse<T>> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthApiResponse<T>>(body, JsonOptions)
               ?? new AuthApiResponse<T> { IsSuccess = false, Message = "خطای نامشخص", StatusCode = (int)response.StatusCode };
    }

    private static async Task<AuthApiResponse> DeserializeSimpleAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthApiResponse>(body, JsonOptions)
               ?? new AuthApiResponse { IsSuccess = false, Message = "خطای نامشخص", StatusCode = (int)response.StatusCode };
    }

    private static Dictionary<string, string> ExtractSetCookies(HttpResponseMessage response)
    {
        var cookies = new Dictionary<string, string>();
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            foreach (var cookie in setCookieValues)
            {
                var parts = cookie.Split(';')[0].Split('=', 2);
                if (parts.Length == 2)
                    cookies[parts[0]] = parts[1];
            }
        }
        return cookies;
    }

    public class LoginData
    {
        public long   UserId   { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role     { get; set; } = string.Empty;
    }

    public class RegisterData
    {
        public long   UserId   { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class ProfileData
    {
        public long      UserId       { get; set; }
        public string    Username     { get; set; } = string.Empty;
        public string?   FirstName    { get; set; }
        public string?   LastName     { get; set; }
        public string?   Email        { get; set; }
        public bool      VerifyEmail  { get; set; }
        public string?   Mobile       { get; set; }
        public bool      VerifyMobile { get; set; }
        public byte?     Gender       { get; set; }
        public DateOnly? BirthDay     { get; set; }
        public string?   AvatarUser   { get; set; }
        public string    Role         { get; set; } = string.Empty;

        public string GenderText => Gender switch
        {
            1 => "مرد",
            2 => "زن",
            3 => "سایر",
            _ => "—"
        };
    }
}

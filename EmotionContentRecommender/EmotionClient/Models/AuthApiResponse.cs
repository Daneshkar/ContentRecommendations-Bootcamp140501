using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmotionClient.Models;

public class AuthApiResponse<T>
{
    public bool    IsSuccess  { get; set; }
    public T?      Data       { get; set; }
    public string? Message    { get; set; }
    public int     StatusCode { get; set; }

    [JsonIgnore]
    public Dictionary<string, string> Cookies { get; set; } = [];
}

public class AuthApiResponse
{
    public bool    IsSuccess  { get; set; }
    public string? Message    { get; set; }
    public int     StatusCode { get; set; }

    [JsonIgnore]
    public Dictionary<string, string> Cookies { get; set; } = [];
}

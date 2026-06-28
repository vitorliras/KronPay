
using System.Text.Json.Serialization;

namespace Application.DTOs.Auth
{
    public class AuthPluggy
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = string.Empty;
    }
}

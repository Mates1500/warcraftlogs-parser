using System.Text.Json.Serialization;

namespace warcraftlogs_parser
{
    public class AuthorizationResponse
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

    }
}
using System.Text.Json.Serialization;

namespace DDNSManager.Lib.Services.Responses.Cloudflare
{
    public class CloudflareCreateRequest
    {
        [JsonPropertyName("content")]
        public string IP { get; set; }
        [JsonPropertyName("name")]
        public string Hostname { get; set; }
        [JsonPropertyName("type")]
        public string RecordType { get; set; } = "A";
        [JsonPropertyName("proxied")]
        public bool Proxied { get; set; }
    }
}

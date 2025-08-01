using System.Text.Json.Serialization;

namespace DDNSManager.Lib.DomainServices.Responses.Cloudflare
{
    public class CloudflareCreateRequest
    {
        [JsonPropertyName("content")]
        public string IP { get; set; } = null!;
        [JsonPropertyName("name")]
        public string Hostname { get; set; } = null!;
        [JsonPropertyName("type")]
        public string RecordType { get; set; } = "A";
        [JsonPropertyName("proxied")]
        public bool Proxied { get; set; }
    }
}

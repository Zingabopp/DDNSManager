using System.Text.Json.Serialization;

namespace DDNSManager.Lib.DomainServices.Responses.Cloudflare
{
    public class CloudflareRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
        [JsonPropertyName("zone_id")]
        public string ZoneId { get; set; } = null!;
        [JsonPropertyName("zone_name")]
        public string ZoneName { get; set; } = null!;
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;
        [JsonPropertyName("content")]
        public string IP { get; set; } = null!;
        [JsonPropertyName("proxied")]
        public bool Proxied { get; set; }

    }
}
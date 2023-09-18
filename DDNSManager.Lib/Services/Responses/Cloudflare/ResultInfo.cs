using System.Text.Json.Serialization;

namespace DDNSManager.Lib.Services.Responses.Cloudflare
{
    public class ResultInfo
    {
        public int Count { get; set; }
        public int Page { get; set; }

        [JsonPropertyName("per_page")]
        public int RecordsPerPage { get; set; }

        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
    }
}

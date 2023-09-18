using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DDNSManager.Lib.Services.Responses.Cloudflare
{
    public class CloudflareGetRecordsResponse
    {
        [JsonPropertyName("result")]
        public List<CloudflareRecord> Result { get; set; } = new List<CloudflareRecord>();
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("errors")]
        public ICollection<CloudflareMessage> Errors { get; set; } = Array.Empty<CloudflareMessage>();
        [JsonPropertyName("messages")]
        public ICollection<CloudflareMessage> Messages { get; set; } = Array.Empty<CloudflareMessage>();

        [JsonPropertyName("result_info")]
        public ResultInfo ResultInfo { get; set; } = new ResultInfo();
    }
}

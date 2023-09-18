using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DDNSManager.Lib.Services.Responses.Cloudflare
{
    public class CloudflareResult
    {
        [JsonPropertyName("result")]
        public CloudflareRecord? Result { get; set; }
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        public ICollection<CloudflareMessage> Errors { get; set; } = Array.Empty<CloudflareMessage>();
        public ICollection<CloudflareMessage> Messages { get; set; } = Array.Empty<CloudflareMessage>();
    }
}

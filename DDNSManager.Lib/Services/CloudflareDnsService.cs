using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.ServiceConfiguration;
using DDNSManager.Lib.Services.Responses.Cloudflare;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DDNSManager.Lib.Services
{
    public class CloudflareDnsService : IDDNSService<CloudflareDnsSettings>
    {
        public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private const string GetRecordsCacheId = "Cf_GetRecords";
        private const string ExternalIpCacheId = "ExternalIpCacheId";
        private const string BaseUrl = "https://api.cloudflare.com/client/v4/";
        public const string ServiceId = "CloudflareDns";
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache? _memoryCache;

        public CloudflareDnsSettings ServiceSettings { get; set; }

        string IDDNSService.ServiceId => ServiceId;
        IServiceSettings IDDNSService.Settings => ServiceSettings;

        public string ServiceName => "Cloudflare DNS";


        public CloudflareDnsService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            ServiceSettings = new CloudflareDnsSettings();
        }
        public CloudflareDnsService(HttpClient httpClient, CloudflareDnsSettings settings, IMemoryCache memoryCache)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            settings ??= new CloudflareDnsSettings();
            ServiceSettings = settings;
            _memoryCache = memoryCache;
        }

        public async Task<DomainMatchResult> CheckDomainAsync(CancellationToken cancellationToken = default)
        {
            string? expectedIp = await GetDomainIpAsync(cancellationToken);
            string hostname = ServiceSettings.Hostname ?? throw new InvalidOperationException("No hostname specified in settings.");
            var domainResult = await Utilities.CheckDomainAsync(_httpClient, expectedIp, hostname, cancellationToken);
            return domainResult;
        }

        public async Task<IRequestResult> SendRequestAsync(CancellationToken cancellationToken = default)
        {
            CloudflareDnsSettings settings = ServiceSettings;
            if (!ServiceSettings.IsValid())
                throw new InvalidOperationException("Settings are not valid.");
            string hostname = settings.Hostname!;
            List<CloudflareRecord> currentRecords = await GetRecordsAsync();
            CloudflareRecord? existingRecord = currentRecords.FirstOrDefault(r => hostname.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
            string? domainIp = await GetDomainIpAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(domainIp))
            {
                return new CloudflareDnsResponse("Failed: Unable to determine domain IP")
                {
                    Status = ResultStatus.Faulted,
                    Message = "Unable to determine domain IP"
                };
            }
            CloudflareCreateRequest cfRequest = new CloudflareCreateRequest()
            {
                Hostname = ServiceSettings.Hostname!,
                IP = domainIp,
                RecordType = "A",
                Proxied = ServiceSettings.Proxied
            };
            Uri uri;
            HttpRequestMessage request;
            if (existingRecord != null)
            {
                uri = GetUpdateRecordUri(ServiceSettings.ZoneId, existingRecord.Id);
                request = CreateRequestMessage(HttpMethod.Put, uri);
            }
            else
            {
                uri = GetCreateRecordUri(ServiceSettings.ZoneId);
                request = CreateRequestMessage(HttpMethod.Post, uri);

            }

            request.Content = JsonContent.Create(cfRequest);
            HttpResponseMessage rawResponse = await _httpClient.SendAsync(request, cancellationToken);
            if (rawResponse.IsSuccessStatusCode)
            {
                string json = await rawResponse.Content.ReadAsStringAsync(cancellationToken);
                CloudflareResult? response = JsonSerializer.Deserialize<CloudflareResult>(json, JsonSerializerOptions);
                return new CloudflareDnsResponse(response, json);
            }
            else
            {
                return new CloudflareDnsResponse("Failed")
                {
                    Status = ResultStatus.Faulted,
                    Message = $"Response indicated failure: {rawResponse.StatusCode} - {rawResponse.ReasonPhrase}"
                };
            }
        }

        public async Task<List<CloudflareRecord>> GetRecordsAsync(CancellationToken cancellationToken = default)
        {
            if (_memoryCache != null
                && _memoryCache.TryGetValue(GetRecordsCacheId, out object? value)
                && value is List<CloudflareRecord> records)
            {
                return records;
            }
            Uri uri = GetRecordsUri(ServiceSettings.ZoneId);
            HttpRequestMessage request = CreateRequestMessage(HttpMethod.Get, uri);
            using HttpResponseMessage rawResponse = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            CloudflareGetRecordsResponse? response = await rawResponse.Content.ReadFromJsonAsync<CloudflareGetRecordsResponse>(JsonSerializerOptions, cancellationToken);
            if (response == null)
            {
                throw new SerializationException("Could not deserialize response");
            }
            records = response.Result;
            if (_memoryCache != null)
            {
                _memoryCache.Set(GetRecordsCacheId, records, TimeSpan.FromMinutes(5));
            }
            return records;
        }

        private async ValueTask<string?> GetDomainIpAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ServiceSettings.IP))
            {
                return await GetExternalIpAsync(cancellationToken);
            }
            return ServiceSettings.IP;
        }

        private async Task<string?> GetExternalIpAsync(CancellationToken cancellationToken = default)
        {
            string? externalIp = null;
            if (_memoryCache != null
                    && _memoryCache.TryGetValue(ExternalIpCacheId, out string? cachedExternalIp)
                    && !string.IsNullOrWhiteSpace(cachedExternalIp))
            {
                externalIp = cachedExternalIp;
            }
            else
            {
                var ip = await Utilities.GetExternalIp(_httpClient, cancellationToken);
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    externalIp = ip;
                    _memoryCache?.Set(ExternalIpCacheId, ip);
                }
            }
            return externalIp;
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, Uri uri)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, uri);
            request.Headers.Add("X-Auth-Email", ServiceSettings.Email);
            request.Headers.Add("X-Auth-Key", ServiceSettings.ApiKey);
            return request;
        }

        private Uri GetRecordsUri(string zoneId)
        {
            return new Uri($"{BaseUrl}zones/{zoneId}/dns_records");
        }

        private Uri GetUpdateRecordUri(string zoneId, string recordId)
        {
            return new Uri($"{BaseUrl}zones/{zoneId}/dns_records/{recordId}");
        }
        private Uri GetCreateRecordUri(string zoneId)
        {
            return new Uri($"{BaseUrl}zones/{zoneId}/dns_records");
        }
    }

    public sealed class CloudflareDnsResponse : IRequestResult
    {
        public ResultStatus Status { get; set; }

        public string Message { get; set; } = string.Empty;

        public string RawMessage { get; set; } = string.Empty;

        public CloudflareDnsResponse(string rawResponse)
        {
            RawMessage = rawResponse;
        }

        public CloudflareDnsResponse(CloudflareResult? result, string rawResponse)
            : this(rawResponse)
        {
            if (result == null)
            {
                Status = ResultStatus.Faulted;
                Message = "Cloudflare result was null";
            }
            else
            {
                Status = result.Success ? ResultStatus.Completed : ResultStatus.Faulted;
                if (result.Errors.Count > 0)
                {
                    Message = result.Errors.First().Message;
                }
            }
        }
    }
}

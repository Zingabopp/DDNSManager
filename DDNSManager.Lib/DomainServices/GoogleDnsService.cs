using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.DomainServiceConfiguration;
using DDNSManager.Lib.Services;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDNSManager.Lib.DomainServices
{
    public sealed class GoogleDnsService : IDDNSService<GoogleDnsSettings>
    {
        public const string ServiceId = "GoogleDns";
        private static readonly Uri PostUri = new Uri("https://domains.google.com/nic/update");
        private readonly HttpClient _httpClient;
        private readonly IDomainCheckService _domainCheckService;
        private readonly IIPCheckService _iPCheckService;
        private readonly ILogger<GoogleDnsService> _logger;

        string IDDNSService.ServiceId => ServiceId;
        IServiceSettings IDDNSService.Settings => ServiceSettings;
        public GoogleDnsSettings ServiceSettings { get; set; }
        public string ServiceName => "Google DDNS";

        public GoogleDnsService(HttpClient httpClient, GoogleDnsSettings settings, 
            IDomainCheckService domainCheckService, IIPCheckService iPCheckService,
            ILogger<GoogleDnsService> logger)
        {
            _httpClient = httpClient;
            if (settings == null)
                settings = new GoogleDnsSettings();
            ServiceSettings = settings;
            _domainCheckService = domainCheckService;
            _iPCheckService = iPCheckService;
            _logger = logger;
        }

        public async Task<DomainMatchResult> CheckDomainAsync(CancellationToken cancellationToken = default)
        {
            string? expectedIp = ServiceSettings.IP;
            if(string.IsNullOrWhiteSpace(expectedIp))
            {
                var ipResult = await _iPCheckService.GetCurrentIPAsync(cancellationToken).ConfigureAwait(false);
                if(ipResult.IsError)
                {
                    _logger.LogError("No expected IP was specified and failed to retrieve current IP address.");
                    return DomainMatchResult.Inconclusive;
                }
                expectedIp = ipResult.Value;
            }
            string hostname = ServiceSettings.Hostname ?? throw new InvalidOperationException("No hostname specified in settings.");
            return await _domainCheckService.CheckDomainAsync(expectedIp, hostname, cancellationToken);
        }

        public async Task<IRequestResult> SendRequestAsync(CancellationToken cancellationToken = default)
        {
            GoogleDnsSettings settings = ServiceSettings;
            if (!ServiceSettings.IsValid())
                throw new InvalidOperationException("Settings are not valid.");
            UriBuilder builder = new UriBuilder(PostUri)
            {
                Query = ServiceSettings.ToQuery()
            };
            Uri uri = builder.Uri;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            string authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);

            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new GoogleDnsResponse(responseContent);
        }
    }

    public sealed class GoogleDnsResponse : IRequestResult
    {
        public static readonly GoogleDnsResponse SkippedResult = new GoogleDnsResponse();
        private const string ResponseGood = "good";
        private const string ResponseNoChange = "nochg";
        private const string ResponseNoHost = "nohost";
        private const string ResponseBadAuth = "badauth";
        private const string ResponseNotFQDN = "notfqdn";
        private const string ResponseBadAgent = "badagent";
        private const string ResponseAbuse = "abuse";
        private const string ResponseError = "911";
        private const string ResponseConflict = "conflict";
        private GoogleDnsResponse()
        {
            Status = ResultStatus.Skipped;
            RawMessage = "Updated skipped";
        }
        public GoogleDnsResponse(string responseContent)
        {
            RawMessage = responseContent;
            if (responseContent.StartsWith(ResponseGood, StringComparison.OrdinalIgnoreCase))
                ResponseType = GoogleResponseType.Good;
            else if (responseContent.StartsWith(ResponseNoChange, StringComparison.OrdinalIgnoreCase))
                ResponseType = GoogleResponseType.NoChange;
            else if (responseContent.StartsWith(ResponseConflict, StringComparison.OrdinalIgnoreCase))
                ResponseType = GoogleResponseType.Conflict;
            else
            {
                ResponseType = responseContent switch
                {
                    ResponseNoHost => GoogleResponseType.NoHost,
                    ResponseBadAuth => GoogleResponseType.BadAuth,
                    ResponseNotFQDN => GoogleResponseType.NotFQDN,
                    ResponseBadAgent => GoogleResponseType.BadAgent,
                    ResponseAbuse => GoogleResponseType.Abuse,
                    ResponseError => GoogleResponseType.Error,
                    _ => GoogleResponseType.None
                };
                if (ResponseType == GoogleResponseType.None)
                    _message = $"Unrecognized response: {RawMessage}";
            }
            Status = ResponseType switch
            {
                GoogleResponseType.Good => ResultStatus.Completed,
                GoogleResponseType.NoChange => ResultStatus.Completed,
                GoogleResponseType.NoHost => ResultStatus.Faulted,
                GoogleResponseType.BadAuth => ResultStatus.Faulted,
                GoogleResponseType.NotFQDN => ResultStatus.Faulted,
                GoogleResponseType.BadAgent => ResultStatus.Faulted,
                GoogleResponseType.Abuse => ResultStatus.Faulted,
                GoogleResponseType.Error => ResultStatus.Faulted,
                GoogleResponseType.Conflict => ResultStatus.Faulted,
                _ => ResultStatus.None
            };
        }
        public GoogleResponseType ResponseType { get; }
        private readonly string? _message;
        public string Message => _message ?? RawMessage;
        public string RawMessage { get; }

        public ResultStatus Status { get; }
    }
    public enum GoogleResponseType
    {
        None,
        Good,
        NoChange,
        NoHost,
        BadAuth,
        NotFQDN,
        BadAgent,
        Abuse,
        Error,
        Conflict
    }
}

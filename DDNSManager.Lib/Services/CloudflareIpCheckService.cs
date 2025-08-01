using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace DDNSManager.Lib.Services;
public partial class CloudflareIpCheckService : IIPCheckService
{
    public const string Endpoint = "https://cloudflare.com/cdn-cgi/trace";
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    [GeneratedRegex(@"^ip\=((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex IpRegex();

    public string ServiceName => "Cloudflare";

    public CloudflareIpCheckService(HttpClient client, ILogger<CloudflareIpCheckService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static bool TryGetIp(string line, [NotNullWhen(true)] out string? ip)
    {
        if (line == null)
        {
            ip = null;
            return false;
        }
        if (string.IsNullOrWhiteSpace(line))
        {
            ip = null;
            return false;
        }
        Match match = IpRegex().Match(line);
        if (match.Success)
        {
            ip = match.Groups[1].Value;
            return true;
        }
        ip = null;
        return false;
    }

    public async Task<Result<string, ProblemDetails>> GetCurrentIPAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage? response = await _client.GetAsync(Endpoint, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using StreamReader reader = new StreamReader(contentStream);
                string? line;
                while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
                {
                    if (TryGetIp(line, out string? ip))
                    {
                        if(_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("Retrieved current IP from Cloudflare: {IP}", ip);
                        }
                        return new Result<string, ProblemDetails>(ip);
                    }
                }
                return new ProblemDetails
                {
                    Title = "Failed to parse IP from Cloudflare response",
                    Detail = "The response did not contain a valid 'ip' line."
                };
            }
            else
            {
                _logger.LogError("Failed to retrieve current IP from Cloudflare: {StatusCode}", response.StatusCode);
                return new ProblemDetails
                {
                    Title = "Failed to retrieve current IP",
                    Detail = $"Status code: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            return new ProblemDetails
            {
                Title = "Error retrieving current IP",
                Detail = ex.Message
            };
        }
    }
}

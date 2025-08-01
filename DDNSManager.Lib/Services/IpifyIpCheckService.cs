using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace DDNSManager.Lib.Services;
public partial class IpifyIpCheckService : IIPCheckService
{
    public const string Endpoint = "https://api.ipify.org/";
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public string ServiceName => "Ipify";
    public IpifyIpCheckService(HttpClient client, ILogger<IpifyIpCheckService> logger)
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
        Match match = Utilities.Ipv4Regex().Match(line);
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
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (TryGetIp(content, out string? ip))
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Retrieved current IP from Ipify: {IP}", ip);
                    }
                    return new Result<string, ProblemDetails>(ip);
                }
                return new ProblemDetails
                {
                    Title = "Failed to parse IP from Ipify response",
                    Detail = "The response did not contain a valid 'ip' line."
                };
            }
            else
            {
                _logger.LogError("Failed to retrieve current IP from Ipify: {StatusCode}", response.StatusCode);
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

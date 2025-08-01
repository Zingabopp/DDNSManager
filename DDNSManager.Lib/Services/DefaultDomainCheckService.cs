using System.Net;
using System.Net.Sockets;

namespace DDNSManager.Lib.Services;

public class DefaultDomainCheckService : IDomainCheckService
{
    private readonly ILogger<DefaultDomainCheckService> _logger;
    private const string HostNotFoundTitle = "Host Not Found";
    private const string NoAddressAssignedTitle = "No Address Assigned";
    public DefaultDomainCheckService(ILogger<DefaultDomainCheckService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DomainMatchResult> CheckDomainAsync(string expectedIp, string hostname, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(hostname, nameof(hostname));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(expectedIp, nameof(expectedIp));
        Result<string, ProblemDetails> hostIpResult = await GetHostIPAsync(hostname, cancellationToken).ConfigureAwait(false);
        return hostIpResult.Match(
            ip =>
            {
                if (ip == expectedIp)
                {
                    return DomainMatchResult.Match;
                }
                else
                {
                    return DomainMatchResult.NoMatch;
                }
            },
            problem =>
            {
                if (problem.Title == HostNotFoundTitle)
                {
                    return DomainMatchResult.HostNotFound;
                }
                else if (problem.Title == NoAddressAssignedTitle)
                {
                    return DomainMatchResult.NoMatch;
                }
                else
                {
                    return DomainMatchResult.Inconclusive;
                }
            });
    }

    public async Task<Result<string, ProblemDetails>> GetHostIPAsync(string hostname, CancellationToken cancellationToken)
    {
        try
        {
            IPAddress[]? addresses = await Dns.GetHostAddressesAsync(hostname, cancellationToken).ConfigureAwait(false);
            if (addresses != null && addresses.Length > 0)
            {
                if (addresses.Length > 1 && _logger.IsEnabled(LogLevel.Warning))
                {
                    // Log a warning if multiple addresses are found
                    // TODO: Support returning all addresses or a specific one based on preference
                    _logger.LogWarning("Multiple IP addresses found for hostname '{HostName}'. Using the first one.", hostname);
                }
                return addresses[0].ToString();
            }
            else
            {
                return new ProblemDetails
                {
                    Title = "No IP address found",
                    Detail = $"No IP address assigned to hostname '{hostname}'."
                };
            }
        }
        catch (SocketException ex)
        {
            SocketError error = ex.SocketErrorCode;
            ProblemDetails? problem = error switch
            {
                SocketError.HostNotFound => new ProblemDetails() { Title = HostNotFoundTitle, Detail = $"Hostname '{hostname}' could not be resolved." }, // Host not found
                SocketError.NoData => new ProblemDetails() { Title = NoAddressAssignedTitle, Detail = $"No address is assigned to hostname '{hostname}'" }, // No address assigned
                _ => null
            };
            if (problem is null)
            {
                _logger.LogError(ex, "An unexpected socket error occurred while resolving hostname '{HostName}'.", hostname);
                problem = new ProblemDetails() { Title = "Unknown Socket Error", Detail = $"An unknown socket error occurred: {ex.Message}" };
            }
            return problem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting the host IP for hostname '{HostName}'.", hostname);
            return new ProblemDetails() { Title = "Unknown Error", Detail = $"An unknown error occurred: {ex.Message}" };
        }
    }
}

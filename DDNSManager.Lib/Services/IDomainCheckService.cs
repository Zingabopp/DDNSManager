namespace DDNSManager.Lib.Services;

public interface IDomainCheckService
{
    /// <summary>
    /// Gets the IP address associated with the specified domain name.
    /// </summary>
    /// <param name="domainName">The domain name to check.</param>
    /// <returns>The IP address of the domain or an error message.</returns>
    Task<Result<string, ProblemDetails>> GetHostIPAsync(string domainName, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if the current IP address matches the expected IP address for the specified hostname.
    /// </summary>
    /// <param name="expectedIp">The expected IP address.</param>
    /// <param name="hostname">The hostname to check.</param>
    /// <returns>A result indicating whether the IP addresses match or an error message.</returns>
    Task<DomainMatchResult> CheckDomainAsync(string expectedIp, string hostname, CancellationToken cancellationToken);
}

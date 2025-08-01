namespace DDNSManager.Lib;

public enum DomainMatchResult
{
    /// <summary>
    /// Could not determine if there is a match.
    /// </summary>
    Inconclusive,
    /// <summary>
    /// No record of the hostname could be found.
    /// </summary>
    HostNotFound,
    /// <summary>
    /// Hostname is assigned to the expected IP.
    /// </summary>
    Match,
    /// <summary>
    /// Hostname is not assigned to the expected IP.
    /// </summary>
    NoMatch
}
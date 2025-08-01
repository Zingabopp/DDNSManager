using System.Threading.Tasks;

namespace DDNSManager.Lib.Services;
public interface IIPCheckService
{
    string ServiceName { get; }
    /// <summary>
    /// Gets the current external IP address of the device.
    /// </summary>
    /// <returns>Current external IP address as a string.</returns>
    Task<Result<string, ProblemDetails>> GetCurrentIPAsync(CancellationToken cancellationToken);
}

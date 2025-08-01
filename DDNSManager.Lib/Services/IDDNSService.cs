using DDNSManager.Lib.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace DDNSManager.Lib.Services
{
    public interface IDDNSService
    {
        string ServiceId { get; }
        string ServiceName { get; }
        IServiceSettings Settings { get; }
        Task<DomainMatchResult> CheckDomainAsync(CancellationToken cancellationToken = default);
        Task<IRequestResult> SendRequestAsync(CancellationToken cancellationToken = default);
    }

    public interface IDDNSService<TSettings> : IDDNSService
        where TSettings : class, new()
    {
        TSettings ServiceSettings { get; set; }
    }
}

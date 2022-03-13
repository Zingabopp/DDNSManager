using DDNSManager.Lib.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
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

    public interface IDDNSService<TSettings>
        where TSettings : class, new()
    {
        TSettings Settings { get; set; }
    }
}

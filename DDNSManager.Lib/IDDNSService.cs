using DDNSManager.Lib.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDNSManager.Lib
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

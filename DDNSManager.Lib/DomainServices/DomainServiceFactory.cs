using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DDNSManager.Lib.DomainServices;
public class DomainServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    public DomainServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    public IDDNSService<TSettings> CreateService<TSettings>(TSettings settings)
        where TSettings : class, IServiceSettings, new()
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        return ActivatorUtilities.CreateInstance<IDDNSService<TSettings>>(_serviceProvider, settings);

    }
}


using DDNSManager.Lib;
using DDNSManager.Lib.DomainServiceConfiguration;
using DDNSManager.Lib.DomainServices;
using DDNSManager.Lib.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DDNSManagerDependencyInjection
{
    /// <summary>
    /// Adds the DDNSManager services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDDNSCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IIPCheckService, CloudflareIpCheckService>();
        services.TryAddSingleton<IDomainCheckService, DefaultDomainCheckService>();
        services.TryAddSingleton<DomainServiceFactory>();
        //services.TryAddTransient<IDDNSService<CloudflareDnsSettings>, CloudflareDnsService>();
        services.TryAddScoped<SettingsContainer>();
        return services;
    }
}

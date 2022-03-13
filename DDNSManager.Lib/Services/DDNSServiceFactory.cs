using DDNSManager.Lib.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Text;

namespace DDNSManager.Lib.Services
{
    public class DDNSServiceFactory
    {
        private IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Func<IServiceProvider, IServiceSettings, IDDNSService>> serviceFactories 
            = new Dictionary<string, Func<IServiceProvider, IServiceSettings, IDDNSService>>()
            {
                { GoogleDnsService.ServiceId, (p, s) => new GoogleDnsService((HttpClient)p.GetService(typeof(HttpClient)), s) }
            };
        private readonly Dictionary<string, Func<IServiceSettings>> serviceSettingsFactories
            = new Dictionary<string, Func<IServiceSettings>>()
            {
                { GoogleDnsService.ServiceId, () => new GoogleDnsSettings() }
            };
        public DDNSServiceFactory(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        public IEnumerable<string> GetRegisteredServiceIds => serviceFactories.Keys;

        public void RegisterService(string serviceId, Func<IServiceProvider, IServiceSettings, IDDNSService> serviceFactory,
            Func<IServiceSettings> emptySettingsFactory)
        {
            if(string.IsNullOrWhiteSpace(serviceId))
                throw new ArgumentNullException(nameof(serviceId));
            serviceFactories[serviceId] = serviceFactory;
            serviceSettingsFactories[serviceId] = emptySettingsFactory;
        }

        public IDDNSService GetService(string serviceId, IServiceSettings settings)
        {
            if (serviceFactories.TryGetValue(serviceId, out var serviceFactory))
            {
                return serviceFactory(_serviceProvider, settings);
            }
            else
                throw new ArgumentException($"Could not find service with Id '{serviceId}'", nameof(serviceId));
        }

        public IServiceSettings CreateSettingsForServiceId(string serviceId)
        {
            if (serviceSettingsFactories.TryGetValue(serviceId, out var settingFactory))
{
                return settingFactory();
            }
            else
                throw new ArgumentException($"Could not find service with Id '{serviceId}'", nameof(serviceId));
        }
    }
}

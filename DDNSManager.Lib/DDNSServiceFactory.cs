using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.ServiceConfiguration;
using DDNSManager.Lib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Text;

namespace DDNSManager.Lib
{
    public class DDNSServiceFactory
    {
        private IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Func<IServiceProvider, IServiceSettings, IDDNSService>> serviceFactories 
            = new Dictionary<string, Func<IServiceProvider, IServiceSettings, IDDNSService>>();

        private readonly Dictionary<string, Func<IServiceSettings>> serviceSettingsFactories;
        public DDNSServiceFactory(IServiceProvider provider)
        {
            _serviceProvider = provider;
            serviceFactories 
                = new Dictionary<string, Func<IServiceProvider, IServiceSettings, IDDNSService>>(DefaultServiceRegistration.serviceFactories);
            serviceSettingsFactories 
                = new Dictionary<string, Func<IServiceSettings>>(DefaultServiceRegistration.serviceSettingsFactories);
        }

        public void ClearProviders()
        {
            serviceFactories.Clear();
            serviceSettingsFactories.Clear();
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

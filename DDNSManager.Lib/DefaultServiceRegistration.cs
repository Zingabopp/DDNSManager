﻿using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.DomainServiceConfiguration;
using DDNSManager.Lib.DomainServices;
using DDNSManager.Lib.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DDNSManager.Lib;

public static class DefaultServiceRegistration
{
    #region Add new services here
    /// <summary>
    /// This dictionary is used by the <see cref="ServiceSettingsConverter"/>. 
    /// Services must be added here directly or by <see cref="RegisterSettingsType(string, Type)"/> or deserialization will not work.
    /// </summary>
    internal static readonly Dictionary<string, Type> settingsTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { GoogleDnsService.ServiceId, typeof(GoogleDnsSettings) },
            { CloudflareDnsService.ServiceId, typeof(CloudflareDnsSettings) }
        };

    /// <summary>
    /// Instances of <see cref="DDNSServiceFactory"/> will get their initial service factory functions here.
    /// </summary>
    internal static readonly Dictionary<string, Func<IServiceProvider, IServiceSettings, IDDNSService>> serviceFactories
        = new Dictionary<string, Func<IServiceProvider, IServiceSettings, IDDNSService>>()
        {
            { GoogleDnsService.ServiceId, (p, s) => ActivatorUtilities.CreateInstance<GoogleDnsService>(p, (GoogleDnsSettings)s) },
            { CloudflareDnsService.ServiceId, (p, s) => ActivatorUtilities.CreateInstance<CloudflareDnsService>(p, (CloudflareDnsSettings)s) }
        };

    /// <summary>
    /// Instances of <see cref="DDNSServiceFactory"/> will get their initial service settings factory functions here.
    /// </summary>
    internal static readonly Dictionary<string, Func<IServiceSettings>> serviceSettingsFactories
        = new Dictionary<string, Func<IServiceSettings>>()
        {
            { GoogleDnsService.ServiceId, () => new GoogleDnsSettings() },
            { CloudflareDnsService.ServiceId, () => new CloudflareDnsSettings() }

        };
    #endregion

    public static IEnumerable<string> RegisteredServices => serviceSettingsFactories.Keys;
    public static bool TryGetSettingsType(string serviceId,
        [MaybeNullWhen(false)] out Type settingsType)
    {
        return settingsTypeMap.TryGetValue(serviceId, out settingsType);
    }

    public static IDDNSService GetService(IServiceProvider serviceProvider, string serviceId, IServiceSettings settings)
    {
        if (serviceFactories.TryGetValue(serviceId, out Func<IServiceProvider, IServiceSettings, IDDNSService>? serviceFactory))
        {
            return serviceFactory(serviceProvider, settings);
        }
        else
            throw new ArgumentException($"Could not find service with Id '{serviceId}'", nameof(serviceId));
    }

    public static IServiceSettings CreateSettingsForServiceId(string serviceId)
    {
        if (serviceSettingsFactories.TryGetValue(serviceId, out Func<IServiceSettings>? settingFactory))
        {
            return settingFactory();
        }
        else
            throw new ArgumentException($"Could not find service with Id '{serviceId}'", nameof(serviceId));
    }

    public static void RegisterService<TService, TSettings>(string serviceId,
        Func<IServiceProvider, IServiceSettings, TService> serviceFactory,
        Func<TSettings> settingsFactory)
        where TService : class, IDDNSService
        where TSettings : class, IServiceSettings
    {
        settingsTypeMap[serviceId] = typeof(TSettings);
        serviceFactories[serviceId] = serviceFactory;
        serviceSettingsFactories[serviceId] = settingsFactory;

    }

    /// <summary>
    /// Use <see cref="RegisterService{TService, TSettings}(string, Func{IServiceProvider, IServiceSettings, TService}, Func{TSettings})"/> instead.
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="type"></param>
    public static void RegisterSettingsType(string serviceId, Type type)
    {
        settingsTypeMap[serviceId] = type;
    }
}

using DDNSManager.Lib;
using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.Services;
using System.Text.Json;

namespace DDNSManager.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DDNSServiceFactory _factory;
        private readonly HttpClient _httpClient;
        private ManagerSettings _settings;
        public Worker(ILogger<Worker> logger, DDNSServiceFactory _serviceFactory, HttpClient httpClient, ManagerSettings settings)
        {
            _logger = logger;
            _factory = _serviceFactory;
            _httpClient = httpClient;
            _settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string? settingsPath = _settings.SettingsPath;
            _logger.LogInformation(new EventId(2, "Starting"), $"DDNS Manager starting, using config at {settingsPath}.");
            while (!stoppingToken.IsCancellationRequested)
            {
                if (File.Exists(settingsPath))
                {
                    ManagerSettings? oldSettings = _settings;
                    try
                    {
                        using FileStream file = File.OpenRead(settingsPath);
                        ManagerSettings? newSettings = await JsonSerializer
                            .DeserializeAsync<ManagerSettings>(file, Utilities.DefaultJsonOptions, stoppingToken)
                            .ConfigureAwait(false);
                        if (newSettings != null)
                        {
                            _settings = newSettings;
                            _settings.SettingsPath = settingsPath;
                        }
                        else
                            _logger.LogWarning(new EventId(2001, "SettingsException"),
                                $"Error reading settings file at '{_settings.SettingsPath}': Deserialized to null");

                    }
                    catch (Exception ex)
                    {
                        string message = $"Error reading settings file at '{_settings.SettingsPath}': {ex.Message}";
                        _logger.LogWarning(new EventId(2001, "SettingsException"), message);
                        _settings = oldSettings;
                    }
                }

                string? currentIp = await Utilities.GetExternalIp(_httpClient, stoppingToken).ConfigureAwait(false);
                if (currentIp != null)
                    _logger.LogInformation(new EventId(1, "Started"), $"DDNS Manager running, current IP: {currentIp}, enabled profiles: {_settings.EnabledProfiles}");
                else
                {
                    _logger.LogInformation(new EventId(1, "Started"), $"DDNS Manager running, unable to get current IP, enabled profiles: {_settings.EnabledProfiles}.");
                }

                foreach (IServiceSettings? serviceSetting in _settings.ServiceSettings.Where(s => s.Enabled))
                {
                    _logger.LogDebug($"Starting update for '{serviceSetting.Name}'");
                    if (!serviceSetting.IsValid())
                    {
                        _logger.LogWarning($"Configuration for '{serviceSetting.Name}' is invalid, disabling...");
                        serviceSetting.Enabled = false;
                        continue;
                    }
                    var service = _factory.GetService(serviceSetting.ServiceId, serviceSetting);
                    DomainMatchResult matchResult = await service.CheckDomainAsync(stoppingToken).ConfigureAwait(false);
                    if(matchResult == DomainMatchResult.HostNotFound)
                    {
                        _logger.LogWarning($"A record for '{serviceSetting.Hostname}' was not found, disabling and skipping.");
                        serviceSetting.Enabled = false;
                        continue;
                    }
                    bool updateRequired = matchResult == DomainMatchResult.Match ? false : true;
                    if (updateRequired)
                    {
                        _logger.LogDebug($"Sending update request for {serviceSetting.Name}...");
                        var result = await service.SendRequestAsync(stoppingToken).ConfigureAwait(false);
                        if (result.Status == ResultStatus.Completed)
                        {
                            _logger.LogDebug($"Update completed successfully");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to update DNS record {serviceSetting.Hostname}: {result.Message}");
                        }
                        await Task.Delay(2000, stoppingToken).ConfigureAwait(false);
                    }
                    else
                        _logger.LogDebug($"Update not required for '{serviceSetting.Name}'");
                }
                if (settingsPath != null)
                    File.WriteAllText(settingsPath, JsonSerializer.Serialize(_settings, Utilities.DefaultJsonOptions));

                TimeSpan interval = _settings.Interval.ToTimeSpan();
                _logger.LogDebug($"Sleeping for {interval}");
                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
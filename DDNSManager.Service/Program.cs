using CommandLine;
using DDNSManager.Lib;
using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.Services;
using System.Reflection;
using System.Text.Json;
namespace DDNSManager.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "DDNS Manager";
                })
                .ConfigureLogging((hostBuilderContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddEventLog(s =>
                    {
                        s.LogName = "Application";
                        s.SourceName = "DDNS Manager";
                    });
                    logging.AddFileLogger(options =>
                    {
                        hostBuilderContext.Configuration.GetSection("Logging").GetSection("FileLogger").GetSection("Options").Bind(options);
                    });

                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<CommandLineOptions>(p => GetCommandLineOptions(p, args));
                    services.AddTransient<ManagerSettings>(GetManagerSettings);
                    services.AddSingleton<HttpClient>(GetHttpClient);
                    services.AddSingleton<DDNSServiceFactory>();
                    services.AddHostedService<Worker>();
                })
                .Build();
            try
            {
                await host.RunAsync();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                if (Environment.ExitCode != 0)
                    return;
                else
                    Console.WriteLine(ex);
            }
        }

        private static HttpClient GetHttpClient(IServiceProvider _)
        {
            HttpClient? client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"DDNSManager/{Assembly.GetExecutingAssembly().GetName().Version}");
            return client;
        }

        private static CommandLineOptions GetCommandLineOptions(IServiceProvider _, string[] args)
        {
            CommandLineOptions options = null!;
            Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(o =>
            {
                options = o;
            });
            return options;
        }

        private static ManagerSettings GetManagerSettings(IServiceProvider p)
        {
            ILogger<ManagerSettings>? logger = p.GetRequiredService<ILogger<ManagerSettings>>();
            CommandLineOptions? args = p.GetRequiredService<CommandLineOptions>();
            string settingsPath = args.SettingsPath!;
            try
            {
                if (string.IsNullOrWhiteSpace(settingsPath))
                    throw new ArgumentException("No settings path specified (use command line arg '-c <PATH>')");
                settingsPath = Path.GetFullPath(settingsPath);
                string? directoryName = Path.GetDirectoryName(settingsPath);

                if (File.Exists(settingsPath))
                {
                    ManagerSettings settings = new ManagerSettings() { SettingsPath = settingsPath };
                    logger.LogDebug($"Read settings file from '{settingsPath}' with {settings.EnabledProfiles} enabled profiles.");
                    return settings;
                }
                else if (Directory.Exists(directoryName))
                {
                    ManagerSettings settings = new ManagerSettings() { SettingsPath = Path.GetDirectoryName(settingsPath) };
                    try
                    {
                        File.WriteAllText(settingsPath, JsonSerializer.Serialize(settings, Utilities.DefaultJsonOptions));
                        logger.LogWarning($"Settings file at '{settingsPath}' did not exist and was created");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Settings file at '{settingsPath}' did not exist and could not be created: {ex.Message}");
                    }
                    return settings;
                }
                else
                    throw new ArgumentException($"Settings file path is not in a valid directory: '{settingsPath}'");
            }
            catch (Exception ex)
            {
                logger.LogError(new EventId(2002, "SettingsException"), $"Error reading settings from '{settingsPath}': {ex.Message}");
                throw;
            }
        }
    }


}


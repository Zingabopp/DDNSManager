using CommandLine;

namespace DDNSManager.Lib.Configuration
{
    public class CommandLineOptions
    {
        [Option(shortName: 'c', longName: "config", HelpText = "Path to configuration file.", Required = true)]
        public string? SettingsPath { get; set; }
        [Option(shortName: 'l', longName: "log-path", HelpText = "Path to the text log file.")]
        public string? LoggingPath { get; set; }
    }
}

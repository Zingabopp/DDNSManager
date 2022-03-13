using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.Services;
using System.Collections.Generic;

namespace DDNSManager.Lib.ServiceConfiguration
{
    public class GoogleDnsSettings : IServiceSettings
    {
        public string ServiceId => GoogleDnsService.ServiceId;
        private string? _name;
        public string? Name
        {
            get => _name ?? Hostname;
            set => _name = value;
        }

        public string? Hostname { get; set; }
        public string? IP { get; set; }
        public bool Enabled { get; set; }

        public string? Username { get; set; }
        public string? Password { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Hostname)
                && !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        public string ToQuery()
        {
            return Utilities.BuildQuery(
                new KeyValuePair<string, string?>("hostname", Hostname),
                new KeyValuePair<string, string?>("myip", IP),
                new KeyValuePair<string, string?>("offline", null));
        }

        public bool Equals(IServiceSettings? obj)
        {
            return obj is GoogleDnsSettings settings &&
                   ServiceId == settings.ServiceId &&
                   Name == settings.Name &&
                   Hostname == settings.Hostname &&
                   IP == settings.IP &&
                   Enabled == settings.Enabled &&
                   Username == settings.Username &&
                   Password == settings.Password;
        }
    }
}

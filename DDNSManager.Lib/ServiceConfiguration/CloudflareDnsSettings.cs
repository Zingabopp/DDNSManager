using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.Services;
using System;

namespace DDNSManager.Lib.ServiceConfiguration
{
    public class CloudflareDnsSettings : IServiceSettings
    {
        public string ServiceId => CloudflareDnsService.ServiceId;
        private string? _name;
        public string? Name
        {
            get => _name ?? Hostname;
            set => _name = value;
        }

        public bool Enabled { get; set; }
        public string? Hostname { get; set; }
        public string? IP { get; set; }
        public bool Proxied { get; set; }
        public bool AllowCreate { get; set; }

        public string ZoneId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ApiKey { get; set; } = null!;

        public bool Equals(IServiceSettings other)
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Hostname)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(ApiKey)
                && !string.IsNullOrWhiteSpace(ZoneId);
        }
    }
}

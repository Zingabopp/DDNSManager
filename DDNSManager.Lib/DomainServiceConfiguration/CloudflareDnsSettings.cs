using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.DomainServices;
using System;

namespace DDNSManager.Lib.DomainServiceConfiguration
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
        public string? Hostname { get; set; }
        public string? IP { get; set; }
        public bool Enabled { get; set; }
        public bool AllowCreate { get; set; }

        public bool Proxied { get; set; }
        public string ZoneId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ApiKey { get; set; } = null!;

        public bool Equals(IServiceSettings? other)
        {
            if (other == null) return false;

            if(other is not CloudflareDnsSettings cfSettings)
                return false;

            return ServiceId == cfSettings.ServiceId
                && Name == cfSettings.Name
                && Hostname == cfSettings.Hostname
                && IP == cfSettings.IP
                && Enabled == cfSettings.Enabled
                && AllowCreate == cfSettings.AllowCreate
                && Proxied == cfSettings.Proxied
                && ZoneId == cfSettings.ZoneId
                && Email == cfSettings.Email
                && ApiKey == cfSettings.ApiKey;
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

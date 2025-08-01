﻿using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.DomainServices;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DDNSManager.Lib.DomainServiceConfiguration
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
        [JsonIgnore]
        bool IServiceSettings.AllowCreate => false;

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

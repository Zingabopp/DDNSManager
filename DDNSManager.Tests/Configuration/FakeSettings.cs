using DDNSManager.Lib.Configuration;

namespace DDNSManager.Tests.Configuration
{
    public class FakeSettings : IServiceSettings
    {
        public string ServiceId => "FakeService";

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
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FakeSetting1 { get; set; }
        public int FakeSetting2 { get; set; }
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Hostname)
                && !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        public bool Equals(IServiceSettings? other)
        {
            return other is FakeSettings settings &&
                   ServiceId == settings.ServiceId &&
                   Name == settings.Name &&
                   Hostname == settings.Hostname &&
                   IP == settings.IP &&
                   Enabled == settings.Enabled &&
                   AllowCreate == settings.AllowCreate &&
                   Username == settings.Username &&
                   Password == settings.Password &&
                   FakeSetting1 == settings.FakeSetting1 &&
                   FakeSetting2 == settings.FakeSetting2;
        }
    }
}

using DDNSManager.Lib;
using DDNSManager.Lib.Configuration;
using DDNSManager.Lib.ServiceConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;

namespace DDNSManager.Tests.Configuration
{
    [TestClass]
    public class Serializing
    {
        private static readonly IServiceSettings[] SettingsArray = new IServiceSettings[]
        {
            new GoogleDnsSettings() { Name = "Google 1", Hostname = "google1.google.com", Enabled = true,
                                        IP = null, Username = "Goog1", Password = "elgoog1" },
            new FakeSettings() { Name = "Fake 1", Hostname = "fake1.fake.com", Enabled = true,
                                IP = "50.40.30.20", Username = "fake1", Password = "1ekaf",
                                FakeSetting1 = "fake_setting", FakeSetting2 = 5 },
            new GoogleDnsSettings() { Name = "Google 2", Hostname = "google2.google.com", Enabled = true,
                                        IP = "9.8.7.6", Username = "Goog2", Password = "elgoog2" },
        };
        [TestInitialize]
        public void Initialize()
        {
            DefaultServiceRegistration.RegisterSettingsType("FakeService", typeof(FakeSettings));
        }

        [TestMethod]
        public void Serialize()
        {
            int expectedMinutes = 10;
            int expectedHours = 5;
            int expectedDays = 1;
            ManagerSettings? settings = new ManagerSettings(new Interval(expectedMinutes, expectedHours, expectedDays), SettingsArray);
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(settings, options);
            Console.WriteLine(json);

            ManagerSettings? newSettings = JsonSerializer.Deserialize<ManagerSettings>(json);
            Assert.IsNotNull(newSettings);
            Assert.AreEqual(3, newSettings.ServiceSettings.Count);
            Assert.IsTrue(settings.Equals(newSettings));
        }
    }
}

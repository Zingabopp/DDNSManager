using DDNSManager.Lib.DomainServiceConfiguration;
using DDNSManager.Lib.DomainServices;
using DDNSManager.Lib.DomainServices.Responses.Cloudflare;
using DDNSManager.Lib.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DDNSManager.Tests.DomainServices
{
    [TestClass]
    public class CloudflareDnsServiceTests
    {
        private static CloudflareDnsSettings GetDnsSettings() => new CloudflareDnsSettings()
        {
            Email = "",
            ApiKey = "",
            ZoneId = ""
        };
        private static HttpClient HttpClient => new HttpClient();
        [TestMethod]
        public async Task GetRecords()
        {
            var settings = GetDnsSettings();
            var service = new CloudflareDnsService(HttpClient, settings, null!, 
                new DefaultDomainCheckService(new ConsoleLogger<DefaultDomainCheckService>()), new IpifyIpCheckService(HttpClient, new ConsoleLogger<IpifyIpCheckService>()),
                new ConsoleLogger<CloudflareDnsService>());
            try
            {
                var records = await service.GetRecordsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        public async Task GetRecordsDeserialization()
        {
            string raw = await File.ReadAllTextAsync("Data/CfGetRecordsResponse1.json");
            var data = JsonSerializer.Deserialize<CloudflareGetRecordsResponse>(raw, CloudflareDnsService.JsonSerializerOptions);

        }
    }
}

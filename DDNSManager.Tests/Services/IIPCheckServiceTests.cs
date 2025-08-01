using DDNSManager.Lib.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DDNSManager.Tests.Services;
[TestClass]
public class IIPCheckServiceTests
{
    private readonly ConsoleLogger ConsoleLogger;
    private readonly HttpClient HttpClient;
    private const string IpifyKey = "Ipify";
    private const string CloudflareKey = "Cloudflare";

    private readonly Dictionary<string, IIPCheckService> _services;

    public IIPCheckServiceTests()
    {
        ConsoleLogger = new ConsoleLogger();
        HttpClient = new HttpClient();
        _services = new Dictionary<string, IIPCheckService>
        {
            { IpifyKey, new IpifyIpCheckService(HttpClient, new ConsoleLogger<IpifyIpCheckService>()) },
            { CloudflareKey, new CloudflareIpCheckService(HttpClient, new ConsoleLogger<CloudflareIpCheckService>()) }
        };
    }

    [TestMethod]
    [DataRow("Ipify")]
    [DataRow("Cloudflare")]
    public async Task GetCurrentIpAsync_ReturnsIp(string serviceName)
    {
        // Arrange
        IIPCheckService service = _services[serviceName];

        // Act
        Lib.Result<string, Lib.ProblemDetails> result = await service.GetCurrentIPAsync(CancellationToken.None);


        // Assert
        result.Match(
            ip => ConsoleLogger.LogInformation($"Service {serviceName} retrieved IP: {ip}"),
            error => Assert.Fail($"Service {serviceName} failed to retrieve IP: {error.Title} - {error.Detail}")
        );
        Assert.IsFalse(result.IsError, $"Service {serviceName} failed to retrieve IP.");
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DDNSManager.Lib
{
    public static class Utilities
    {
        public static readonly JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        public static string BuildQuery(params KeyValuePair<string, string?>[] pairs)
        {
            if (pairs == null || pairs.Length == 0)
                return string.Empty;
            bool first = true;
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string?> pair in pairs)
            {
                string? value = pair.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (first)
                    {
                        sb.Append("?");
                        first = false;
                    }
                    else
                        sb.Append("&");
                    sb.Append(pair.Key);
                    sb.Append("=");
                    sb.Append(value!.Trim());
                }
            }
            return sb.ToString();
        }

        public static async Task<DomainMatchResult> CheckDomainAsync(HttpClient httpClient, string? expectedIp, string hostname, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                throw new InvalidOperationException("No hostname specified in settings.");
            if (string.IsNullOrWhiteSpace(expectedIp))
                expectedIp = await GetExternalIp(httpClient, cancellationToken);
            if (expectedIp == null || expectedIp.Length == 0)
                return DomainMatchResult.Inconclusive;
            // throw new Exception("An IP was not specified in settings and could not find external IP");
            try
            {
                string? actualIp = await GetDomainIp(hostname);
                if (actualIp == null)
                    return DomainMatchResult.NoMatch;
                if (actualIp.Length == 0)
                    return DomainMatchResult.Inconclusive;
                // throw new Exception($"Could not determine IP of hostname '{hostname}'");
                return actualIp.Equals(expectedIp) ? DomainMatchResult.Match : DomainMatchResult.NoMatch;
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.HostNotFound)
                    return DomainMatchResult.HostNotFound;
                throw;
            }
        }

        public static async Task<string?> GetExternalIp(HttpClient client, CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage? response = await client.GetAsync("https://domains.google.com/checkip", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                string ip = await response.Content.ReadAsStringAsync();
                return ip;
            }
            return null;
        }
        /// <summary>
        /// Attempts to get the IP address currently assigned to the given <paramref name="hostname"/>.
        /// Returns the IP address if found, null if no IP is assigned to the hostname, or an empty string if some other error occurred.
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        /// <exception cref="SocketException"></exception>
        public static async Task<string?> GetDomainIp(string hostname)
        {
            try
            {
                IPAddress[]? addresses = await Dns.GetHostAddressesAsync(hostname).ConfigureAwait(false);
                if (addresses != null && addresses.Length > 0)
                    return addresses[0].ToString();
            }
            catch (SocketException ex)
            {
                SocketError error = ex.SocketErrorCode;
                return error switch
                {
                    SocketError.NoData => null, // No address assigned
                    _ => throw ex
                };
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }
    }
    public enum DomainMatchResult
    {
        /// <summary>
        /// Could not determine if there is a match.
        /// </summary>
        Inconclusive,
        /// <summary>
        /// No record of the hostname could be found.
        /// </summary>
        HostNotFound,
        /// <summary>
        /// Hostname is assigned to the expected IP.
        /// </summary>
        Match,
        /// <summary>
        /// Hostname is not assigned to the expected IP.
        /// </summary>
        NoMatch
    }
}

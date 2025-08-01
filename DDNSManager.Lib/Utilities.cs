using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DDNSManager.Lib;

public static partial class Utilities
{
    [GeneratedRegex(@"^((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    public static partial Regex Ipv4Regex();

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
}
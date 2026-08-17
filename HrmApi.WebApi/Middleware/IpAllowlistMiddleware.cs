using System.Net;
using System.Net.Sockets;
using HrmApi.Application.Common.Interfaces;

namespace HrmApi.WebApi.Middleware
{
    public class IpAllowlistMiddleware
    {
        private readonly RequestDelegate _next;

        public IpAllowlistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IIpAllowlistCache ipAllowlistCache)
        {
            string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (IsLocalhost(context.Connection.RemoteIpAddress))
            {
                await _next(context);
                return;
            }

            IReadOnlyList<string> entries = await ipAllowlistCache.GetActiveEntriesAsync(context.RequestAborted);

            if (entries.Count == 0)
            {
                await _next(context);
                return;
            }

            if (string.IsNullOrWhiteSpace(remoteIp) || !entries.Any(e => IpMatches(remoteIp, e)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("IP address is not allowed.");
                return;
            }

            await _next(context);
        }

        private static bool IsLocalhost(IPAddress? ip)
        {
            if (ip == null) return false;
            if (IPAddress.IsLoopback(ip)) return true;
            string s = ip.ToString();
            return s is "127.0.0.1" or "::1" or "::ffff:127.0.0.1";
        }

        internal static bool IpMatches(string remoteIp, string cidrOrIp)
        {
            string rule = cidrOrIp.Trim();
            if (string.IsNullOrEmpty(rule)) return false;

            if (!rule.Contains('/'))
            {
                return string.Equals(remoteIp, rule, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(NormalizeIp(remoteIp), NormalizeIp(rule), StringComparison.OrdinalIgnoreCase);
            }

            string[] parts = rule.Split('/', 2);
            if (parts.Length != 2
                || !IPAddress.TryParse(parts[0], out IPAddress? network)
                || !int.TryParse(parts[1], out int prefix)
                || !IPAddress.TryParse(remoteIp, out IPAddress? client))
            {
                return false;
            }

            if (client.AddressFamily == AddressFamily.InterNetworkV6 && client.IsIPv4MappedToIPv6)
                client = client.MapToIPv4();
            if (network.AddressFamily == AddressFamily.InterNetworkV6 && network.IsIPv4MappedToIPv6)
                network = network.MapToIPv4();

            if (client.AddressFamily != network.AddressFamily) return false;

            byte[] clientBytes = client.GetAddressBytes();
            byte[] networkBytes = network.GetAddressBytes();
            int maxBits = clientBytes.Length * 8;
            if (prefix < 0 || prefix > maxBits) return false;

            int fullBytes = prefix / 8;
            int remBits = prefix % 8;
            for (int i = 0; i < fullBytes; i++)
            {
                if (clientBytes[i] != networkBytes[i]) return false;
            }

            if (remBits == 0) return true;
            int mask = (byte)~(0xFF >> remBits);
            return (clientBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
        }

        private static string NormalizeIp(string ip)
        {
            if (IPAddress.TryParse(ip, out IPAddress? addr))
            {
                if (addr.IsIPv4MappedToIPv6) return addr.MapToIPv4().ToString();
                return addr.ToString();
            }
            return ip;
        }
    }
}

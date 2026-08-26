using System.Collections.Concurrent;
using Microsoft.IdentityModel.Tokens;

namespace HrmApi.Infrastructure.Security;

public static class JwksKeyResolver
{
    private static readonly ConcurrentDictionary<string, (DateTime CachedAt, ICollection<SecurityKey> Keys)> _cache = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    public static IEnumerable<SecurityKey> ResolveSigningKeys(string jwksUrl, string? kid)
    {
        if (string.IsNullOrWhiteSpace(jwksUrl))
        {
            return Enumerable.Empty<SecurityKey>();
        }

        if (_cache.TryGetValue(jwksUrl, out var cached) && DateTime.UtcNow - cached.CachedAt < CacheDuration)
        {
            if (string.IsNullOrWhiteSpace(kid))
            {
                return cached.Keys;
            }
            var matched = cached.Keys.Where(k => string.Equals(k.KeyId, kid, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matched.Count > 0)
            {
                return matched;
            }
        }

        try
        {
            var json = _httpClient.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
            if (!string.IsNullOrWhiteSpace(json))
            {
                var jwks = new JsonWebKeySet(json);
                var keys = jwks.GetSigningKeys();
                _cache[jwksUrl] = (DateTime.UtcNow, keys);

                if (string.IsNullOrWhiteSpace(kid))
                {
                    return keys;
                }
                return keys.Where(k => string.Equals(k.KeyId, kid, StringComparison.OrdinalIgnoreCase));
            }
        }
        catch
        {
            // If network failure occurs but we have existing cached keys, use them as fallback
            if (_cache.TryGetValue(jwksUrl, out var fallback))
            {
                return fallback.Keys;
            }
        }

        return Enumerable.Empty<SecurityKey>();
    }
}

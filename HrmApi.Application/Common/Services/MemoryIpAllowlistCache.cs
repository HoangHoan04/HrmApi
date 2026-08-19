using HrmApi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace HrmApi.Application.Common.Services
{
    public sealed class MemoryIpAllowlistCache : IIpAllowlistCache
    {
        private const string CacheKey = "auth:ip-allowlist:active";
        private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(45);

        private readonly IMemoryCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;

        public MemoryIpAllowlistCache(IMemoryCache cache, IServiceScopeFactory scopeFactory)
        {
            _cache = cache;
            _scopeFactory = scopeFactory;
        }

        public async Task<IReadOnlyList<string>> GetActiveEntriesAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(CacheKey, out List<string>? cached) && cached != null)
            {
                return cached;
            }

            using IServiceScope scope = _scopeFactory.CreateScope();
            IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            List<string> entries = await db.IpAllowlistEntryEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .Select(x => x.CidrOrIp)
                .ToListAsync(cancellationToken);

            _ = _cache.Set(
                CacheKey,
                entries,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl });

            return entries;
        }

        public void Invalidate()
        {
            _cache.Remove(CacheKey);
        }
    }
}

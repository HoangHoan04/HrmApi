using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace HrmApi.Application.Common.Services
{
    public sealed class MemoryPermissionCache : IPermissionCache
    {
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);
        private readonly IMemoryCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;

        public MemoryPermissionCache(IMemoryCache cache, IServiceScopeFactory scopeFactory)
        {
            _cache = cache;
            _scopeFactory = scopeFactory;
        }

        private static string Key(Guid userId) => $"auth:perm:{userId:N}";

        public bool TryGet(Guid userId, out AuthContextDto? context)
        {
            if (_cache.TryGetValue(Key(userId), out AuthContextDto? cached) && cached != null)
            {
                context = new AuthContextDto
                {
                    Roles = [.. cached.Roles],
                    Permissions = [.. cached.Permissions],
                };
                return true;
            }

            context = null;
            return false;
        }

        public void Set(Guid userId, AuthContextDto context)
        {
            var copy = new AuthContextDto
            {
                Roles = [.. context.Roles],
                Permissions = [.. context.Permissions],
            };
            _cache.Set(
                Key(userId),
                copy,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = DefaultTtl,
                });
        }

        public void InvalidateUser(Guid userId) => _cache.Remove(Key(userId));

        public async Task InvalidateByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var userIds = await db.UserRoleEntities.AsNoTracking()
                .Where(x => x.RoleId == roleId && !x.IsDeleted)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (Guid userId in userIds)
                InvalidateUser(userId);
        }

        public void InvalidateAll()
        {
            if (_cache is MemoryCache mc)
                mc.Compact(1.0);
        }
    }
}

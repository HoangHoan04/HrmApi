using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Auth;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Common.Services
{
    public class AuthContextService : IAuthContextService
    {
        private readonly IApplicationDbContext _context;
        private readonly IPermissionCache _permissionCache;

        public AuthContextService(IApplicationDbContext context, IPermissionCache permissionCache)
        {
            _context = context;
            _permissionCache = permissionCache;
        }

        public async Task<AuthContextDto> LoadAuthContextAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (_permissionCache.TryGet(userId, out AuthContextDto? cached) && cached != null)
            {
                return cached;
            }

            AuthContextDto loaded = await LoadFromDatabaseAsync(userId, cancellationToken);
            _permissionCache.Set(userId, loaded);
            return loaded;
        }

        private async Task<AuthContextDto> LoadFromDatabaseAsync(Guid userId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var userType = await _context.UserEntities.AsNoTracking()
                .Where(x => x.Id == userId && !x.IsDeleted)
                .Select(x => x.Type)
                .FirstOrDefaultAsync(cancellationToken);

            var roleRows = await (
                from ur in _context.UserRoleEntities.AsNoTracking()
                join r in _context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                where ur.UserId == userId
                      && !ur.IsDeleted
                      && !r.IsDeleted
                      && r.IsActive
                      && (ur.EffectiveFrom == null || ur.EffectiveFrom <= now)
                      && (ur.EffectiveTo == null || ur.EffectiveTo >= now)
                select new { r.Id, r.Code }
            ).ToListAsync(cancellationToken);

            var roles = roleRows
                .Select(x => x.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var roleIds = roleRows.Select(x => x.Id).Distinct().ToList();
            List<string> permissions = [];

            var isAdmin =
                string.Equals(userType, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase)
                || roles.Any(r => string.Equals(r, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                if (!roles.Any(r => string.Equals(r, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase)))
                {
                    roles = [RoleCodes.Admin, .. roles];
                }

                permissions = PermissionCodes.All.OrderBy(x => x).ToList();
            }
            else if (roleIds.Count > 0)
            {
                permissions = await LoadPermissionCodesAsync(roleIds, cancellationToken);
            }
            else
            {
                var employeeRoleId = await _context.RoleEntities.AsNoTracking()
                    .Where(x => x.Code == RoleCodes.Employee && !x.IsDeleted && x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (employeeRoleId.HasValue)
                {
                    roles = [RoleCodes.Employee];
                    permissions = await LoadPermissionCodesAsync([employeeRoleId.Value], cancellationToken);
                }
            }

            return new AuthContextDto
            {
                Roles = roles,
                Permissions = permissions,
            };
        }

        private async Task<List<string>> LoadPermissionCodesAsync(
            IReadOnlyCollection<Guid> roleIds,
            CancellationToken cancellationToken)
        {
            return await _context.RolePermissionEntities.AsNoTracking()
                .Where(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted && rp.PermissionCode != "")
                .Select(rp => rp.PermissionCode)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
        }
    }
}

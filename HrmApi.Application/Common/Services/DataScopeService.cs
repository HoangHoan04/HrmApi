using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Common.Services
{
    public class DataScopeService : IDataScopeService
    {
        private static readonly string[] ScopeRank =
        [
            DataScopes.Own,
            DataScopes.Department,
            DataScopes.Branch,
            DataScopes.All,
        ];

        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private DataScopeActor? _actorCache;
        private Dictionary<string, string>? _scopeCache;

        public DataScopeService(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<string> GetEffectiveScopeAsync(
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            if (_currentUser.IsAdmin)
                return DataScopes.All;

            if (string.IsNullOrWhiteSpace(permissionCode) || _currentUser.UserId is null)
                return DataScopes.Own;

            _scopeCache ??= await LoadScopesAsync(cancellationToken);
            var code = permissionCode.Trim();
            return _scopeCache.TryGetValue(code, out var scope) ? scope : DataScopes.Own;
        }

        public async Task<DataScopeActor> GetActorAsync(CancellationToken cancellationToken = default)
        {
            if (_actorCache != null)
                return _actorCache;

            Guid? companyId = _currentUser.CompanyId;
            Guid? branchId = _currentUser.BranchId;
            Guid? departmentId = null;
            Guid? employeeId = _currentUser.EmployeeId;

            if (employeeId.HasValue)
            {
                var emp = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == employeeId && !x.IsDeleted)
                    .Select(x => new { x.CompanyId, x.BranchId, x.DepartmentId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (emp != null)
                {
                    companyId ??= emp.CompanyId;
                    branchId ??= emp.BranchId;
                    departmentId = emp.DepartmentId;
                }
            }

            _actorCache = new DataScopeActor(employeeId, companyId, branchId, departmentId);
            return _actorCache;
        }

        private async Task<Dictionary<string, string>> LoadScopesAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId!.Value;
            var now = DateTime.UtcNow;

            var roleIds = await (
                from ur in _context.UserRoleEntities.AsNoTracking()
                join r in _context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                where ur.UserId == userId
                      && !ur.IsDeleted
                      && !r.IsDeleted
                      && r.IsActive
                      && (ur.EffectiveFrom == null || ur.EffectiveFrom <= now)
                      && (ur.EffectiveTo == null || ur.EffectiveTo >= now)
                select r.Id
            ).Distinct().ToListAsync(cancellationToken);

            if (roleIds.Count == 0)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var rows = await _context.RolePermissionEntities.AsNoTracking()
                .Where(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted && rp.PermissionCode != "")
                .Select(rp => new { rp.PermissionCode, rp.DataScope })
                .ToListAsync(cancellationToken);

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var code = row.PermissionCode.Trim();
                var scope = NormalizeScope(row.DataScope);
                if (!map.TryGetValue(code, out var existing))
                    map[code] = scope;
                else
                    map[code] = Widen(existing, scope);
            }

            return map;
        }

        private static string NormalizeScope(string? scope)
        {
            var s = (scope ?? DataScopes.Own).Trim().ToUpperInvariant();
            return s switch
            {
                DataScopes.All => DataScopes.All,
                DataScopes.Branch => DataScopes.Branch,
                DataScopes.Department => DataScopes.Department,
                _ => DataScopes.Own,
            };
        }

        private static string Widen(string a, string b)
        {
            var ra = Array.IndexOf(ScopeRank, NormalizeScope(a));
            var rb = Array.IndexOf(ScopeRank, NormalizeScope(b));
            return ScopeRank[Math.Max(ra, rb)];
        }
    }
}

using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Role;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.UserRoles
{
    public class GetUserRolesByUserQuery : IRequest<List<UserRoleItemDto>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserRolesByUserQueryHandler : IRequestHandler<GetUserRolesByUserQuery, List<UserRoleItemDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserRolesByUserQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserRoleItemDto>> Handle(GetUserRolesByUserQuery request, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            return await (
                from ur in _context.UserRoleEntities.AsNoTracking()
                join r in _context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                where ur.UserId == request.UserId
                      && !ur.IsDeleted
                      && !r.IsDeleted
                      && (ur.EffectiveFrom == null || ur.EffectiveFrom <= now)
                      && (ur.EffectiveTo == null || ur.EffectiveTo >= now)
                orderby r.Code
                select new UserRoleItemDto
                {
                    RoleId = r.Id,
                    RoleCode = r.Code,
                    RoleName = r.Name,
                    IsSystem = r.IsSystem,
                    EffectiveFrom = ur.EffectiveFrom,
                    EffectiveTo = ur.EffectiveTo,
                }
            ).ToListAsync(cancellationToken);
        }
    }

    public class SetUserRolesCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public List<Guid> RoleIds { get; set; } = [];
    }

    public class SetUserRolesCommandHandler : IRequestHandler<SetUserRolesCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IPermissionCache _permissionCache;

        public SetUserRolesCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog,
            IPermissionCache permissionCache)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _permissionCache = permissionCache;
        }

        public async Task<bool> Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
        {
            List<Guid> roleIds = (request.RoleIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
            if (roleIds.Count > 0)
            {
                int found = await _context.RoleEntities.AsNoTracking()
                    .CountAsync(x => roleIds.Contains(x.Id) && !x.IsDeleted && x.IsActive, cancellationToken);
                if (found != roleIds.Count)
                {
                    throw new InvalidOperationException("Có vai trò không tồn tại hoặc đã ngưng hoạt động.");
                }
            }

            List<UserRoleEntity> existing = await _context.UserRoleEntities
                .Where(x => x.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            HashSet<Guid> keep = roleIds.ToHashSet();
            foreach (UserRoleEntity? ur in existing.Where(x => !x.IsDeleted && !keep.Contains(x.RoleId)))
            {
                ur.IsDeleted = true;
                ur.UpdatedAt = DateTime.UtcNow;
                ur.UpdatedBy = _currentUser.UserId;
            }

            foreach (Guid roleId in roleIds)
            {
                UserRoleEntity? match = existing.FirstOrDefault(x => x.RoleId == roleId);
                if (match != null)
                {
                    match.IsDeleted = false;
                    match.EffectiveTo = null;
                    if (!match.EffectiveFrom.HasValue)
                    {
                        match.EffectiveFrom = DateTime.UtcNow;
                    }

                    match.UpdatedAt = DateTime.UtcNow;
                    match.UpdatedBy = _currentUser.UserId;
                }
                else
                {
                    _ = _context.UserRoleEntities.Add(new UserRoleEntity
                    {
                        UserId = request.UserId,
                        RoleId = roleId,
                        EffectiveFrom = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = _currentUser.UserId ?? Guid.Empty,
                    });
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            _permissionCache.InvalidateUser(request.UserId);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "UserRoleEntity",
                request.UserId,
                null,
                new { RoleIds = roleIds },
                "Gán vai trò cho người dùng");

            return true;
        }
    }

    public class GetUserRolesByEmployeeQuery : IRequest<List<UserRoleItemDto>>
    {
        public Guid EmployeeId { get; set; }
    }

    public class GetUserRolesByEmployeeQueryHandler : IRequestHandler<GetUserRolesByEmployeeQuery, List<UserRoleItemDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserRolesByEmployeeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<UserRoleItemDto>> Handle(GetUserRolesByEmployeeQuery request, CancellationToken cancellationToken)
            => EmployeeRoleLoader.LoadAsync(_context, request.EmployeeId, cancellationToken);
    }

    /// <summary>
    /// Vai trò đang hiệu lực của nhân viên — cùng cách tính với AuthContextService:
    /// gồm vai trò gán theo nhân viên (EmployeeId) và gán trực tiếp cho tài khoản (UserId).
    /// </summary>
    internal static class EmployeeRoleLoader
    {
        public static async Task<Guid?> FindUserIdAsync(IApplicationDbContext context, Guid employeeId, CancellationToken cancellationToken)
        {
            Guid? linkedUserId = await context.EmployeeEntities.AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => e.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            return await context.UserEntities.AsNoTracking()
                .Where(u => !u.IsDeleted && (u.EmployeeId == employeeId || (linkedUserId.HasValue && u.Id == linkedUserId.Value)))
                .Select(u => (Guid?)u.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public static async Task<List<UserRoleItemDto>> LoadAsync(IApplicationDbContext context, Guid employeeId, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            Guid? userId = await FindUserIdAsync(context, employeeId, cancellationToken);

            var rows = await (
                from ur in context.UserRoleEntities.AsNoTracking()
                join r in context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                where (ur.EmployeeId == employeeId || (userId.HasValue && ur.UserId == userId.Value))
                      && !ur.IsDeleted
                      && !r.IsDeleted
                      && (ur.EffectiveFrom == null || ur.EffectiveFrom <= now)
                      && (ur.EffectiveTo == null || ur.EffectiveTo >= now)
                select new
                {
                    r.Id,
                    r.Code,
                    r.Name,
                    r.IsSystem,
                    ur.EffectiveFrom,
                    ur.EffectiveTo,
                    ByEmployee = ur.EmployeeId == employeeId,
                }
            ).ToListAsync(cancellationToken);

            // Một vai trò có thể vừa gán theo nhân viên vừa theo tài khoản -> ưu tiên nguồn "EMPLOYEE" (sửa được)
            return rows
                .GroupBy(x => x.Id)
                .Select(g => g.OrderByDescending(x => x.ByEmployee).First())
                .OrderBy(x => x.Code)
                .Select(x => new UserRoleItemDto
                {
                    RoleId = x.Id,
                    RoleCode = x.Code,
                    RoleName = x.Name,
                    IsSystem = x.IsSystem,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    Source = x.ByEmployee ? "EMPLOYEE" : "USER",
                })
                .ToList();
        }
    }

    public class GetEmployeeAccessSummaryQuery : IRequest<EmployeeAccessSummaryDto>
    {
        public Guid EmployeeId { get; set; }
    }

    public class GetEmployeeAccessSummaryQueryHandler : IRequestHandler<GetEmployeeAccessSummaryQuery, EmployeeAccessSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetEmployeeAccessSummaryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeAccessSummaryDto> Handle(GetEmployeeAccessSummaryQuery request, CancellationToken cancellationToken)
        {
            var emp = await _context.EmployeeEntities.AsNoTracking()
                .Where(e => e.Id == request.EmployeeId && !e.IsDeleted)
                .Select(e => new
                {
                    e.Id,
                    e.Code,
                    e.FullName,
                    e.FirstName,
                    e.LastName,
                    e.AvatarUrl,
                    e.CompanyEmail,
                    e.Status,
                    CompanyName = e.Company != null ? e.Company.Name : null,
                    BranchName = e.Branch != null ? e.Branch.Name : null,
                    DepartmentName = e.Department != null ? e.Department.Name : null,
                    PositionName = e.Position != null && e.Position.PositionMaster != null ? e.Position.PositionMaster.Name : null,
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");

            Guid? userId = await EmployeeRoleLoader.FindUserIdAsync(_context, emp.Id, cancellationToken);
            EmployeeAccountInfoDto? account = null;
            if (userId.HasValue)
            {
                account = await _context.UserEntities.AsNoTracking()
                    .Where(u => u.Id == userId.Value)
                    .Select(u => new EmployeeAccountInfoDto
                    {
                        UserId = u.Id,
                        Username = u.Username,
                        Type = u.Type,
                        IsActive = u.IsActive,
                        IsLocked = u.IsLocked && u.LockedUntil != null && u.LockedUntil > DateTime.UtcNow,
                        MustChangePassword = u.MustChangePassword,
                        LastLoginAt = u.LastLoginAt,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }

            List<UserRoleItemDto> roles = await EmployeeRoleLoader.LoadAsync(_context, emp.Id, cancellationToken);
            bool isAdmin = string.Equals(account?.Type, UserType.Admin, StringComparison.OrdinalIgnoreCase);

            return new EmployeeAccessSummaryDto
            {
                EmployeeId = emp.Id,
                Code = emp.Code,
                FullName = !string.IsNullOrWhiteSpace(emp.FullName) ? emp.FullName! : $"{emp.LastName} {emp.FirstName}".Trim(),
                AvatarUrl = emp.AvatarUrl,
                CompanyEmail = emp.CompanyEmail,
                Status = emp.Status,
                CompanyName = emp.CompanyName,
                BranchName = emp.BranchName,
                DepartmentName = emp.DepartmentName,
                PositionName = emp.PositionName,
                Account = account,
                Roles = roles,
                UsesDefaultEmployeeRole = roles.Count == 0 && !isAdmin,
            };
        }
    }

    public class SetUserRolesByEmployeeCommand : IRequest<bool>
    {
        public Guid EmployeeId { get; set; }
        public List<Guid> RoleIds { get; set; } = [];
    }

    public class SetUserRolesByEmployeeCommandHandler : IRequestHandler<SetUserRolesByEmployeeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IPermissionCache _permissionCache;

        public SetUserRolesByEmployeeCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog,
            IPermissionCache permissionCache)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
            _permissionCache = permissionCache;
        }

        public async Task<bool> Handle(SetUserRolesByEmployeeCommand request, CancellationToken cancellationToken)
        {
            bool empExists = await _context.EmployeeEntities.AsNoTracking()
                .AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken);
            if (!empExists)
            {
                throw new InvalidOperationException("Không tìm thấy nhân viên.");
            }

            List<Guid> roleIds = (request.RoleIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
            if (roleIds.Count > 0)
            {
                int found = await _context.RoleEntities.AsNoTracking()
                    .CountAsync(x => roleIds.Contains(x.Id) && !x.IsDeleted && x.IsActive, cancellationToken);
                if (found != roleIds.Count)
                {
                    throw new InvalidOperationException("Có vai trò không tồn tại hoặc đã ngưng hoạt động.");
                }
            }

            List<UserRoleEntity> existing = await _context.UserRoleEntities
                .Where(x => x.EmployeeId == request.EmployeeId)
                .ToListAsync(cancellationToken);

            HashSet<Guid> keep = roleIds.ToHashSet();
            foreach (UserRoleEntity? ur in existing.Where(x => !x.IsDeleted && !keep.Contains(x.RoleId)))
            {
                ur.IsDeleted = true;
                ur.UpdatedAt = DateTime.UtcNow;
                ur.UpdatedBy = _currentUser.UserId;
            }

            foreach (Guid roleId in roleIds)
            {
                UserRoleEntity? match = existing.FirstOrDefault(x => x.RoleId == roleId);
                if (match != null)
                {
                    match.IsDeleted = false;
                    match.EffectiveTo = null;
                    if (!match.EffectiveFrom.HasValue)
                    {
                        match.EffectiveFrom = DateTime.UtcNow;
                    }

                    match.UpdatedAt = DateTime.UtcNow;
                    match.UpdatedBy = _currentUser.UserId;
                }
                else
                {
                    _ = _context.UserRoleEntities.Add(new UserRoleEntity
                    {
                        EmployeeId = request.EmployeeId,
                        RoleId = roleId,
                        EffectiveFrom = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = _currentUser.UserId ?? Guid.Empty,
                    });
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);

            // Áp dụng quyền mới ngay, không chờ cache hết hạn
            Guid? userId = await EmployeeRoleLoader.FindUserIdAsync(_context, request.EmployeeId, cancellationToken);
            if (userId.HasValue)
            {
                _permissionCache.InvalidateUser(userId.Value);
            }

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "UserRoleEntity",
                request.EmployeeId,
                null,
                new { RoleIds = roleIds },
                "Gán vai trò cho nhân viên");

            return true;
        }
    }
}

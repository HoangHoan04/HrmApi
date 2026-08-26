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

        public async Task<List<UserRoleItemDto>> Handle(GetUserRolesByEmployeeQuery request, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            return await (
                from ur in _context.UserRoleEntities.AsNoTracking()
                join r in _context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                where ur.EmployeeId == request.EmployeeId
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

        public SetUserRolesByEmployeeCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
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

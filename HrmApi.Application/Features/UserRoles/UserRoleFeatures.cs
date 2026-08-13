using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
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

        public GetUserRolesByUserQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<UserRoleItemDto>> Handle(GetUserRolesByUserQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
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
            bool userExists = await _context.UserEntities.AsNoTracking()
                .AnyAsync(x => x.Id == request.UserId && !x.IsDeleted, cancellationToken);
            if (!userExists)
                throw new InvalidOperationException("Không tìm thấy người dùng.");

            var roleIds = (request.RoleIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
            if (roleIds.Count > 0)
            {
                int found = await _context.RoleEntities.AsNoTracking()
                    .CountAsync(x => roleIds.Contains(x.Id) && !x.IsDeleted && x.IsActive, cancellationToken);
                if (found != roleIds.Count)
                    throw new InvalidOperationException("Có vai trò không tồn tại hoặc đã ngưng hoạt động.");
            }

            var existing = await _context.UserRoleEntities
                .Where(x => x.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            var keep = roleIds.ToHashSet();
            foreach (var ur in existing.Where(x => !x.IsDeleted && !keep.Contains(x.RoleId)))
            {
                ur.IsDeleted = true;
                ur.UpdatedAt = DateTime.UtcNow;
                ur.UpdatedBy = _currentUser.UserId;
            }

            foreach (Guid roleId in roleIds)
            {
                var match = existing.FirstOrDefault(x => x.RoleId == roleId);
                if (match != null)
                {
                    match.IsDeleted = false;
                    match.EffectiveTo = null;
                    if (!match.EffectiveFrom.HasValue)
                        match.EffectiveFrom = DateTime.UtcNow;
                    match.UpdatedAt = DateTime.UtcNow;
                    match.UpdatedBy = _currentUser.UserId;
                }
                else
                {
                    _context.UserRoleEntities.Add(new UserRoleEntity
                    {
                        UserId = request.UserId,
                        RoleId = roleId,
                        EffectiveFrom = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = _currentUser.UserId ?? Guid.Empty,
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _permissionCache.InvalidateUser(request.UserId);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "UserEntity",
                request.UserId,
                null,
                new { RoleIds = roleIds },
                "Gán vai trò cho người dùng");

            return true;
        }
    }

    public class GetUsersPagedQuery : PagedRequest, IRequest<PagedResult<UserListItemDto>>
    {
        public string? Username { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Type { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyCode { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsLocked { get; set; }
    }

    public class GetUsersPagedQueryHandler : IRequestHandler<GetUsersPagedQuery, PagedResult<UserListItemDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetUsersPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<UserListItemDto>> Handle(GetUsersPagedQuery request, CancellationToken cancellationToken)
        {
            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var query =
                from u in _context.UserEntities.AsNoTracking()
                join c in _context.CompanyEntities.AsNoTracking() on u.CompanyId equals c.Id into cj
                from c in cj.DefaultIfEmpty()
                join e in _context.EmployeeEntities.AsNoTracking() on u.EmployeeId equals e.Id into ej
                from e in ej.DefaultIfEmpty()
                where !u.IsDeleted
                select new
                {
                    User = u,
                    CompanyCode = c != null ? c.Code : null,
                    CompanyName = c != null ? c.Name : null,
                    EmployeeCode = e != null ? e.Code : null,
                    EmployeeName = e != null
                        ? (!string.IsNullOrWhiteSpace(e.FullName)
                            ? e.FullName!
                            : (e.LastName + " " + e.FirstName).Trim())
                        : null,
                };

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                string type = request.Type.Trim().ToUpperInvariant();
                query = query.Where(x => x.User.Type.ToUpper() == type);
            }
            if (request.IsActive.HasValue)
                query = query.Where(x => x.User.IsActive == request.IsActive.Value);
            if (request.IsLocked.HasValue)
                query = query.Where(x => x.User.IsLocked == request.IsLocked.Value);
            if (request.CompanyId.HasValue)
                query = query.Where(x => x.User.CompanyId == request.CompanyId.Value);
            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                string companyCode = request.CompanyCode.Trim().ToLower();
                query = query.Where(x => x.CompanyCode != null && x.CompanyCode.ToLower() == companyCode);
            }
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                string username = request.Username.Trim().ToLower();
                query = query.Where(x => x.User.Username.ToLower().Contains(username));
            }
            if (!string.IsNullOrWhiteSpace(request.EmployeeCode))
            {
                string empCode = request.EmployeeCode.Trim().ToLower();
                query = query.Where(x => x.EmployeeCode != null && x.EmployeeCode.ToLower().Contains(empCode));
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.User.Username.ToLower().Contains(s)
                    || (x.User.Email != null && x.User.Email.ToLower().Contains(s))
                    || (x.EmployeeCode != null && x.EmployeeCode.ToLower().Contains(s))
                    || (x.EmployeeName != null && x.EmployeeName.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderBy(x => x.User.Username)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.User.Id,
                    x.User.Username,
                    x.User.Type,
                    x.User.Email,
                    x.User.PhoneNumber,
                    x.User.IsActive,
                    x.User.IsLocked,
                    x.User.CompanyId,
                    x.CompanyCode,
                    x.CompanyName,
                    x.User.EmployeeId,
                    x.EmployeeCode,
                    x.EmployeeName,
                })
                .ToListAsync(cancellationToken);

            var userIds = users.Select(x => x.Id).ToList();

            var roleRows = userIds.Count == 0
                ? new List<(Guid UserId, Guid RoleId, string Code)>()
                : (await (
                    from ur in _context.UserRoleEntities.AsNoTracking()
                    join r in _context.RoleEntities.AsNoTracking() on ur.RoleId equals r.Id
                    where userIds.Contains(ur.UserId) && !ur.IsDeleted && !r.IsDeleted && r.IsActive
                    select new { ur.UserId, ur.RoleId, r.Code }
                ).ToListAsync(cancellationToken))
                .Select(x => (x.UserId, x.RoleId, x.Code))
                .ToList();

            var roleCodeMap = roleRows
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Code).Distinct().OrderBy(c => c).ToList());

            var roleIdMap = roleRows
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.RoleId).Distinct().ToList());

            var items = users.Select(u => new UserListItemDto
            {
                Id = u.Id,
                Username = u.Username,
                Type = u.Type,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                IsLocked = u.IsLocked,
                CompanyId = u.CompanyId,
                CompanyCode = u.CompanyCode,
                CompanyName = u.CompanyName,
                EmployeeId = u.EmployeeId,
                EmployeeCode = u.EmployeeCode,
                EmployeeName = u.EmployeeName,
                RoleIds = roleIdMap.TryGetValue(u.Id, out List<Guid>? ids) ? ids : [],
                RoleCodes = roleCodeMap.TryGetValue(u.Id, out List<string>? codes) ? codes : [],
            }).ToList();

            return new PagedResult<UserListItemDto>(items, total, pageIndex, pageSize);
        }
    }

    public class GetUserRolesByEmployeeQuery : IRequest<List<UserRoleItemDto>>
    {
        public Guid EmployeeId { get; set; }
    }

    public class GetUserRolesByEmployeeQueryHandler : IRequestHandler<GetUserRolesByEmployeeQuery, List<UserRoleItemDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserRolesByEmployeeQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<UserRoleItemDto>> Handle(GetUserRolesByEmployeeQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.UserEntities.AsNoTracking()
                .Where(x => x.EmployeeId == request.EmployeeId && !x.IsDeleted)
                .Select(x => new { x.Id })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Nhân viên chưa có tài khoản đăng nhập.");

            return await new GetUserRolesByUserQueryHandler(_context)
                .Handle(new GetUserRolesByUserQuery { UserId = user.Id }, cancellationToken);
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
        private readonly IMediator _mediator;

        public SetUserRolesByEmployeeCommandHandler(IApplicationDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<bool> Handle(SetUserRolesByEmployeeCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.UserEntities.AsNoTracking()
                .Where(x => x.EmployeeId == request.EmployeeId && !x.IsDeleted)
                .Select(x => new { x.Id })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Nhân viên chưa có tài khoản đăng nhập.");

            return await _mediator.Send(
                new SetUserRolesCommand { UserId = user.Id, RoleIds = request.RoleIds },
                cancellationToken);
        }
    }
}

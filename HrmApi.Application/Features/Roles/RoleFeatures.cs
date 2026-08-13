using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Role;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Roles
{
    public class GetRolesPagedQuery : PagedRequest, IRequest<PagedResult<RoleListItemDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyCode { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSystem { get; set; }
    }

    public class GetRolesPagedQueryHandler : IRequestHandler<GetRolesPagedQuery, PagedResult<RoleListItemDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetRolesPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<RoleListItemDto>> Handle(GetRolesPagedQuery request, CancellationToken cancellationToken)
        {
            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var query =
                from r in _context.RoleEntities.AsNoTracking()
                join c in _context.CompanyEntities.AsNoTracking() on r.CompanyId equals c.Id into cj
                from c in cj.DefaultIfEmpty()
                where !r.IsDeleted
                select new { Role = r, CompanyCode = c != null ? c.Code : null, CompanyName = c != null ? c.Name : null };

            if (request.IsActive.HasValue)
                query = query.Where(x => x.Role.IsActive == request.IsActive.Value);
            if (request.IsSystem.HasValue)
                query = query.Where(x => x.Role.IsSystem == request.IsSystem.Value);
            if (request.CompanyId.HasValue)
                query = query.Where(x => x.Role.CompanyId == request.CompanyId.Value);
            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                string companyCode = request.CompanyCode.Trim().ToLower();
                query = query.Where(x => x.CompanyCode != null && x.CompanyCode.ToLower() == companyCode);
            }
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                string code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Role.Code.ToLower().Contains(code));
            }
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                string name = request.Name.Trim().ToLower();
                query = query.Where(x => x.Role.Name.ToLower().Contains(name));
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.Role.Code.ToLower().Contains(s)
                    || x.Role.Name.ToLower().Contains(s)
                    || (x.Role.Description != null && x.Role.Description.ToLower().Contains(s)));
            }

            int total = await query.CountAsync(cancellationToken);

            var roles = await query
                .OrderByDescending(x => x.Role.IsSystem)
                .ThenBy(x => x.Role.Code)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RoleListItemDto
                {
                    Id = x.Role.Id,
                    Code = x.Role.Code,
                    Name = x.Role.Name,
                    Description = x.Role.Description,
                    IsSystem = x.Role.IsSystem,
                    IsActive = x.Role.IsActive,
                    CompanyId = x.Role.CompanyId,
                    CompanyCode = x.CompanyCode,
                    CompanyName = x.CompanyName,
                    CreatedAt = x.Role.CreatedAt,
                    PermissionCount = x.Role.RolePermissions.Count(rp => !rp.IsDeleted),
                    UserCount = x.Role.UserRoles.Count(ur => !ur.IsDeleted),
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RoleListItemDto>(roles, total, pageIndex, pageSize);
        }
    }

    public class GetRoleSelectBoxQuery : IRequest<List<RoleSelectBoxDto>>
    {
        public bool? IsActive { get; set; } = true;
        public Guid? CompanyId { get; set; }
    }

    public class GetRoleSelectBoxQueryHandler : IRequestHandler<GetRoleSelectBoxQuery, List<RoleSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetRoleSelectBoxQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<RoleSelectBoxDto>> Handle(GetRoleSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.RoleEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (request.CompanyId.HasValue)
                query = query.Where(x => x.CompanyId == null || x.CompanyId == request.CompanyId.Value);

            return await query
                .OrderByDescending(x => x.IsSystem)
                .ThenBy(x => x.Code)
                .Select(x => new RoleSelectBoxDto { Id = x.Id, Code = x.Code, Name = x.Name })
                .ToListAsync(cancellationToken);
        }
    }

    public class GetRoleByIdQuery : IRequest<RoleDetailDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDetailDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetRoleByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<RoleDetailDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _context.RoleEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (role == null) return null;

            var rpRows = await _context.RolePermissionEntities.AsNoTracking()
                .Where(rp => rp.RoleId == role.Id && !rp.IsDeleted && rp.PermissionCode != "")
                .Select(rp => new { rp.PermissionCode, rp.DataScope })
                .ToListAsync(cancellationToken);

            var catalogByCode = RbacPermissionCatalog.Items
                .ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

            var permissions = rpRows
                .Select(rp =>
                {
                    string code = rp.PermissionCode.Trim();
                    catalogByCode.TryGetValue(code, out var catalog);

                    return new RolePermissionItemDto
                    {
                        PermissionCode = code,
                        PermissionName = catalog?.Name ?? code,
                        Module = catalog?.Module ?? string.Empty,
                        DataScope = rp.DataScope,
                    };
                })
                .OrderBy(x => x.Module)
                .ThenBy(x => x.PermissionCode)
                .ToList();

            return new RoleDetailDto
            {
                Id = role.Id,
                Code = role.Code,
                Name = role.Name,
                Description = role.Description,
                IsSystem = role.IsSystem,
                IsActive = role.IsActive,
                CompanyId = role.CompanyId,
                BranchId = role.BranchId,
                Permissions = permissions,
            };
        }
    }

    public class CreateRoleCommand : IRequest<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public CreateRoleCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            string code = (request.Code ?? string.Empty).Trim().ToUpperInvariant();
            string name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException("Mã vai trò không được để trống.");
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Tên vai trò không được để trống.");

            bool exists = await _context.RoleEntities.AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.Code.ToUpper() == code, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Mã vai trò '{code}' đã tồn tại.");

            var entity = new RoleEntity
            {
                Code = code,
                Name = name,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                CompanyId = request.CompanyId,
                BranchId = request.BranchId,
                IsActive = request.IsActive,
                IsSystem = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };

            _context.RoleEntities.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "RoleEntity",
                entity.Id,
                null,
                new { entity.Code, entity.Name, entity.IsActive },
                "Tạo vai trò");

            return entity.Id;
        }
    }

    public class UpdateRoleCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public UpdateRoleCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.RoleEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy vai trò.");

            string name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Tên vai trò không được để trống.");

            var before = new { entity.Name, entity.Description, entity.IsActive };

            entity.Name = name;
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.IsActive = request.IsActive;
            if (!entity.IsSystem)
            {
                entity.CompanyId = request.CompanyId;
                entity.BranchId = request.BranchId;
            }
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "RoleEntity",
                entity.Id,
                before,
                new { entity.Name, entity.Description, entity.IsActive },
                "Cập nhật vai trò");

            return true;
        }
    }

    public class DeleteRoleCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;

        public DeleteRoleCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IActionLogService actionLog)
        {
            _context = context;
            _currentUser = currentUser;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.RoleEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy vai trò.");

            if (entity.IsSystem)
                throw new InvalidOperationException("Không thể xóa vai trò hệ thống.");

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.DELETE,
                "RoleEntity",
                entity.Id,
                new { entity.Code, entity.Name },
                null,
                "Xóa mềm vai trò");

            return true;
        }
    }

    public class SetRolePermissionsCommand : IRequest<bool>
    {
        public Guid RoleId { get; set; }
        public List<RolePermissionSetItem> Items { get; set; } = [];

        [System.Text.Json.Serialization.JsonPropertyName("permissions")]
        public List<RolePermissionSetItem>? Permissions
        {
            get => Items;
            set
            {
                if (value != null)
                    Items = value;
            }
        }
    }

    public class SetRolePermissionsCommandHandler : IRequestHandler<SetRolePermissionsCommand, bool>
    {
        private static readonly HashSet<string> ValidCodes =
            new(PermissionCodes.All, StringComparer.OrdinalIgnoreCase);

        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IActionLogService _actionLog;
        private readonly IPermissionCache _permissionCache;

        public SetRolePermissionsCommandHandler(
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

        public async Task<bool> Handle(SetRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            var role = await _context.RoleEntities
                .FirstOrDefaultAsync(x => x.Id == request.RoleId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy vai trò.");

            var items = (request.Items ?? [])
                .Select(x => new
                {
                    PermissionCode = (x.PermissionCode ?? string.Empty).Trim().ToUpperInvariant(),
                    DataScope = x.DataScope,
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.PermissionCode))
                .GroupBy(x => x.PermissionCode, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            var invalid = items
                .Where(x => !ValidCodes.Contains(x.PermissionCode))
                .Select(x => x.PermissionCode)
                .ToList();
            if (invalid.Count > 0)
                throw new InvalidOperationException(
                    $"Có quyền không tồn tại trong danh mục: {string.Join(", ", invalid)}.");

            var existing = await _context.RolePermissionEntities
                .Where(x => x.RoleId == role.Id)
                .ToListAsync(cancellationToken);

            var keepCodes = items
                .Select(x => x.PermissionCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var rp in existing.Where(x =>
                         !x.IsDeleted
                         && !keepCodes.Contains(x.PermissionCode)))
            {
                rp.IsDeleted = true;
                rp.UpdatedAt = DateTime.UtcNow;
                rp.UpdatedBy = _currentUser.UserId;
            }

            var existingByCode = existing
                .Where(x => !string.IsNullOrWhiteSpace(x.PermissionCode))
                .GroupBy(x => x.PermissionCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.IsDeleted).First(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                string scope = NormalizeScope(item.DataScope);
                if (existingByCode.TryGetValue(item.PermissionCode, out var rp))
                {
                    rp.PermissionCode = item.PermissionCode;
                    rp.DataScope = scope;
                    rp.IsDeleted = false;
                    rp.UpdatedAt = DateTime.UtcNow;
                    rp.UpdatedBy = _currentUser.UserId;
                }
                else
                {
                    _context.RolePermissionEntities.Add(new RolePermissionEntity
                    {
                        RoleId = role.Id,
                        PermissionCode = item.PermissionCode,
                        DataScope = scope,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = _currentUser.UserId ?? Guid.Empty,
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await _permissionCache.InvalidateByRoleAsync(role.Id, cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "RoleEntity",
                role.Id,
                null,
                new { role.Code, Permissions = items.Select(i => i.PermissionCode).ToList() },
                "Cập nhật quyền của vai trò");

            return true;
        }

        private static string NormalizeScope(string? scope)
        {
            string s = (scope ?? DataScopes.Own).Trim().ToUpperInvariant();
            return s switch
            {
                DataScopes.All or DataScopes.Branch or DataScopes.Department or DataScopes.Own => s,
                _ => DataScopes.Own,
            };
        }
    }
}

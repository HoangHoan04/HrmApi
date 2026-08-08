using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Department;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Departments.Queries
{
    #region Paged Query
    public class GetDepartmentsPagedQuery : PagedRequest, IRequest<PagedResult<DepartmentDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? ParentDepartmentId { get; set; }
    }

    public class GetDepartmentsPagedQueryHandler : IRequestHandler<GetDepartmentsPagedQuery, PagedResult<DepartmentDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetDepartmentsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<DepartmentDto>> Handle(GetDepartmentsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.DepartmentEntities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var name = request.Name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (request.ParentDepartmentId.HasValue && request.ParentDepartmentId != Guid.Empty)
            {
                query = query.Where(x => x.ParentDepartmentId == request.ParentDepartmentId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, request);

            var entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            var branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            var parentIds = entities.Where(x => x.ParentDepartmentId.HasValue).Select(x => x.ParentDepartmentId!.Value).Distinct().ToList();

            var companyMap = companyIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var branchMap = branchIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.BranchEntities
                    .AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var parentMap = parentIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.DepartmentEntities
                    .AsNoTracking()
                    .Where(x => parentIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = entities.Select(x =>
            {
                string? companyName = x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out var cName) ? cName : null;
                string? branchName = x.BranchId.HasValue && branchMap.TryGetValue(x.BranchId.Value, out var bName) ? bName : null;
                string? parentName = x.ParentDepartmentId.HasValue && parentMap.TryGetValue(x.ParentDepartmentId.Value, out var pName) ? pName : null;
                return DepartmentMapper.ToDto(x, companyName, branchName, parentName);
            }).ToList();

            return new PagedResult<DepartmentDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private static IQueryable<Domain.Entities.Organization.DepartmentEntity> ApplySorting(
            IQueryable<Domain.Entities.Organization.DepartmentEntity> query,
            GetDepartmentsPagedQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.SortField))
            {
                var isDesc = request.SortOrder?.ToLower() == "desc";
                return request.SortField.ToLower() switch
                {
                    "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
                    "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                    "status" or "isdeleted" => isDesc
                        ? query.OrderByDescending(x => x.IsDeleted)
                        : query.OrderBy(x => x.IsDeleted),
                    "createdat" => isDesc
                        ? query.OrderByDescending(x => x.CreatedAt)
                        : query.OrderBy(x => x.CreatedAt),
                    _ => query.OrderByDescending(x => x.CreatedAt)
                };
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
    #endregion

    #region Detail Query
    public class GetDepartmentByIdQuery : IRequest<DepartmentDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetDepartmentByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DepartmentDto?> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
        {
            var department = await _context.DepartmentEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (department == null) return null;

            string? companyName = null;
            if (department.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => x.Id == department.CompanyId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? branchName = null;
            if (department.BranchId.HasValue)
            {
                branchName = await _context.BranchEntities
                    .AsNoTracking()
                    .Where(x => x.Id == department.BranchId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? parentName = null;
            if (department.ParentDepartmentId.HasValue)
            {
                parentName = await _context.DepartmentEntities
                    .AsNoTracking()
                    .Where(x => x.Id == department.ParentDepartmentId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return DepartmentMapper.ToDto(department, companyName, branchName, parentName);
        }
    }
    #endregion

    #region Select Box Query
    public class GetDepartmentSelectBoxQuery : IRequest<List<DepartmentSelectBoxDto>>
    {
        public Guid? ExcludeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public class GetDepartmentSelectBoxQueryHandler : IRequestHandler<GetDepartmentSelectBoxQuery, List<DepartmentSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetDepartmentSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentSelectBoxDto>> Handle(GetDepartmentSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.DepartmentEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.ExcludeId.HasValue && request.ExcludeId != Guid.Empty)
            {
                query = query.Where(x => x.Id != request.ExcludeId.Value);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new DepartmentSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CompanyId = x.CompanyId,
                    BranchId = x.BranchId
                })
                .ToListAsync(cancellationToken);
        }
    }
    #endregion

    #region Cascade Query
    /// <summary>
    /// Load phòng ban theo chi nhánh (bắt buộc branchId).
    /// </summary>
    public class GetDepartmentsByBranchQuery : IRequest<List<DepartmentSelectBoxDto>>
    {
        public Guid BranchId { get; set; }
        public Guid? ExcludeId { get; set; }
    }

    public class GetDepartmentsByBranchQueryHandler : IRequestHandler<GetDepartmentsByBranchQuery, List<DepartmentSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetDepartmentsByBranchQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentSelectBoxDto>> Handle(GetDepartmentsByBranchQuery request, CancellationToken cancellationToken)
        {
            if (request.BranchId == Guid.Empty)
                throw new InvalidOperationException("Id chi nhánh là bắt buộc.");

            var query = _context.DepartmentEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.BranchId == request.BranchId);

            if (request.ExcludeId.HasValue)
            {
                query = query.Where(x => x.Id != request.ExcludeId.Value);
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new DepartmentSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CompanyId = x.CompanyId,
                    BranchId = x.BranchId
                })
                .ToListAsync(cancellationToken);
        }
    }
    #endregion
}
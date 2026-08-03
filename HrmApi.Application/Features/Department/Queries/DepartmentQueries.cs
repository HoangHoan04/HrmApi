using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs;
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
        public Guid? BranchId { get; set; }
        public Guid? CompanyId { get; set; }
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
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

            if (request.BranchId.HasValue)
                query = query.Where(x => x.BranchId == request.BranchId.Value);

            if (request.CompanyId.HasValue)
                query = query.Where(x => x.CompanyId == request.CompanyId.Value);

            if (request.ParentDepartmentId.HasValue)
                query = query.Where(x => x.ParentDepartmentId == request.ParentDepartmentId.Value);

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

            var parentDeptIds = entities.Where(x => x.ParentDepartmentId.HasValue).Select(x => x.ParentDepartmentId!.Value).Distinct().ToList();
            var branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            var companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            var managerIds = entities.Where(x => x.ManagerId.HasValue).Select(x => x.ManagerId!.Value).Distinct().ToList();
            var deputyIds = entities.Where(x => x.DeputyManagerId.HasValue).Select(x => x.DeputyManagerId!.Value).Distinct().ToList();

            var parentDeptNames = await _context.DepartmentEntities
                .Where(x => parentDeptIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var branchNames = await _context.BranchEntities
                .Where(x => branchIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var companyNames = await _context.CompanyEntities
                .Where(x => companyIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            // var managerNames = await _context.EmployeeEntities
            //     .Where(x => managerIds.Contains(x.Id))
            //     .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

            // var deputyNames = await _context.EmployeeEntities
            //     .Where(x => deputyIds.Contains(x.Id))
            //     .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

            var items = entities.Select(x =>
            {
                var dto = DepartmentMapper.ToDto(x);
                return dto;
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
                    "level" => isDesc ? query.OrderByDescending(x => x.Level) : query.OrderBy(x => x.Level),
                    "status" or "isdeleted" => isDesc ? query.OrderByDescending(x => x.IsDeleted) : query.OrderBy(x => x.IsDeleted),
                    "createdat" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
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

            var dto = DepartmentMapper.ToDto(department);

            if (department.ParentDepartmentId.HasValue)
            {
                var parentName = await _context.DepartmentEntities
                    .Where(x => x.Id == department.ParentDepartmentId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return dto;
        }
    }
    #endregion

    public class DepartmentSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class GetDepartmentSelectBoxQuery : IRequest<List<DepartmentSelectBoxDto>>
    {
        public Guid? ExcludeId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; } = true;
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

            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);

            if (request.ExcludeId.HasValue && request.ExcludeId != Guid.Empty)
                query = query.Where(x => x.Id != request.ExcludeId.Value);

            if (request.BranchId.HasValue)
                query = query.Where(x => x.BranchId == request.BranchId.Value);

            if (request.CompanyId.HasValue)
                query = query.Where(x => x.CompanyId == request.CompanyId.Value);

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new DepartmentSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);
        }
    }
}
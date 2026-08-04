using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Part;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Parts.Queries
{
    #region Paged Query
    public class GetPartsPagedQuery : PagedRequest, IRequest<PagedResult<PartDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartMasterId { get; set; }
    }

    public class GetPartsPagedQueryHandler : IRequestHandler<GetPartsPagedQuery, PagedResult<PartDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPartsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<PartDto>> Handle(GetPartsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PartEntities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code != null && x.Code.ToLower().Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var name = request.Name.Trim().ToLower();
                query = query.Where(x => x.Name != null && x.Name.ToLower().Contains(name));
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

            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
            {
                query = query.Where(x => x.DepartmentId == request.DepartmentId);
            }

            if (request.PartMasterId.HasValue && request.PartMasterId != Guid.Empty)
            {
                query = query.Where(x => x.PartMasterId == request.PartMasterId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(search)) ||
                    (x.Code != null && x.Code.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, request);

            var entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            var branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            var departmentIds = entities.Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();
            var partMasterIds = entities.Where(x => x.PartMasterId.HasValue).Select(x => x.PartMasterId!.Value).Distinct().ToList();

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

            var departmentMap = departmentIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.DepartmentEntities
                    .AsNoTracking()
                    .Where(x => departmentIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var partMasterMap = partMasterIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.PartMasterEntities
                    .AsNoTracking()
                    .Where(x => partMasterIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = entities.Select(x =>
            {
                string? companyName = x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out var cName) ? cName : null;
                string? branchName = x.BranchId.HasValue && branchMap.TryGetValue(x.BranchId.Value, out var bName) ? bName : null;
                string? departmentName = x.DepartmentId.HasValue && departmentMap.TryGetValue(x.DepartmentId.Value, out var dName) ? dName : null;
                string? partMasterName = x.PartMasterId.HasValue && partMasterMap.TryGetValue(x.PartMasterId.Value, out var pmName) ? pmName : null;
                return PartMapper.ToDto(x, companyName, branchName, departmentName, partMasterName);
            }).ToList();

            return new PagedResult<PartDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private static IQueryable<Domain.Entities.Organization.PartEntity> ApplySorting(
            IQueryable<Domain.Entities.Organization.PartEntity> query,
            GetPartsPagedQuery request)
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
    public class GetPartByIdQuery : IRequest<PartDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetPartByIdQueryHandler : IRequestHandler<GetPartByIdQuery, PartDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetPartByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PartDto?> Handle(GetPartByIdQuery request, CancellationToken cancellationToken)
        {
            var part = await _context.PartEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (part == null) return null;

            string? companyName = null;
            if (part.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities
                    .AsNoTracking()
                    .Where(x => x.Id == part.CompanyId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? branchName = null;
            if (part.BranchId.HasValue)
            {
                branchName = await _context.BranchEntities
                    .AsNoTracking()
                    .Where(x => x.Id == part.BranchId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? departmentName = null;
            if (part.DepartmentId.HasValue)
            {
                departmentName = await _context.DepartmentEntities
                    .AsNoTracking()
                    .Where(x => x.Id == part.DepartmentId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? partMasterName = null;
            if (part.PartMasterId.HasValue)
            {
                partMasterName = await _context.PartMasterEntities
                    .AsNoTracking()
                    .Where(x => x.Id == part.PartMasterId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return PartMapper.ToDto(part, companyName, branchName, departmentName, partMasterName);
        }
    }
    #endregion

    #region Select Box Query
    public class GetPartSelectBoxQuery : IRequest<List<PartSelectBoxDto>>
    {
        public Guid? ExcludeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class GetPartSelectBoxQueryHandler : IRequestHandler<GetPartSelectBoxQuery, List<PartSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPartSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PartSelectBoxDto>> Handle(GetPartSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PartEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.ExcludeId.HasValue && request.ExcludeId != Guid.Empty)
            {
                query = query.Where(x => x.Id != request.ExcludeId.Value);
            }

            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
            {
                query = query.Where(x => x.DepartmentId == request.DepartmentId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            return await query
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .Select(x => new PartSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name ?? x.Code,
                    DepartmentId = x.DepartmentId,
                    PartMasterId = x.PartMasterId
                })
                .ToListAsync(cancellationToken);
        }
    }
    #endregion
}
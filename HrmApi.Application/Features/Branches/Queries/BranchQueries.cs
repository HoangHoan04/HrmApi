using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Branch;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Application.Features.Branches.Queries
{
    #region Paged Query
    public class GetBranchesPagedQuery : PagedRequest, IRequest<PagedResult<BranchDto>>
    {
        public Guid? CompanyId { get; set; }
    }

    public class GetBranchesPagedQueryHandler : IRequestHandler<GetBranchesPagedQuery, PagedResult<BranchDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetBranchesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<BranchDto>> Handle(GetBranchesPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.BranchEntities.AsNoTracking();

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.SortField))
            {
                var isDesc = request.SortOrder?.ToLower() == "desc";
                switch (request.SortField.ToLower())
                {
                    case "code":
                        query = isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code);
                        break;
                    case "name":
                        query = isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);
                        break;
                    case "createdat":
                        query = isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.CreatedAt);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            var branchList = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var companyIds = branchList.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            var companiesMap = await _context.CompanyEntities
                .AsNoTracking()
                .Where(x => companyIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = branchList.Select(x => new BranchDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                Address = x.Address,
                IpAddress = x.IpAddress,
                GroupSalary = x.GroupSalary,
                ShortName = x.ShortName,
                Type = x.Type,
                CompanyId = x.CompanyId,
                CompanyName = x.CompanyId.HasValue && companiesMap.TryGetValue(x.CompanyId.Value, out var compName) ? compName : string.Empty,
                IsDeleted = x.IsDeleted,
                CreatedAt = x.CreatedAt
            }).ToList();

            return new PagedResult<BranchDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }
    #endregion

    #region Detail Query
    public class GetBranchByIdQuery : IRequest<BranchDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, BranchDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetBranchByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BranchDto?> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
        {
            var branch = await _context.BranchEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (branch == null) return null;

            string? companyName = null;
            if (branch.CompanyId.HasValue)
            {
                var company = await _context.CompanyEntities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == branch.CompanyId.Value, cancellationToken);
                companyName = company?.Name;
            }

            return new BranchDto
            {
                Id = branch.Id,
                Code = branch.Code,
                Name = branch.Name,
                Description = branch.Description,
                Address = branch.Address,
                IpAddress = branch.IpAddress,
                GroupSalary = branch.GroupSalary,
                ShortName = branch.ShortName,
                Type = branch.Type,
                CompanyId = branch.CompanyId,
                CompanyName = companyName,
                IsDeleted = branch.IsDeleted,
                CreatedAt = branch.CreatedAt
            };
        }
    }
    #endregion

    #region Select Box Query
    public class GetBranchSelectBoxQuery : IRequest<List<BranchSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
    }

    public class GetBranchSelectBoxQueryHandler : IRequestHandler<GetBranchSelectBoxQuery, List<BranchSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetBranchSelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchSelectBoxDto>> Handle(GetBranchSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.BranchEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new BranchSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    CompanyId = x.CompanyId
                })
                .ToListAsync(cancellationToken);
        }
    }
    #endregion
}

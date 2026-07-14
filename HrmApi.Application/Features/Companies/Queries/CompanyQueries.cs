using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Application.Features.Companies.Queries
{
    #region Paged Query
    public class GetCompaniesPagedQuery : PagedRequest, IRequest<PagedResult<CompanyDto>>
    {
    }

    public class GetCompaniesPagedQueryHandler : IRequestHandler<GetCompaniesPagedQuery, PagedResult<CompanyDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCompaniesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CompanyDto>> Handle(GetCompaniesPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CompanyEntities.AsNoTracking();

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

            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new CompanyDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    Address = x.Address,
                    TaxCode = x.TaxCode,
                    Hotline = x.Hotline,
                    IsDeleted = x.IsDeleted,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<CompanyDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }
    #endregion

    #region Detail Query
    public class GetCompanyByIdQuery : IRequest<CompanyDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetCompanyByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
        {
            var company = await _context.CompanyEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (company == null) return null;

            return new CompanyDto
            {
                Id = company.Id,
                Code = company.Code,
                Name = company.Name,
                Description = company.Description,
                Address = company.Address,
                TaxCode = company.TaxCode,
                Hotline = company.Hotline,
                IsDeleted = company.IsDeleted,
                CreatedAt = company.CreatedAt
            };
        }
    }
    #endregion

    #region Select Box Query
    public class GetCompanySelectBoxQuery : IRequest<List<CompanySelectBoxDto>>
    {
    }

    public class GetCompanySelectBoxQueryHandler : IRequestHandler<GetCompanySelectBoxQuery, List<CompanySelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCompanySelectBoxQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompanySelectBoxDto>> Handle(GetCompanySelectBoxQuery request, CancellationToken cancellationToken)
        {
            return await _context.CompanyEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new CompanySelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);
        }
    }
    #endregion
}

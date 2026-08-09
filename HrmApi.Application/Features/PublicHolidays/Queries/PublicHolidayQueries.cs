using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.PublicHoliday;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PublicHolidays.Queries
{
    public class GetPublicHolidaysPagedQuery : PagedRequest, IRequest<PagedResult<PublicHolidayDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public int? Year { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetPublicHolidaysPagedQueryHandler : IRequestHandler<GetPublicHolidaysPagedQuery, PagedResult<PublicHolidayDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetPublicHolidaysPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<PublicHolidayDto>> Handle(GetPublicHolidaysPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PublicHolidayEntities.AsNoTracking();

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
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId || x.CompanyId == null);
            if (request.Year.HasValue)
                query = query.Where(x => x.IsRecurringYearly || x.HolidayDate.Year == request.Year.Value);
            if (request.IsDeleted.HasValue)
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            query = query.OrderByDescending(x => x.HolidayDate);

            var entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            var companyMap = companyIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = entities.Select(x => PublicHolidayMapper.ToDto(
                x, x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out var n) ? n : null)).ToList();

            return new PagedResult<PublicHolidayDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetPublicHolidayByIdQuery : IRequest<PublicHolidayDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetPublicHolidayByIdQueryHandler : IRequestHandler<GetPublicHolidayByIdQuery, PublicHolidayDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetPublicHolidayByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PublicHolidayDto?> Handle(GetPublicHolidayByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.PublicHolidayEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return null;

            string? companyName = null;
            if (entity.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => x.Id == entity.CompanyId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            return PublicHolidayMapper.ToDto(entity, companyName);
        }
    }
}

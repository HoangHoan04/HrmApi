using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.ShiftMaster;
using HrmApi.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.ShiftMasters.Queries
{
    public class GetShiftMastersPagedQuery : PagedRequest, IRequest<PagedResult<ShiftMasterDto>>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetShiftMastersPagedQueryHandler : IRequestHandler<GetShiftMastersPagedQuery, PagedResult<ShiftMasterDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetShiftMastersPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<ShiftMasterDto>> Handle(GetShiftMastersPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ShiftMasterEntities.AsNoTracking();
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
                query = query.Where(x => x.CompanyId == request.CompanyId);
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
            var entities = await query.OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var companyIds = entities.Where(x => x.CompanyId.HasValue).Select(x => x.CompanyId!.Value).Distinct().ToList();
            var companyMap = companyIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.CompanyEntities.AsNoTracking()
                    .Where(x => companyIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var items = entities.Select(x => ShiftMasterMapper.ToDto(
                x, x.CompanyId.HasValue && companyMap.TryGetValue(x.CompanyId.Value, out var n) ? n : null)).ToList();
            return new PagedResult<ShiftMasterDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }

    public class GetShiftMasterByIdQuery : IRequest<ShiftMasterDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetShiftMasterByIdQueryHandler : IRequestHandler<GetShiftMasterByIdQuery, ShiftMasterDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetShiftMasterByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ShiftMasterDto?> Handle(GetShiftMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.ShiftMasterEntities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null) return null;
            string? companyName = null;
            if (entity.CompanyId.HasValue)
            {
                companyName = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => x.Id == entity.CompanyId).Select(x => x.Name).FirstOrDefaultAsync(cancellationToken);
            }
            return ShiftMasterMapper.ToDto(entity, companyName);
        }
    }

    public class GetShiftMasterSelectBoxQuery : IRequest<List<ShiftMasterSelectBoxDto>>
    {
        public Guid? CompanyId { get; set; }
    }

    public class GetShiftMasterSelectBoxQueryHandler : IRequestHandler<GetShiftMasterSelectBoxQuery, List<ShiftMasterSelectBoxDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetShiftMasterSelectBoxQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<ShiftMasterSelectBoxDto>> Handle(GetShiftMasterSelectBoxQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ShiftMasterEntities.AsNoTracking().Where(x => !x.IsDeleted && x.IsActive);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId || x.CompanyId == null);

            return await query.OrderBy(x => x.Name)
                .Select(x => new ShiftMasterSelectBoxDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    CompanyId = x.CompanyId
                }).ToListAsync(cancellationToken);
        }
    }
}

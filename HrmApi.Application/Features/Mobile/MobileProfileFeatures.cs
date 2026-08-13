using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Contract;
using HrmApi.Application.DTOs.Employee;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.DTOs.Organization;
using HrmApi.Application.Features.Contracts.Queries;
using HrmApi.Application.Features.Organization;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Contract;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Mobile
{
    public class GetMyContractsQuery : IRequest<List<ContractDto>> { }

    public class GetMyContractsQueryHandler : IRequestHandler<GetMyContractsQuery, List<ContractDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyContractsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<ContractDto>> Handle(GetMyContractsQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            List<ContractEntity> rows = await _context.ContractEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId)
                .OrderByDescending(x => x.StartDate)
                .Take(100)
                .ToListAsync(cancellationToken);
            return await ContractQueryHelper.MapAsync(_context, rows, cancellationToken);
        }
    }

    public class GetMyEmployeeFilesQuery : IRequest<List<EmployeeFileDto>> { }

    public class GetMyEmployeeFilesQueryHandler : IRequestHandler<GetMyEmployeeFilesQuery, List<EmployeeFileDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyEmployeeFilesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<EmployeeFileDto>> Handle(GetMyEmployeeFilesQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            var rows = await _context.EmployeeFileEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId)
                .OrderByDescending(x => x.IsCurrent)
                .ThenByDescending(x => x.CreatedAt)
                .Take(200)
                .ToListAsync(cancellationToken);
            return rows.Select(EmployeeMapper.ToFileDto).ToList();
        }
    }

    public class GetMobileDirectoryQuery : MobileDirectoryQuery, IRequest<PagedResult<MobileDirectoryEmployeeDto>> { }

    public class GetMobileDirectoryQueryHandler
        : IRequestHandler<GetMobileDirectoryQuery, PagedResult<MobileDirectoryEmployeeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMobileDirectoryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PagedResult<MobileDirectoryEmployeeDto>> Handle(
            GetMobileDirectoryQuery request,
            CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            Guid? companyId = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == employeeId)
                .Select(x => x.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!companyId.HasValue || companyId == Guid.Empty)
                throw new InvalidOperationException("Nhân viên chưa gắn công ty.");

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

            var query = _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId == companyId);

            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
                query = query.Where(x => x.DepartmentId == request.DepartmentId);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(s)
                    || (x.FullName != null && x.FullName.ToLower().Contains(s))
                    || x.Phone.ToLower().Contains(s)
                    || x.Email.ToLower().Contains(s));
            }

            int total = await query.CountAsync(cancellationToken);
            var page = await query
                .OrderBy(x => x.Code)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.FullName,
                    x.Phone,
                    x.Email,
                    x.DepartmentId,
                    x.PositionId,
                })
                .ToListAsync(cancellationToken);

            var deptIds = page.Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();
            var posIds = page.Where(x => x.PositionId.HasValue).Select(x => x.PositionId!.Value).Distinct().ToList();

            Dictionary<Guid, string> depts = deptIds.Count == 0
                ? []
                : await _context.DepartmentEntities.AsNoTracking()
                    .Where(x => deptIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> positions = [];
            if (posIds.Count > 0)
            {
                var posRows = await _context.PositionEntities.AsNoTracking()
                    .Where(x => posIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.PositionMasterId })
                    .ToListAsync(cancellationToken);
                var masterIds = posRows.Where(x => x.PositionMasterId.HasValue).Select(x => x.PositionMasterId!.Value).Distinct().ToList();
                Dictionary<Guid, string> masters = masterIds.Count == 0
                    ? []
                    : await _context.PositionMasterEntities.AsNoTracking()
                        .Where(x => masterIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
                foreach (var row in posRows)
                {
                    if (row.PositionMasterId.HasValue && masters.TryGetValue(row.PositionMasterId.Value, out var name))
                        positions[row.Id] = name;
                }
            }

            var items = page.Select(x => new MobileDirectoryEmployeeDto
            {
                Id = x.Id,
                Code = x.Code,
                FullName = x.FullName,
                Phone = x.Phone,
                Email = x.Email,
                DepartmentName = x.DepartmentId.HasValue && depts.TryGetValue(x.DepartmentId.Value, out var dn) ? dn : null,
                PositionName = x.PositionId.HasValue && positions.TryGetValue(x.PositionId.Value, out var pn) ? pn : null,
            }).ToList();

            return new PagedResult<MobileDirectoryEmployeeDto>(items, total, pageIndex, pageSize);
        }
    }

    public class GetMyOrgChartQuery : IRequest<OrgChartNodeDto?>
    {
        public bool IncludeParts { get; set; } = true;
    }

    public class GetMyOrgChartQueryHandler : IRequestHandler<GetMyOrgChartQuery, OrgChartNodeDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IMediator _mediator;

        public GetMyOrgChartQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IMediator mediator)
        {
            _context = context;
            _currentUser = currentUser;
            _mediator = mediator;
        }

        public async Task<OrgChartNodeDto?> Handle(GetMyOrgChartQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            Guid? companyId = _currentUser.CompanyId;
            if (!companyId.HasValue || companyId == Guid.Empty)
            {
                companyId = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == employeeId)
                    .Select(x => x.CompanyId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (!companyId.HasValue || companyId == Guid.Empty)
                throw new InvalidOperationException("Nhân viên chưa gắn công ty.");

            return await _mediator.Send(new GetOrgChartTreeQuery
            {
                CompanyId = companyId.Value,
                IncludeParts = request.IncludeParts,
            }, cancellationToken);
        }
    }
}

using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.RegisterDayOff;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.RegisterDayOffs.Queries
{
    public class GetRegisterDayOffsPagedQuery : PagedRequest, IRequest<PagedResult<RegisterDayOffDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public DayOffStatus? Status { get; set; }
        public DayOffType? DayOffType { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class GetRegisterDayOffsPagedQueryHandler : IRequestHandler<GetRegisterDayOffsPagedQuery, PagedResult<RegisterDayOffDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetRegisterDayOffsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<RegisterDayOffDto>> Handle(GetRegisterDayOffsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<RegisterDayOffEntity> query = _context.RegisterDayOffEntities.AsNoTracking();

            query = request.IsDeleted.HasValue ? query.Where(x => x.IsDeleted == request.IsDeleted.Value) : query.Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            if (request.DayOffType.HasValue)
            {
                query = query.Where(x => x.DayOffType == request.DayOffType.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(x => x.ToDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(x => x.FromDate <= request.ToDate.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);
            query = query.OrderByDescending(x => x.CreatedAt);

            List<RegisterDayOffEntity> entities = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<RegisterDayOffDto> items = await MapAsync(entities, cancellationToken);
            return new PagedResult<RegisterDayOffDto>(items, totalCount, request.PageIndex, request.PageSize);
        }

        private async Task<List<RegisterDayOffDto>> MapAsync(
            List<Domain.Entities.Leave.RegisterDayOffEntity> entities,
            CancellationToken cancellationToken)
        {
            List<Guid> employeeIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> branchIds = entities.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            List<Guid> configIds = entities.Where(x => x.DayOffConfigId.HasValue).Select(x => x.DayOffConfigId!.Value).Distinct().ToList();
            List<Guid> approverIds = entities.Where(x => x.ApproverId.HasValue).Select(x => x.ApproverId!.Value).Distinct().ToList();

            Dictionary<Guid, (string? Name, string Code)> employees = employeeIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => employeeIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => (Name: (string?)(x.FullName ?? $"{x.LastName} {x.FirstName}".Trim()), x.Code),
                        cancellationToken);

            Dictionary<Guid, string> branches = branchIds.Count == 0
                ? []
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> configs = configIds.Count == 0
                ? []
                : await _context.DayOffConfigEntities.AsNoTracking()
                    .Where(x => configIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> approvers = approverIds.Count == 0
                ? []
                : await _context.UserEntities.AsNoTracking()
                    .Where(x => approverIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Username ?? x.Id.ToString(), cancellationToken);

            return entities.Select(x =>
            {
                _ = employees.TryGetValue(x.EmployeeId, out (string? Name, string Code) emp);
                string? branchName = x.BranchId.HasValue && branches.TryGetValue(x.BranchId.Value, out string? bn) ? bn : null;
                string? configName = x.DayOffConfigId.HasValue && configs.TryGetValue(x.DayOffConfigId.Value, out string? cn) ? cn : null;
                string? approverName = x.ApproverId.HasValue && approvers.TryGetValue(x.ApproverId.Value, out string? an) ? an : null;
                return RegisterDayOffMapper.ToDto(x, emp.Name, emp.Code, branchName, configName, approverName);
            }).ToList();
        }
    }

    public class GetRegisterDayOffByIdQuery : IRequest<RegisterDayOffDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetRegisterDayOffByIdQueryHandler : IRequestHandler<GetRegisterDayOffByIdQuery, RegisterDayOffDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetRegisterDayOffByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RegisterDayOffDto?> Handle(GetRegisterDayOffByIdQuery request, CancellationToken cancellationToken)
        {
            RegisterDayOffEntity? entity = await _context.RegisterDayOffEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
            {
                return null;
            }

            var emp = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == entity.EmployeeId)
                .Select(x => new { x.FullName, x.LastName, x.FirstName, x.Code })
                .FirstOrDefaultAsync(cancellationToken);

            string? branchName = null;
            if (entity.BranchId.HasValue)
            {
                branchName = await _context.BranchEntities.AsNoTracking()
                    .Where(x => x.Id == entity.BranchId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? configName = null;
            if (entity.DayOffConfigId.HasValue)
            {
                configName = await _context.DayOffConfigEntities.AsNoTracking()
                    .Where(x => x.Id == entity.DayOffConfigId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? approverName = null;
            if (entity.ApproverId.HasValue)
            {
                approverName = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == entity.ApproverId)
                    .Select(x => x.Username)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return RegisterDayOffMapper.ToDto(
                entity,
                emp?.FullName ?? $"{emp?.LastName} {emp?.FirstName}".Trim(),
                emp?.Code,
                branchName,
                configName,
                approverName);
        }
    }

    public class GetMyRegisterDayOffsQuery : IRequest<List<RegisterDayOffDto>>
    {
        public DayOffStatus? Status { get; set; }
        public int? Year { get; set; }
    }

    public class GetMyRegisterDayOffsQueryHandler : IRequestHandler<GetMyRegisterDayOffsQuery, List<RegisterDayOffDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyRegisterDayOffsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<RegisterDayOffDto>> Handle(GetMyRegisterDayOffsQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await ResolveEmployeeIdAsync(cancellationToken);

            IQueryable<RegisterDayOffEntity> query = _context.RegisterDayOffEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId && !x.IsDeleted);

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            if (request.Year.HasValue)
            {
                query = query.Where(x => x.FromDate.Year == request.Year.Value || x.ToDate.Year == request.Year.Value);
            }

            List<RegisterDayOffEntity> entities = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

            List<Guid> configIds = entities.Where(x => x.DayOffConfigId.HasValue).Select(x => x.DayOffConfigId!.Value).Distinct().ToList();
            Dictionary<Guid, string> configs = configIds.Count == 0
                ? []
                : await _context.DayOffConfigEntities.AsNoTracking()
                    .Where(x => configIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return entities.Select(x =>
            {
                string? configName = x.DayOffConfigId.HasValue && configs.TryGetValue(x.DayOffConfigId.Value, out string? cn) ? cn : null;
                return RegisterDayOffMapper.ToDto(x, null, null, null, configName);
            }).ToList();
        }

        private async Task<Guid> ResolveEmployeeIdAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
            {
                return _currentUser.EmployeeId.Value;
            }

            if (_currentUser.UserId.HasValue)
            {
                Guid? empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                {
                    return empId.Value;
                }
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
        }
    }

    public class GetMyLeaveBalanceQuery : IRequest<MobileLeaveBalanceDto>
    {
        public int? Year { get; set; }
    }

    public class GetMyLeaveBalanceQueryHandler : IRequestHandler<GetMyLeaveBalanceQuery, MobileLeaveBalanceDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyLeaveBalanceQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<MobileLeaveBalanceDto> Handle(GetMyLeaveBalanceQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await ResolveEmployeeIdAsync(cancellationToken);
            int year = request.Year ?? BusinessDateHelper.Today().Year;

            EmployeeEntity? employee = await _context.EmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == employeeId && !x.IsDeleted, cancellationToken);
            Guid? companyId = employee?.CompanyId;

            List<DayOffConfigEntity> configs = await _context.DayOffConfigEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive
                    && (x.CompanyId == null || (companyId.HasValue && x.CompanyId == companyId)))
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            DateOnly yearStart = new(year, 1, 1);
            DateOnly yearEnd = new(year, 12, 31);

            List<RegisterDayOffEntity> yearLeaves = await _context.RegisterDayOffEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId
                    && !x.IsDeleted
                    && x.FromDate <= yearEnd
                    && x.ToDate >= yearStart
                    && (x.Status == DayOffStatus.PENDING || x.Status == DayOffStatus.APPROVED))
                .ToListAsync(cancellationToken);

            decimal SumDays(DayOffType type, DayOffStatus status) =>
                yearLeaves
                    .Where(x => x.DayOffType == type && x.Status == status)
                    .Sum(x => x.TotalDays);

            decimal annualUsed = SumDays(DayOffType.ANNUAL, DayOffStatus.APPROVED);
            decimal annualPending = SumDays(DayOffType.ANNUAL, DayOffStatus.PENDING);
            decimal sickUsed = SumDays(DayOffType.SICK, DayOffStatus.APPROVED)
                + SumDays(DayOffType.SICK, DayOffStatus.PENDING);
            decimal unpaidUsed = SumDays(DayOffType.UNPAID, DayOffStatus.APPROVED)
                + SumDays(DayOffType.UNPAID, DayOffStatus.PENDING);

            DayOffConfigEntity? annualConfig = configs.FirstOrDefault(x => x.DayOffType == DayOffType.ANNUAL);

            decimal annualTotal = 0;
            DayOffConfigEmployeeEntity? empAllocation = null;
            if (annualConfig != null)
            {
                empAllocation = await _context.DayOffConfigEmployeeEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeId == employeeId
                        && x.DayOffConfigId == annualConfig.Id
                        && x.Year == year
                        && !x.IsDeleted, cancellationToken);
            }

            if (empAllocation != null)
            {
                annualTotal = empAllocation.AllocatedDays;
            }
            else if (annualConfig != null)
            {
                annualTotal = annualConfig.DefaultDaysPerYear;
            }

            decimal annualRemaining = Math.Max(0, annualTotal - annualUsed - annualPending);

            return new MobileLeaveBalanceDto
            {
                Year = year,
                AnnualTotal = annualTotal,
                AnnualUsed = annualUsed,
                AnnualPending = annualPending,
                AnnualRemaining = annualRemaining,
                SickUsed = sickUsed,
                UnpaidUsed = unpaidUsed,
                Configs = configs.Select(x => new MobileLeaveConfigDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    DayOffType = x.DayOffType,
                    DefaultDaysPerYear = x.DefaultDaysPerYear,
                    IsPaid = x.IsPaid,
                }).ToList(),
            };
        }

        private async Task<Guid> ResolveEmployeeIdAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.EmployeeId.HasValue && _currentUser.EmployeeId != Guid.Empty)
            {
                return _currentUser.EmployeeId.Value;
            }

            if (_currentUser.UserId.HasValue)
            {
                Guid? empId = await _context.UserEntities.AsNoTracking()
                    .Where(x => x.Id == _currentUser.UserId.Value)
                    .Select(x => x.EmployeeId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (empId.HasValue && empId != Guid.Empty)
                {
                    return empId.Value;
                }
            }

            throw new InvalidOperationException("Tài khoản chưa gắn nhân viên.");
        }
    }
}

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
        public Guid? DayOffConfigId { get; set; }
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

            if (request.DayOffConfigId.HasValue && request.DayOffConfigId != Guid.Empty)
            {
                query = query.Where(x => x.DayOffConfigId == request.DayOffConfigId.Value);
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
            List<Guid> requestedIds = entities.Where(x => x.RequestedApproverId.HasValue).Select(x => x.RequestedApproverId!.Value).Distinct().ToList();
            List<Guid> nameIds = approverIds.Union(requestedIds).Distinct().ToList();

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

            Dictionary<Guid, string> names = nameIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => nameIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => x.FullName ?? $"{x.LastName} {x.FirstName}".Trim(),
                        cancellationToken);

            return entities.Select(x =>
            {
                _ = employees.TryGetValue(x.EmployeeId, out (string? Name, string Code) emp);
                string? branchName = x.BranchId.HasValue && branches.TryGetValue(x.BranchId.Value, out string? bn) ? bn : null;
                string? configName = x.DayOffConfigId.HasValue && configs.TryGetValue(x.DayOffConfigId.Value, out string? cn) ? cn : null;
                string? approverName = x.ApproverId.HasValue && names.TryGetValue(x.ApproverId.Value, out string? an) ? an : null;
                string? requestedName = x.RequestedApproverId.HasValue && names.TryGetValue(x.RequestedApproverId.Value, out string? rn) ? rn : null;
                return RegisterDayOffMapper.ToDto(x, emp.Name, emp.Code, branchName, configName, approverName, requestedName);
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
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
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
                approverName = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == entity.ApproverId)
                    .Select(x => x.FullName ?? (x.LastName + " " + x.FirstName).Trim())
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? requestedApproverName = null;
            if (entity.RequestedApproverId.HasValue)
            {
                requestedApproverName = await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => x.Id == entity.RequestedApproverId)
                    .Select(x => x.FullName ?? (x.LastName + " " + x.FirstName).Trim())
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return RegisterDayOffMapper.ToDto(
                entity,
                emp?.FullName ?? $"{emp?.LastName} {emp?.FirstName}".Trim(),
                emp?.Code,
                branchName,
                configName,
                approverName,
                requestedApproverName);
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
            List<Guid> approverIds = entities.Where(x => x.ApproverId.HasValue).Select(x => x.ApproverId!.Value).Distinct().ToList();
            List<Guid> requestedIds = entities.Where(x => x.RequestedApproverId.HasValue).Select(x => x.RequestedApproverId!.Value).Distinct().ToList();
            List<Guid> nameIds = approverIds.Union(requestedIds).Distinct().ToList();

            Dictionary<Guid, string> configs = configIds.Count == 0
                ? []
                : await _context.DayOffConfigEntities.AsNoTracking()
                    .Where(x => configIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> names = nameIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => nameIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => x.FullName ?? $"{x.LastName} {x.FirstName}".Trim(),
                        cancellationToken);

            return entities.Select(x =>
            {
                string? configName = x.DayOffConfigId.HasValue && configs.TryGetValue(x.DayOffConfigId.Value, out string? cn) ? cn : null;
                string? approverName = x.ApproverId.HasValue && names.TryGetValue(x.ApproverId.Value, out string? an) ? an : null;
                string? requestedName = x.RequestedApproverId.HasValue && names.TryGetValue(x.RequestedApproverId.Value, out string? rn) ? rn : null;
                return RegisterDayOffMapper.ToDto(x, null, null, null, configName, approverName, requestedName);
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

            DayOffConfigEntity? annualConfig = configs.FirstOrDefault(x => string.Equals(x.Code, "ANNUAL", StringComparison.OrdinalIgnoreCase))
                ?? configs.FirstOrDefault(x => x.DeductBalance)
                ?? configs.FirstOrDefault();

            decimal annualUsed = 0;
            decimal annualPending = 0;
            if (annualConfig != null)
            {
                annualUsed = yearLeaves
                    .Where(x => x.DayOffConfigId == annualConfig.Id && x.Status == DayOffStatus.APPROVED)
                    .Sum(x => x.TotalDays);
                annualPending = yearLeaves
                    .Where(x => x.DayOffConfigId == annualConfig.Id && x.Status == DayOffStatus.PENDING)
                    .Sum(x => x.TotalDays);
            }

            decimal otherUsed = yearLeaves
                .Where(x => annualConfig == null || x.DayOffConfigId != annualConfig.Id)
                .Sum(x => x.TotalDays);

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

            decimal annualRemaining = Math.Max(0, annualTotal - annualUsed - annualPending);

            return new MobileLeaveBalanceDto
            {
                Year = year,
                AnnualTotal = annualTotal,
                AnnualUsed = annualUsed,
                AnnualPending = annualPending,
                AnnualRemaining = annualRemaining,
                SickUsed = otherUsed,
                UnpaidUsed = 0,
                Configs = configs.Select(x => new MobileLeaveConfigDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    DefaultDaysPerYear = x.DefaultDaysPerYear,
                    IsPaid = x.IsPaid,
                    DeductBalance = x.DeductBalance,
                    RequireAttachment = x.RequireAttachment,
                    MaxDaysPerRequest = x.MaxDaysPerRequest,
                    MinNoticeDays = x.MinNoticeDays,
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

    public class GetPendingApprovalsQuery : IRequest<List<RegisterDayOffDto>>
    {
        public DayOffStatus? Status { get; set; }
    }

    public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, List<RegisterDayOffDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public GetPendingApprovalsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<RegisterDayOffDto>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
        {
            Guid managerId = await Features.RegisterDayOffs.Commands.CurrentEmployeeHelper.ResolveAsync(
                _context, _currentUser, cancellationToken);

            DayOffStatus status = request.Status ?? DayOffStatus.PENDING;
            List<RegisterDayOffEntity> entities = await _context.RegisterDayOffEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.Status == status
                    && x.RequestedApproverId == managerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            List<Guid> empIds = entities.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> cfgIds = entities.Where(x => x.DayOffConfigId.HasValue).Select(x => x.DayOffConfigId!.Value).Distinct().ToList();
            List<Guid> requestedIds = entities.Where(x => x.RequestedApproverId.HasValue).Select(x => x.RequestedApproverId!.Value).Distinct().ToList();

            Dictionary<Guid, (string? Name, string Code)> employees = empIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => empIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => (Name: (string?)(x.FullName ?? $"{x.LastName} {x.FirstName}".Trim()), x.Code),
                        cancellationToken);

            Dictionary<Guid, string> configs = cfgIds.Count == 0
                ? []
                : await _context.DayOffConfigEntities.AsNoTracking()
                    .Where(x => cfgIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            Dictionary<Guid, string> requestedNames = requestedIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => requestedIds.Contains(x.Id))
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => x.FullName ?? $"{x.LastName} {x.FirstName}".Trim(),
                        cancellationToken);

            return entities.Select(x =>
            {
                employees.TryGetValue(x.EmployeeId, out var emp);
                string? configName = x.DayOffConfigId.HasValue && configs.TryGetValue(x.DayOffConfigId.Value, out string? cn) ? cn : null;
                string? requestedName = x.RequestedApproverId.HasValue && requestedNames.TryGetValue(x.RequestedApproverId.Value, out string? rn) ? rn : null;
                return RegisterDayOffMapper.ToDto(x, emp.Name, emp.Code, null, configName, null, requestedName);
            }).ToList();
        }
    }
}

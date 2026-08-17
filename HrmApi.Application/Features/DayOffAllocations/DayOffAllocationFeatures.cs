using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.RegisterDayOff;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.DayOffAllocations
{
    public class DayOffAllocationDto
    {
        public Guid Id { get; set; }
        public Guid DayOffConfigId { get; set; }
        public string? DayOffConfigName { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public decimal PendingDays { get; set; }
        public string? Note { get; set; }
    }

    public class GetDayOffAllocationsPagedQuery : PagedRequest, IRequest<PagedResult<DayOffAllocationDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? DayOffConfigId { get; set; }
        public int? Year { get; set; }
    }

    public class GetDayOffAllocationsPagedQueryHandler : IRequestHandler<GetDayOffAllocationsPagedQuery, PagedResult<DayOffAllocationDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetDayOffAllocationsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<DayOffAllocationDto>> Handle(GetDayOffAllocationsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<DayOffConfigEmployeeEntity> query = _context.DayOffConfigEmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (request.DayOffConfigId.HasValue) query = query.Where(x => x.DayOffConfigId == request.DayOffConfigId);
            if (request.Year.HasValue) query = query.Where(x => x.Year == request.Year);

            int total = await query.CountAsync(cancellationToken);
            List<DayOffConfigEmployeeEntity> rows = await query
                .OrderByDescending(x => x.Year).ThenBy(x => x.EmployeeId)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<DayOffAllocationDto> items = await MapAsync(rows, cancellationToken);
            return new PagedResult<DayOffAllocationDto>(items, total, request.PageIndex, request.PageSize);
        }

        private async Task<List<DayOffAllocationDto>> MapAsync(List<DayOffConfigEmployeeEntity> rows, CancellationToken cancellationToken)
        {
            List<Guid> empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> cfgIds = rows.Select(x => x.DayOffConfigId).Distinct().ToList();

            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.FullName, x.Code, x.LastName, x.FirstName }, cancellationToken);
            var configs = await _context.DayOffConfigEntities.AsNoTracking()
                .Where(x => cfgIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            List<DayOffAllocationDto> result = [];
            foreach (DayOffConfigEmployeeEntity row in rows)
            {
                employees.TryGetValue(row.EmployeeId, out var emp);
                configs.TryGetValue(row.DayOffConfigId, out var cfgName);
                DateOnly ys = new(row.Year, 1, 1);
                DateOnly ye = new(row.Year, 12, 31);
                decimal pending = await _context.RegisterDayOffEntities.AsNoTracking()
                    .Where(x => x.EmployeeId == row.EmployeeId
                        && x.DayOffConfigId == row.DayOffConfigId
                        && !x.IsDeleted
                        && x.Status == DayOffStatus.PENDING
                        && x.FromDate <= ye && x.ToDate >= ys)
                    .SumAsync(x => x.TotalDays, cancellationToken);

                result.Add(new DayOffAllocationDto
                {
                    Id = row.Id,
                    DayOffConfigId = row.DayOffConfigId,
                    DayOffConfigName = cfgName,
                    EmployeeId = row.EmployeeId,
                    EmployeeName = emp?.FullName ?? $"{emp?.LastName} {emp?.FirstName}".Trim(),
                    EmployeeCode = emp?.Code,
                    Year = row.Year,
                    AllocatedDays = row.AllocatedDays,
                    UsedDays = row.UsedDays,
                    RemainingDays = Math.Max(0, row.AllocatedDays - row.UsedDays - pending),
                    PendingDays = pending,
                    Note = row.Note,
                });
            }
            return result;
        }
    }

    public class UpsertDayOffAllocationCommand : IRequest<Guid>
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid DayOffConfigId { get; set; }
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public string? Note { get; set; }
    }

    public class UpsertDayOffAllocationCommandHandler : IRequestHandler<UpsertDayOffAllocationCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpsertDayOffAllocationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(UpsertDayOffAllocationCommand request, CancellationToken cancellationToken)
        {
            if (request.EmployeeId == Guid.Empty || request.DayOffConfigId == Guid.Empty)
                throw new InvalidOperationException("Nhân viên và loại nghỉ là bắt buộc.");
            if (request.Year < 2000 || request.Year > 2100)
                throw new InvalidOperationException("Năm không hợp lệ.");
            if (request.AllocatedDays < 0)
                throw new InvalidOperationException("Số ngày cấp phải >= 0.");

            bool empOk = await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken);
            if (!empOk) throw new InvalidOperationException("Nhân viên không tồn tại.");
            bool cfgOk = await _context.DayOffConfigEntities.AnyAsync(x => x.Id == request.DayOffConfigId && !x.IsDeleted, cancellationToken);
            if (!cfgOk) throw new InvalidOperationException("Cấu hình nghỉ không tồn tại.");

            DayOffConfigEmployeeEntity? entity = null;
            if (request.Id.HasValue && request.Id != Guid.Empty)
            {
                entity = await _context.DayOffConfigEmployeeEntities
                    .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            }
            entity ??= await _context.DayOffConfigEmployeeEntities
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == request.EmployeeId
                    && x.DayOffConfigId == request.DayOffConfigId
                    && x.Year == request.Year
                    && !x.IsDeleted, cancellationToken);

            if (entity == null)
            {
                entity = new DayOffConfigEmployeeEntity
                {
                    EmployeeId = request.EmployeeId,
                    DayOffConfigId = request.DayOffConfigId,
                    Year = request.Year,
                    UsedDays = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                };
                _ = _context.DayOffConfigEmployeeEntities.Add(entity);
            }

            entity.AllocatedDays = request.AllocatedDays;
            entity.RemainingDays = Math.Max(0, entity.AllocatedDays - entity.UsedDays);
            entity.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }

    public class PreviewLeaveDaysQuery : PreviewLeaveDaysRequest, IRequest<PreviewLeaveDaysDto> { }

    public class PreviewLeaveDaysQueryHandler : IRequestHandler<PreviewLeaveDaysQuery, PreviewLeaveDaysDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public PreviewLeaveDaysQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PreviewLeaveDaysDto> Handle(PreviewLeaveDaysQuery request, CancellationToken cancellationToken)
        {
            if (request.ToDate < request.FromDate)
                throw new InvalidOperationException("Ngày kết thúc phải >= ngày bắt đầu.");

            Guid? companyId = request.CompanyId;
            if (!companyId.HasValue)
            {
                Guid? employeeId = request.EmployeeId;
                if (!employeeId.HasValue || employeeId == Guid.Empty)
                {
                    try
                    {
                        employeeId = await Features.RegisterDayOffs.Commands.CurrentEmployeeHelper.ResolveAsync(
                            _context, _currentUser, cancellationToken);
                    }
                    catch
                    {
                        employeeId = null;
                    }
                }

                if (employeeId.HasValue)
                {
                    companyId = await _context.EmployeeEntities.AsNoTracking()
                        .Where(x => x.Id == employeeId)
                        .Select(x => x.CompanyId)
                        .FirstOrDefaultAsync(cancellationToken);
                }
            }

            SaturdayPolicy policy = await LeaveDayCalculator.ResolveSaturdayPolicyAsync(_context, companyId, cancellationToken);
            decimal total = await LeaveDayCalculator.CountWorkingDaysAsync(
                _context, request.FromDate, request.ToDate, companyId, request.Session, cancellationToken);

            return new PreviewLeaveDaysDto
            {
                TotalDays = total,
                SaturdayPolicy = policy,
                Session = request.Session,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
            };
        }
    }

    public class GetLeaveBalanceReportQuery : PagedRequest, IRequest<PagedResult<LeaveBalanceReportDto>>
    {
        public int? Year { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DayOffConfigId { get; set; }
        public Guid? EmployeeId { get; set; }
        public bool OnlyWithRemaining { get; set; }
        public DateOnly? ExpiringBefore { get; set; }
    }

    public class LeaveBalanceReportDto
    {
        public Guid Id { get; set; }
        public Guid DayOffConfigId { get; set; }
        public string? DayOffConfigName { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public decimal PendingDays { get; set; }
        public DateOnly ExpiresOn { get; set; }
        public bool IsExpiringSoon { get; set; }
        public string? Note { get; set; }
    }

    public class GetLeaveBalanceReportQueryHandler
        : IRequestHandler<GetLeaveBalanceReportQuery, PagedResult<LeaveBalanceReportDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetLeaveBalanceReportQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<LeaveBalanceReportDto>> Handle(
            GetLeaveBalanceReportQuery request,
            CancellationToken cancellationToken)
        {
            int year = request.Year ?? DateTime.UtcNow.Year;
            DateOnly expiresOn = new(year, 12, 31);

            if (request.ExpiringBefore.HasValue && expiresOn >= request.ExpiringBefore.Value)
                return new PagedResult<LeaveBalanceReportDto>([], 0, request.PageIndex, request.PageSize);

            IQueryable<DayOffConfigEmployeeEntity> query = _context.DayOffConfigEmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Year == year);

            if (request.DayOffConfigId.HasValue && request.DayOffConfigId != Guid.Empty)
                query = query.Where(x => x.DayOffConfigId == request.DayOffConfigId);
            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
                query = query.Where(x => x.EmployeeId == request.EmployeeId);

            if (request.CompanyId.HasValue || request.BranchId.HasValue || request.DepartmentId.HasValue)
            {
                var empFilter = _context.EmployeeEntities.AsNoTracking().Where(e => !e.IsDeleted);
                if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                    empFilter = empFilter.Where(e => e.CompanyId == request.CompanyId);
                if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                    empFilter = empFilter.Where(e => e.BranchId == request.BranchId);
                if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
                    empFilter = empFilter.Where(e => e.DepartmentId == request.DepartmentId);

                var empIds = empFilter.Select(e => e.Id);
                query = query.Where(x => empIds.Contains(x.EmployeeId));
            }

            List<DayOffConfigEmployeeEntity> allRows = await query
                .OrderBy(x => x.EmployeeId)
                .ThenBy(x => x.DayOffConfigId)
                .ToListAsync(cancellationToken);

            if (allRows.Count == 0)
                return new PagedResult<LeaveBalanceReportDto>([], 0, request.PageIndex, request.PageSize);

            List<LeaveBalanceReportDto> mapped = await MapReportAsync(allRows, expiresOn, cancellationToken);

            if (request.OnlyWithRemaining)
                mapped = mapped.Where(x => x.RemainingDays > 0).ToList();

            int total = mapped.Count;
            List<LeaveBalanceReportDto> page = mapped
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<LeaveBalanceReportDto>(page, total, request.PageIndex, request.PageSize);
        }

        private async Task<List<LeaveBalanceReportDto>> MapReportAsync(
            List<DayOffConfigEmployeeEntity> rows,
            DateOnly expiresOn,
            CancellationToken cancellationToken)
        {
            List<Guid> empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> cfgIds = rows.Select(x => x.DayOffConfigId).Distinct().ToList();
            DateOnly ys = new(expiresOn.Year, 1, 1);
            DateOnly ye = expiresOn;

            var pendingGroups = await _context.RegisterDayOffEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.Status == DayOffStatus.PENDING
                    && empIds.Contains(x.EmployeeId)
                    && x.DayOffConfigId.HasValue
                    && cfgIds.Contains(x.DayOffConfigId.Value)
                    && x.FromDate <= ye && x.ToDate >= ys)
                .GroupBy(x => new { x.EmployeeId, ConfigId = x.DayOffConfigId!.Value })
                .Select(g => new { g.Key.EmployeeId, g.Key.ConfigId, Pending = g.Sum(x => x.TotalDays) })
                .ToListAsync(cancellationToken);

            var pendingMap = pendingGroups.ToDictionary(
                x => (x.EmployeeId, x.ConfigId),
                x => x.Pending);

            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    Name = x.FullName ?? ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim(),
                    x.CompanyId,
                    x.BranchId,
                    x.DepartmentId,
                })
                .ToListAsync(cancellationToken);
            var empMap = employees.ToDictionary(x => x.Id);

            var branchIds = employees.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            var deptIds = employees.Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();

            var branchMap = branchIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var deptMap = deptIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.DepartmentEntities.AsNoTracking()
                    .Where(x => deptIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var configs = await _context.DayOffConfigEntities.AsNoTracking()
                .Where(x => cfgIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly soonThreshold = today.AddDays(60);

            List<LeaveBalanceReportDto> result = [];
            foreach (var row in rows)
            {
                empMap.TryGetValue(row.EmployeeId, out var emp);
                configs.TryGetValue(row.DayOffConfigId, out var cfgName);
                pendingMap.TryGetValue((row.EmployeeId, row.DayOffConfigId), out decimal pending);
                decimal remaining = Math.Max(0, row.AllocatedDays - row.UsedDays - pending);

                result.Add(new LeaveBalanceReportDto
                {
                    Id = row.Id,
                    DayOffConfigId = row.DayOffConfigId,
                    DayOffConfigName = cfgName,
                    EmployeeId = row.EmployeeId,
                    EmployeeName = emp?.Name,
                    EmployeeCode = emp?.Code,
                    CompanyId = emp?.CompanyId,
                    BranchId = emp?.BranchId,
                    BranchName = emp?.BranchId is Guid bid && branchMap.TryGetValue(bid, out var bn) ? bn : null,
                    DepartmentId = emp?.DepartmentId,
                    DepartmentName = emp?.DepartmentId is Guid did && deptMap.TryGetValue(did, out var dn) ? dn : null,
                    Year = row.Year,
                    AllocatedDays = row.AllocatedDays,
                    UsedDays = row.UsedDays,
                    RemainingDays = remaining,
                    PendingDays = pending,
                    ExpiresOn = expiresOn,
                    IsExpiringSoon = remaining > 0 && expiresOn <= soonThreshold,
                    Note = row.Note,
                });
            }

            return result
                .OrderByDescending(x => x.RemainingDays)
                .ThenBy(x => x.EmployeeCode)
                .ToList();
        }
    }
}

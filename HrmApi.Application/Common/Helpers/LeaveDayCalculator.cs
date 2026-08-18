using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Common.Helpers
{
    public static class LeaveDayCalculator
    {
        public static async Task<decimal> CountWorkingDaysAsync(
            IApplicationDbContext context,
            DateOnly from,
            DateOnly to,
            Guid? companyId,
            LeaveSession session = LeaveSession.FULL,
            CancellationToken cancellationToken = default)
        {
            SaturdayPolicy satPolicy = await ResolveSaturdayPolicyAsync(context, companyId, cancellationToken);
            HashSet<DateOnly> holidays = await LoadHolidayDatesAsync(context, from, to, companyId, cancellationToken);

            decimal total = 0;
            for (DateOnly d = from; d <= to; d = d.AddDays(1))
            {
                decimal weight = DayWeight(d, satPolicy, holidays);
                if (weight <= 0) continue;

                if (session is LeaveSession.AM or LeaveSession.PM)
                {
                    total += Math.Min(0.5m, weight);
                }
                else
                {
                    total += weight;
                }
            }

            return total;
        }

        public static async Task<List<DateOnly>> GetWorkingDatesAsync(
            IApplicationDbContext context,
            DateOnly from,
            DateOnly to,
            Guid? companyId,
            CancellationToken cancellationToken = default)
        {
            SaturdayPolicy satPolicy = await ResolveSaturdayPolicyAsync(context, companyId, cancellationToken);
            HashSet<DateOnly> holidays = await LoadHolidayDatesAsync(context, from, to, companyId, cancellationToken);
            List<DateOnly> dates = [];
            for (DateOnly d = from; d <= to; d = d.AddDays(1))
            {
                if (DayWeight(d, satPolicy, holidays) > 0)
                {
                    dates.Add(d);
                }
            }
            return dates;
        }

        public static decimal DayWeight(DateOnly d, SaturdayPolicy satPolicy, HashSet<DateOnly> holidays)
        {
            if (holidays.Contains(d)) return 0;
            if (d.DayOfWeek == DayOfWeek.Sunday) return 0;
            if (d.DayOfWeek == DayOfWeek.Saturday)
            {
                return satPolicy switch
                {
                    SaturdayPolicy.Off => 0,
                    SaturdayPolicy.Half => 0.5m,
                    _ => 1m,
                };
            }
            return 1m;
        }

        public static async Task<SaturdayPolicy> ResolveSaturdayPolicyAsync(
            IApplicationDbContext context,
            Guid? companyId,
            CancellationToken cancellationToken)
        {
            if (!companyId.HasValue) return SaturdayPolicy.Work;
            SaturdayPolicy? policy = await context.CompanyEntities.AsNoTracking()
                .Where(x => x.Id == companyId.Value)
                .Select(x => (SaturdayPolicy?)x.SaturdayPolicy)
                .FirstOrDefaultAsync(cancellationToken);
            return policy ?? SaturdayPolicy.Work;
        }

        private static async Task<HashSet<DateOnly>> LoadHolidayDatesAsync(
            IApplicationDbContext context,
            DateOnly from,
            DateOnly to,
            Guid? companyId,
            CancellationToken cancellationToken)
        {
            List<PublicHolidayEntity> holidays = await context.PublicHolidayEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive
                    && (x.CompanyId == null || (companyId.HasValue && x.CompanyId == companyId)))
                .ToListAsync(cancellationToken);

            HashSet<DateOnly> set = [];
            for (DateOnly d = from; d <= to; d = d.AddDays(1))
            {
                foreach (PublicHolidayEntity h in holidays)
                {
                    if (h.IsRecurringYearly)
                    {
                        if (h.HolidayDate.Month == d.Month && h.HolidayDate.Day == d.Day)
                            set.Add(d);
                    }
                    else if (h.HolidayDate == d)
                    {
                        set.Add(d);
                    }
                }
            }
            return set;
        }
    }

    public static class LeaveApproverResolver
    {
        public static async Task<Guid?> ResolveAsync(
            IApplicationDbContext context,
            Guid employeeId,
            CancellationToken cancellationToken)
        {
            var emp = await context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == employeeId && !x.IsDeleted)
                .Select(x => new { x.PartId, x.DepartmentId, x.BranchId, x.CompanyId })
                .FirstOrDefaultAsync(cancellationToken);
            if (emp == null) return null;

            if (emp.PartId.HasValue)
            {
                Guid? partMgr = await context.PartEntities.AsNoTracking()
                    .Where(x => x.Id == emp.PartId.Value && !x.IsDeleted)
                    .Select(x => x.ManagerId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (partMgr.HasValue && partMgr != Guid.Empty && partMgr != employeeId)
                    return partMgr;
            }

            if (emp.DepartmentId.HasValue)
            {
                Guid? deptMgr = await context.DepartmentEntities.AsNoTracking()
                    .Where(x => x.Id == emp.DepartmentId.Value && !x.IsDeleted)
                    .Select(x => x.ManagerId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (deptMgr.HasValue && deptMgr != Guid.Empty && deptMgr != employeeId)
                    return deptMgr;
            }

            if (emp.BranchId.HasValue)
            {
                Guid? branchMgr = await context.BranchEntities.AsNoTracking()
                    .Where(x => x.Id == emp.BranchId.Value && !x.IsDeleted)
                    .Select(x => x.ManagerId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (branchMgr.HasValue && branchMgr != Guid.Empty && branchMgr != employeeId)
                    return branchMgr;
            }
            else if (emp.CompanyId.HasValue)
            {
                Guid? hqMgr = await context.BranchEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.CompanyId == emp.CompanyId && x.ManagerId != null)
                    .OrderByDescending(x => x.IsHeadQuarter)
                    .Select(x => x.ManagerId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (hqMgr.HasValue && hqMgr != Guid.Empty && hqMgr != employeeId)
                    return hqMgr;
            }

            return null;
        }
    }

    public static class LeaveBalanceHelper
    {
        public static async Task EnsureBalanceAsync(
            IApplicationDbContext context,
            Guid employeeId,
            Guid? companyId,
            DayOffConfigEntity config,
            decimal requestDays,
            int year,
            CancellationToken cancellationToken)
        {
            if (!config.DeductBalance)
                return;

            DayOffConfigEmployeeEntity? allocation = await context.DayOffConfigEmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId
                    && x.DayOffConfigId == config.Id
                    && x.Year == year
                    && !x.IsDeleted, cancellationToken);

            if (allocation == null && config.DefaultDaysPerYear <= 0)
            {
                throw new InvalidOperationException(
                    "Nhân viên chưa được cấp quỹ phép năm. Vui lòng liên hệ HR cấp phát trước khi đăng ký.");
            }

            decimal remaining = await GetRemainingAsync(
                context, employeeId, companyId, config.Id, year, cancellationToken);
            if (requestDays > remaining)
            {
                throw new InvalidOperationException(
                    $"Số ngày phép còn lại không đủ (còn {remaining:0.##} ngày, yêu cầu {requestDays:0.##} ngày).");
            }
        }

        public static async Task<decimal> GetRemainingAsync(
            IApplicationDbContext context,
            Guid employeeId,
            Guid? companyId,
            Guid? dayOffConfigId,
            int year,
            CancellationToken cancellationToken)
        {
            DayOffConfigEntity? config = await ResolveConfigAsync(
                context, companyId, dayOffConfigId, cancellationToken);

            DateOnly yearStart = new(year, 1, 1);
            DateOnly yearEnd = new(year, 12, 31);

            Guid? targetConfigId = config?.Id ?? dayOffConfigId;

            decimal usedOrPending = await context.RegisterDayOffEntities.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId
                    && !x.IsDeleted
                    && (targetConfigId == null || x.DayOffConfigId == targetConfigId)
                    && (x.Status == DayOffStatus.PENDING || x.Status == DayOffStatus.APPROVED)
                    && x.FromDate <= yearEnd
                    && x.ToDate >= yearStart)
                .SumAsync(x => x.TotalDays, cancellationToken);

            decimal total = 0;
            if (config != null)
            {
                DayOffConfigEmployeeEntity? allocation = await context.DayOffConfigEmployeeEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeId == employeeId
                        && x.DayOffConfigId == config.Id
                        && x.Year == year
                        && !x.IsDeleted, cancellationToken);

                if (allocation != null)
                {
                    total = allocation.AllocatedDays;
                }
                else
                {
                    total = config.DefaultDaysPerYear;
                }
            }

            return Math.Max(0, total - usedOrPending);
        }

        public static async Task ApplyApprovedUsageAsync(
            IApplicationDbContext context,
            RegisterDayOffEntity leave,
            CancellationToken cancellationToken)
        {
            DayOffConfigEntity? config = await ResolveConfigAsync(
                context, leave.CompanyId, leave.DayOffConfigId, cancellationToken, track: true);
            if (config == null || !config.DeductBalance)
                return;

            int year = leave.FromDate.Year;
            DayOffConfigEmployeeEntity? allocation = await context.DayOffConfigEmployeeEntities
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == leave.EmployeeId
                    && x.DayOffConfigId == config.Id
                    && x.Year == year
                    && !x.IsDeleted, cancellationToken);

            if (allocation == null)
            {
                allocation = new DayOffConfigEmployeeEntity
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = leave.EmployeeId,
                    DayOffConfigId = config.Id,
                    Year = year,
                    AllocatedDays = config.DefaultDaysPerYear,
                    UsedDays = 0,
                    RemainingDays = config.DefaultDaysPerYear,
                    CreatedBy = leave.UpdatedBy ?? Guid.Empty,
                    CreatedAt = DateTime.UtcNow,
                };
                _ = context.DayOffConfigEmployeeEntities.Add(allocation);
            }

            allocation.UsedDays += leave.TotalDays;
            allocation.RemainingDays = Math.Max(0, allocation.AllocatedDays - allocation.UsedDays);
            allocation.UpdatedAt = DateTime.UtcNow;
            leave.DayOffConfigId ??= config.Id;
        }

        public static async Task RevertApprovedUsageAsync(
            IApplicationDbContext context,
            RegisterDayOffEntity leave,
            CancellationToken cancellationToken)
        {
            DayOffConfigEntity? config = await ResolveConfigAsync(
                context, leave.CompanyId, leave.DayOffConfigId, cancellationToken, track: true);
            if (config == null || !config.DeductBalance)
                return;

            int year = leave.FromDate.Year;
            DayOffConfigEmployeeEntity? allocation = await context.DayOffConfigEmployeeEntities
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == leave.EmployeeId
                    && x.DayOffConfigId == config.Id
                    && x.Year == year
                    && !x.IsDeleted, cancellationToken);
            if (allocation == null) return;

            allocation.UsedDays = Math.Max(0, allocation.UsedDays - leave.TotalDays);
            allocation.RemainingDays = Math.Max(0, allocation.AllocatedDays - allocation.UsedDays);
            allocation.UpdatedAt = DateTime.UtcNow;
        }

        public static Task EnsureAnnualBalanceAsync(
            IApplicationDbContext context,
            Guid employeeId,
            Guid? companyId,
            Guid? dayOffConfigId,
            decimal requestDays,
            int year,
            CancellationToken cancellationToken)
            => EnsureBalanceForTypeInternalAsync(context, employeeId, companyId, dayOffConfigId, requestDays, year, cancellationToken);

        private static async Task EnsureBalanceForTypeInternalAsync(
            IApplicationDbContext context,
            Guid employeeId,
            Guid? companyId,
            Guid? dayOffConfigId,
            decimal requestDays,
            int year,
            CancellationToken cancellationToken)
        {
            DayOffConfigEntity? config = await ResolveConfigAsync(
                context, companyId, dayOffConfigId, cancellationToken);
            if (config == null) return;
            await EnsureBalanceAsync(context, employeeId, companyId, config, requestDays, year, cancellationToken);
        }

        public static Task<decimal> GetAnnualRemainingAsync(
            IApplicationDbContext context,
            Guid employeeId,
            Guid? companyId,
            Guid? dayOffConfigId,
            int year,
            CancellationToken cancellationToken)
            => GetRemainingAsync(context, employeeId, companyId, dayOffConfigId, year, cancellationToken);

        private static async Task<DayOffConfigEntity?> ResolveConfigAsync(
            IApplicationDbContext context,
            Guid? companyId,
            Guid? dayOffConfigId,
            CancellationToken cancellationToken,
            bool track = false)
        {
            IQueryable<DayOffConfigEntity> query = track
                ? context.DayOffConfigEntities
                : context.DayOffConfigEntities.AsNoTracking();

            if (dayOffConfigId.HasValue && dayOffConfigId.Value != Guid.Empty)
            {
                DayOffConfigEntity? byId = await query
                    .FirstOrDefaultAsync(x => x.Id == dayOffConfigId.Value && !x.IsDeleted, cancellationToken);
                if (byId != null) return byId;
            }

            var byCompany = await query
                .Where(x => !x.IsDeleted && x.IsActive
                    && (x.CompanyId == null || (companyId.HasValue && x.CompanyId == companyId)))
                .OrderByDescending(x => x.CompanyId.HasValue)
                .FirstOrDefaultAsync(cancellationToken);

            if (byCompany != null) return byCompany;

            return await query
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}

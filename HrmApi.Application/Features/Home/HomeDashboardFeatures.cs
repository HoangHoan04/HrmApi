using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Home;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Home
{
    public class GetHomeDashboardQuery : IRequest<HomeDashboardDto>
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }

    public class GetHomeDashboardQueryHandler : IRequestHandler<GetHomeDashboardQuery, HomeDashboardDto>
    {
        private readonly IApplicationDbContext _context;

        public GetHomeDashboardQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HomeDashboardDto> Handle(GetHomeDashboardQuery request, CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var toDate = request.ToDate ?? today;
            var fromDate = request.FromDate ?? new DateOnly(toDate.Year, toDate.Month, 1);
            if (fromDate > toDate)
            {
                (fromDate, toDate) = (toDate, fromDate);
            }

            var fromDt = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var toDtExclusive = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var rangeDays = Math.Max(1, toDate.DayNumber - fromDate.DayNumber + 1);
            var prevTo = fromDate.AddDays(-1);
            var prevFrom = prevTo.AddDays(-(rangeDays - 1));
            var prevFromDt = prevFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var prevToExclusive = fromDt;
            var expiringUntil = toDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(30);

            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.Gender,
                    x.DepartmentId,
                    x.JoinDate,
                    x.CreatedAt,
                })
                .ToListAsync(cancellationToken);

            var total = employees.Count;
            var female = employees.Count(x => IsFemale(x.Gender));
            var male = employees.Count(x => IsMale(x.Gender));
            var otherGender = Math.Max(0, total - female - male);

            var newInPeriod = employees.Count(x => x.JoinDate >= fromDt && x.JoinDate < toDtExclusive);
            var newPrevPeriod = employees.Count(x => x.JoinDate >= prevFromDt && x.JoinDate < prevToExclusive);
            var hireChange = newPrevPeriod == 0
                ? (newInPeriod > 0 ? 100m : 0m)
                : Math.Round((newInPeriod - newPrevPeriod) * 100m / newPrevPeriod, 1);

            var deptIds = employees
                .Where(x => x.DepartmentId.HasValue)
                .Select(x => x.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var deptNames = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => deptIds.Contains(x.Id) && !x.IsDeleted)
                .Select(x => new { x.Id, x.Name })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            var departmentHeadcount = employees
                .GroupBy(x => x.DepartmentId)
                .Select(g =>
                {
                    string name = g.Key.HasValue && deptNames.TryGetValue(g.Key.Value, out var n)
                        ? n
                        : "Chưa gán phòng ban";
                    return new HomeNamedCountDto
                    {
                        Key = g.Key?.ToString() ?? "NONE",
                        Name = name,
                        Count = g.Count(),
                    };
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var yearFrom = toDate.Year - 4;
            var headcountByYear = Enumerable.Range(yearFrom, 5)
                .Select(year => new HomeNamedCountDto
                {
                    Key = year.ToString(),
                    Name = year.ToString(),
                    Count = employees.Count(e => e.JoinDate.Year <= year),
                })
                .ToList();

            var monthCursor = new DateTime(fromDate.Year, fromDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = new DateTime(toDate.Year, toDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var hireMonths = new List<HomeNamedCountDto>();
            while (monthCursor <= monthEnd && hireMonths.Count < 12)
            {
                var next = monthCursor.AddMonths(1);
                hireMonths.Add(new HomeNamedCountDto
                {
                    Key = monthCursor.ToString("yyyy-MM"),
                    Name = $"T{monthCursor.Month}/{monthCursor.Year % 100:00}",
                    Count = employees.Count(e => e.JoinDate >= monthCursor && e.JoinDate < next),
                });
                monthCursor = next;
            }

            var pendingLeave = await _context.RegisterDayOffEntities.AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.Status == DayOffStatus.PENDING, cancellationToken);

            var pendingComplaints = await _context.AttendanceComplaintEntities.AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.Status == AttendanceComplaintStatus.PENDING, cancellationToken);

            var pendingTransfers = await _context.TransferEmployeeEntities.AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.Status == TransferStatus.Pending, cancellationToken);

            var pendingReviews = await _context.ReviewRenewalEntities.AsNoTracking()
                .CountAsync(x =>
                        !x.IsDeleted
                        && (x.Status == ReviewRenewalStatus.PendingReview
                            || x.Status == ReviewRenewalStatus.PendingApproval),
                    cancellationToken);

            var contracts = await _context.ContractEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new { x.Status, x.EndDate })
                .ToListAsync(cancellationToken);

            var activeContracts = contracts.Count(x =>
                string.Equals(x.Status, ContractStatus.Active, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Status, ContractStatus.ExpiringSoon, StringComparison.OrdinalIgnoreCase));
            var pendingSign = contracts.Count(x =>
                string.Equals(x.Status, ContractStatus.PendingSign, StringComparison.OrdinalIgnoreCase));
            var expired = contracts.Count(x =>
                string.Equals(x.Status, ContractStatus.Expired, StringComparison.OrdinalIgnoreCase));
            var expiringSoon = contracts.Count(x =>
                x.EndDate.HasValue
                && x.EndDate.Value.Date >= toDate.ToDateTime(TimeOnly.MinValue).Date
                && x.EndDate.Value.Date <= expiringUntil.Date
                && !string.Equals(x.Status, ContractStatus.Terminated, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.Status, ContractStatus.Liquidated, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.Status, ContractStatus.Expired, StringComparison.OrdinalIgnoreCase));

            var periodTk = await _context.TimekeepingEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.WorkDate >= fromDate && x.WorkDate <= toDate)
                .Select(x => x.Status)
                .ToListAsync(cancellationToken);

            var present = periodTk.Count(s => s is AttendanceStatus.ON_TIME or AttendanceStatus.EARLY);
            var late = periodTk.Count(s => s == AttendanceStatus.LATE);
            var absent = periodTk.Count(s => s == AttendanceStatus.ABSENT);
            var onLeave = periodTk.Count(s => s == AttendanceStatus.LEAVE);
            var incomplete = periodTk.Count(s => s == AttendanceStatus.INCOMPLETE);
            var tkTotal = periodTk.Count;
            var attendanceRate = tkTotal == 0
                ? 0m
                : Math.Round((present + late) * 100m / tkTotal, 1);

            var leaveInPeriod = await _context.RegisterDayOffEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CreatedAt >= fromDt && x.CreatedAt < toDtExclusive)
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var leaveStatus = Enum.GetValues<DayOffStatus>()
                .Select(st => new HomeNamedCountDto
                {
                    Key = st.ToString(),
                    Name = LeaveStatusName(st),
                    Count = leaveInPeriod.FirstOrDefault(x => x.Status == st)?.Count ?? 0,
                })
                .ToList();

            return new HomeDashboardDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                Kpis = new HomeKpiDto
                {
                    TotalEmployees = total,
                    FemaleEmployees = female,
                    MaleEmployees = male,
                    OtherGenderEmployees = otherGender,
                    NewHiresThisMonth = newInPeriod,
                    NewHiresLastMonth = newPrevPeriod,
                    NewHiresChangePercent = hireChange,
                    ActiveContracts = activeContracts,
                },
                Pending = new HomePendingDto
                {
                    LeaveRequests = pendingLeave,
                    AttendanceComplaints = pendingComplaints,
                    Transfers = pendingTransfers,
                    ReviewRenewals = pendingReviews,
                },
                AttendanceToday = new HomeAttendanceTodayDto
                {
                    Present = present,
                    Late = late,
                    Absent = absent,
                    OnLeave = onLeave,
                    Incomplete = incomplete,
                    TotalRecords = tkTotal,
                    AttendanceRatePercent = attendanceRate,
                },
                Contracts = new HomeContractSnapshotDto
                {
                    Active = activeContracts,
                    PendingSign = pendingSign,
                    ExpiringIn30Days = expiringSoon,
                    Expired = expired,
                },
                GenderBreakdown =
                [
                    new HomeNamedCountDto { Key = "FEMALE", Name = "Nữ", Count = female },
                    new HomeNamedCountDto { Key = "MALE", Name = "Nam", Count = male },
                    new HomeNamedCountDto { Key = "OTHER", Name = "Khác", Count = otherGender },
                ],
                DepartmentHeadcount = departmentHeadcount,
                HeadcountByYear = headcountByYear,
                LeaveStatusThisMonth = leaveStatus,
                NewHiresByMonth = hireMonths,
            };
        }

        private static bool IsFemale(string? gender) =>
            string.Equals(gender, "FEMALE", StringComparison.OrdinalIgnoreCase)
            || string.Equals(gender, "F", StringComparison.OrdinalIgnoreCase)
            || string.Equals(gender, "NỮ", StringComparison.OrdinalIgnoreCase)
            || string.Equals(gender, "NU", StringComparison.OrdinalIgnoreCase);

        private static bool IsMale(string? gender) =>
            string.Equals(gender, "MALE", StringComparison.OrdinalIgnoreCase)
            || string.Equals(gender, "M", StringComparison.OrdinalIgnoreCase)
            || string.Equals(gender, "NAM", StringComparison.OrdinalIgnoreCase);

        private static string LeaveStatusName(DayOffStatus status) => status switch
        {
            DayOffStatus.PENDING => "Chờ duyệt",
            DayOffStatus.APPROVED => "Đã duyệt",
            DayOffStatus.REJECTED => "Từ chối",
            DayOffStatus.CANCELLED => "Đã hủy",
            _ => status.ToString(),
        };
    }
}

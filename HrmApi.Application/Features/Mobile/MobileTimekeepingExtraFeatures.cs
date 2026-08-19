using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.DTOs.OvertimeRequest;
using HrmApi.Application.DTOs.Timekeeping;
using HrmApi.Application.Features.OvertimeRequests;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Mobile
{
    public class GetMobileTeamMonthQuery : MobileTeamMonthQuery, IRequest<MobileTeamMonthDto> { }

    public class GetMobileTeamMonthQueryHandler : IRequestHandler<GetMobileTeamMonthQuery, MobileTeamMonthDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMobileTeamMonthQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<MobileTeamMonthDto> Handle(GetMobileTeamMonthQuery request, CancellationToken cancellationToken)
        {
            if (request.Year < 2000 || request.Month < 1 || request.Month > 12)
            {
                throw new InvalidOperationException("Năm/tháng không hợp lệ.");
            }

            bool allowed = _currentUser.IsAdmin
                || _currentUser.HasPermission(PermissionCodes.OperateLeaveApprove)
                || _currentUser.HasPermission(PermissionCodes.OperateTimekeepingView);
            if (!allowed)
            {
                throw new InvalidOperationException("Bạn không có quyền xem công đội nhóm.");
            }

            Guid managerId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            DateOnly from = new(request.Year, request.Month, 1);
            DateOnly to = from.AddMonths(1).AddDays(-1);

            List<EmployeeEntity> reports = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.DirectManagerId == managerId)
                .OrderBy(x => x.Code)
                .ToListAsync(cancellationToken);

            MobileTeamMonthDto result = new() { Year = request.Year, Month = request.Month };
            if (reports.Count == 0)
            {
                return result;
            }

            List<Guid> empIds = reports.Select(x => x.Id).ToList();
            List<TimekeepingEntity> records = await _context.TimekeepingEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && empIds.Contains(x.EmployeeId)
                    && x.WorkDate >= from
                    && x.WorkDate <= to)
                .OrderBy(x => x.WorkDate)
                .ToListAsync(cancellationToken);

            Dictionary<Guid, List<TimekeepingEntity>> byEmp = records.GroupBy(x => x.EmployeeId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (EmployeeEntity emp in reports)
            {
                List<TimekeepingEntity> days = byEmp.TryGetValue(emp.Id, out List<TimekeepingEntity>? list) ? list : [];
                List<MobileMonthDayDto> dayDtos = days.Select(TimekeepingMapper.ToMonthDayDto).ToList();
                result.Members.Add(new MobileTeamMemberMonthDto
                {
                    EmployeeId = emp.Id,
                    EmployeeCode = emp.Code,
                    EmployeeName = emp.FullName ?? $"{emp.LastName} {emp.FirstName}".Trim(),
                    Days = dayDtos,
                    OnTimeDays = dayDtos.Count(d => d.Status == AttendanceStatus.ON_TIME),
                    LateDays = dayDtos.Count(d => d.Status == AttendanceStatus.LATE),
                    EarlyDays = dayDtos.Count(d => d.Status == AttendanceStatus.EARLY),
                    LeaveDays = dayDtos.Count(d => d.Status == AttendanceStatus.LEAVE),
                    AbsentDays = dayDtos.Count(d => d.Status == AttendanceStatus.ABSENT),
                    IncompleteDays = dayDtos.Count(d => d.Status == AttendanceStatus.INCOMPLETE),
                    TotalWorkedMinutes = dayDtos.Sum(d => d.WorkedMinutes),
                });
            }

            return result;
        }
    }

    public class GetMyOvertimeRequestsQuery : IRequest<List<OvertimeRequestDto>>
    {
        public string? Status { get; set; }
        public int? Year { get; set; }
    }

    public class GetMyOvertimeRequestsQueryHandler : IRequestHandler<GetMyOvertimeRequestsQuery, List<OvertimeRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyOvertimeRequestsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<OvertimeRequestDto>> Handle(GetMyOvertimeRequestsQuery request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);

            IQueryable<OvertimeRequestEntity> query = _context.OvertimeRequestEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EmployeeId == employeeId);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim());
            }

            if (request.Year.HasValue)
            {
                DateOnly from = new(request.Year.Value, 1, 1);
                DateOnly to = new(request.Year.Value, 12, 31);
                query = query.Where(x => x.WorkDate >= from && x.WorkDate <= to);
            }

            List<OvertimeRequestEntity> rows = await query
                .OrderByDescending(x => x.WorkDate)
                .ThenByDescending(x => x.CreatedAt)
                .Take(500)
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return [];
            }

            var emp = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == employeeId)
                .Select(x => new { x.Code, x.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            List<Guid> branchIds = rows.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
            Dictionary<Guid, string> branches = branchIds.Count == 0
                ? []
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            List<Guid> approverIds = rows.Where(x => x.ApproverId.HasValue).Select(x => x.ApproverId!.Value).Distinct().ToList();
            Dictionary<Guid, string> approvers = approverIds.Count == 0
                ? []
                : await _context.EmployeeEntities.AsNoTracking()
                    .Where(x => approverIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.FullName ?? (x.LastName + " " + x.FirstName).Trim(), cancellationToken);

            return rows.Select(x => OvertimeRequestMapper.ToDto(
                x,
                emp?.FullName,
                emp?.Code,
                x.BranchId.HasValue && branches.TryGetValue(x.BranchId.Value, out var bn) ? bn : null,
                x.ApproverId.HasValue && approvers.TryGetValue(x.ApproverId.Value, out var an) ? an : null)).ToList();
        }
    }

    public class CreateMyOvertimeRequestCommand : IRequest<Guid>
    {
        public DateOnly WorkDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public int? RequestedMinutes { get; set; }
        public string OtType { get; set; } = OvertimeType.AfterShift;
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public bool Submit { get; set; } = true;
    }

    public class CreateMyOvertimeRequestCommandHandler : IRequestHandler<CreateMyOvertimeRequestCommand, Guid>
    {
        private readonly IMediator _mediator;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CreateMyOvertimeRequestCommandHandler(
            IMediator mediator,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateMyOvertimeRequestCommand request, CancellationToken cancellationToken)
        {
            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            return await _mediator.Send(new CreateOvertimeRequestCommand
            {
                EmployeeId = employeeId,
                WorkDate = request.WorkDate,
                FromTime = request.FromTime,
                ToTime = request.ToTime,
                RequestedMinutes = request.RequestedMinutes,
                OtType = request.OtType,
                Reason = request.Reason,
                AttachmentUrl = request.AttachmentUrl,
                Submit = request.Submit,
            }, cancellationToken);
        }
    }
}

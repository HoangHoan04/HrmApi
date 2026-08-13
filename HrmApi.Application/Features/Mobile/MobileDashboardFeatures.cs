using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Mobile
{
    public class GetMobileManagerSummaryQuery : IRequest<MobileManagerSummaryDto> { }

    public class GetMobileManagerSummaryQueryHandler
        : IRequestHandler<GetMobileManagerSummaryQuery, MobileManagerSummaryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMobileManagerSummaryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<MobileManagerSummaryDto> Handle(
            GetMobileManagerSummaryQuery request,
            CancellationToken cancellationToken)
        {
            Guid managerId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);

            int pendingLeave = await _context.RegisterDayOffEntities.AsNoTracking()
                .CountAsync(x => !x.IsDeleted
                    && x.Status == DayOffStatus.PENDING
                    && x.RequestedApproverId == managerId, cancellationToken);

            DateOnly today = BusinessDateHelper.Today();
            DateOnly monthStart = new(today.Year, today.Month, 1);
            DateOnly monthEnd = monthStart.AddMonths(1).AddDays(-1);

            List<Guid> reportIds = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.DirectManagerId == managerId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            int teamLate = reportIds.Count == 0
                ? 0
                : await _context.TimekeepingEntities.AsNoTracking()
                    .CountAsync(x => !x.IsDeleted
                        && reportIds.Contains(x.EmployeeId)
                        && x.WorkDate >= monthStart
                        && x.WorkDate <= monthEnd
                        && x.Status == AttendanceStatus.LATE, cancellationToken);

            int? expiring = null;
            bool isHr = _currentUser.IsAdmin
                || _currentUser.HasPermission(PermissionCodes.HrContractView)
                || _currentUser.HasPermission(PermissionCodes.HrEmployeeView)
                || _currentUser.HasPermission(PermissionCodes.OrgManage);
            if (isHr)
            {
                Guid? companyId = _currentUser.CompanyId;
                if (!companyId.HasValue || companyId == Guid.Empty)
                {
                    companyId = await _context.EmployeeEntities.AsNoTracking()
                        .Where(x => x.Id == managerId)
                        .Select(x => x.CompanyId)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                if (companyId.HasValue && companyId != Guid.Empty)
                {
                    DateTime horizon = DateTime.UtcNow.Date.AddDays(30);
                    expiring = await _context.ContractEntities.AsNoTracking()
                        .CountAsync(x => !x.IsDeleted
                            && x.CompanyId == companyId
                            && (x.Status == ContractStatus.Active || x.Status == ContractStatus.ExpiringSoon)
                            && x.EndDate.HasValue
                            && x.EndDate.Value.Date <= horizon
                            && x.EndDate.Value.Date >= DateTime.UtcNow.Date, cancellationToken);
                }
                else
                {
                    expiring = 0;
                }
            }

            return new MobileManagerSummaryDto
            {
                PendingLeaveApprovals = pendingLeave,
                TeamLateThisMonth = teamLate,
                ExpiringContractsCount = expiring,
            };
        }
    }
}

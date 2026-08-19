using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Mobile;
using HrmApi.Application.Features.RegisterDayOffs.Queries;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Mobile
{
    public class GetMobileTeamCalendarQuery : MobileTeamCalendarQuery, IRequest<List<LeaveCalendarEventDto>> { }

    public class GetMobileTeamCalendarQueryHandler
        : IRequestHandler<GetMobileTeamCalendarQuery, List<LeaveCalendarEventDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IMediator _mediator;

        public GetMobileTeamCalendarQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IMediator mediator)
        {
            _context = context;
            _currentUser = currentUser;
            _mediator = mediator;
        }

        public async Task<List<LeaveCalendarEventDto>> Handle(
            GetMobileTeamCalendarQuery request,
            CancellationToken cancellationToken)
        {
            if (request.To < request.From)
            {
                throw new InvalidOperationException("Đến ngày phải >= Từ ngày.");
            }

            Guid employeeId = await MobileEmployeeHelper.ResolveEmployeeIdAsync(_context, _currentUser, cancellationToken);
            var me = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => x.Id == employeeId && !x.IsDeleted)
                .Select(x => new { x.CompanyId, x.BranchId })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");

            return !me.CompanyId.HasValue || me.CompanyId == Guid.Empty
                ? throw new InvalidOperationException("Nhân viên chưa gắn công ty.")
                : await _mediator.Send(new GetLeaveCalendarRangeQuery
                {
                    FromDate = request.From,
                    ToDate = request.To,
                    CompanyId = me.CompanyId,
                    BranchId = me.BranchId,
                    Status = DayOffStatus.APPROVED,
                    IncludeHolidays = true,
                    IncludePending = false,
                }, cancellationToken);
        }
    }
}

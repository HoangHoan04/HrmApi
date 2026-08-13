using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Entities.Settings;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrmApi.Application.Features.Settings
{
    public class GetReportSchedulesPagedQuery : SettingsPagedQuery, IRequest<PagedResult<ReportScheduleDto>>
    {
        public string? ReportType { get; set; }
    }

    public class GetReportSchedulesPagedQueryHandler : IRequestHandler<GetReportSchedulesPagedQuery, PagedResult<ReportScheduleDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetReportSchedulesPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<ReportScheduleDto>> Handle(GetReportSchedulesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ReportScheduleEntity> query = _context.ReportScheduleEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);
            if (!string.IsNullOrWhiteSpace(request.ReportType))
                query = query.Where(x => x.ReportType == request.ReportType.Trim().ToUpperInvariant());
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s) || x.EmailTo.ToLower().Contains(s));
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<ReportScheduleEntity> rows = await query.OrderBy(x => x.Code)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<ReportScheduleDto>(rows.Select(ToDto).ToList(), total, pageIndex, pageSize);
        }

        internal static ReportScheduleDto ToDto(ReportScheduleEntity e) => new()
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
            ReportType = e.ReportType,
            CronHint = e.CronHint,
            EmailTo = e.EmailTo,
            IsActive = e.IsActive,
            LastRunAt = e.LastRunAt,
            Note = e.Note,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
        };
    }

    public class GetReportScheduleByIdQuery : SettingsIdRequest, IRequest<ReportScheduleDto?> { }

    public class GetReportScheduleByIdQueryHandler : IRequestHandler<GetReportScheduleByIdQuery, ReportScheduleDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetReportScheduleByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ReportScheduleDto?> Handle(GetReportScheduleByIdQuery request, CancellationToken cancellationToken)
        {
            ReportScheduleEntity? e = await _context.ReportScheduleEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : GetReportSchedulesPagedQueryHandler.ToDto(e);
        }
    }

    public class CreateReportScheduleCommand : ReportScheduleCommandFields, IRequest<Guid> { }

    public class CreateReportScheduleCommandHandler : IRequestHandler<CreateReportScheduleCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateReportScheduleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateReportScheduleCommand request, CancellationToken cancellationToken)
        {
            Validate(request);
            string code = request.Code!.Trim().ToUpperInvariant();
            if (await _context.ReportScheduleEntities.AnyAsync(x => !x.IsDeleted && x.Code == code, cancellationToken))
                throw new InvalidOperationException("Mã lịch báo cáo đã tồn tại.");

            ReportScheduleEntity entity = new()
            {
                Code = code,
                Name = request.Name!.Trim(),
                ReportType = NormalizeReportType(request.ReportType),
                CronHint = string.IsNullOrWhiteSpace(request.CronHint) ? ReportCronHint.Daily : request.CronHint.Trim().ToUpperInvariant(),
                EmailTo = request.EmailTo!.Trim(),
                IsActive = request.IsActive ?? true,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.ReportScheduleEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal static void Validate(ReportScheduleCommandFields request)
        {
            if (string.IsNullOrWhiteSpace(request.Code)) throw new InvalidOperationException("Mã lịch báo cáo là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Tên lịch báo cáo là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.EmailTo)) throw new InvalidOperationException("Email nhận báo cáo là bắt buộc.");
        }

        internal static string NormalizeReportType(string? value)
        {
            string t = (value ?? ReportScheduleType.ContractExpiry).Trim().ToUpperInvariant();
            return t is ReportScheduleType.ContractExpiry or ReportScheduleType.LeaveBalance or ReportScheduleType.PayrollPeriod
                ? t
                : throw new InvalidOperationException("ReportType không hợp lệ.");
        }
    }

    public class UpdateReportScheduleCommand : ReportScheduleCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateReportScheduleCommandHandler : IRequestHandler<UpdateReportScheduleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateReportScheduleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateReportScheduleCommand request, CancellationToken cancellationToken)
        {
            ReportScheduleEntity? entity = await _context.ReportScheduleEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            CreateReportScheduleCommandHandler.Validate(request);
            string code = request.Code!.Trim().ToUpperInvariant();
            if (await _context.ReportScheduleEntities.AnyAsync(x => !x.IsDeleted && x.Code == code && x.Id != request.Id, cancellationToken))
                throw new InvalidOperationException("Mã lịch báo cáo đã tồn tại.");

            entity.Code = code;
            entity.Name = request.Name!.Trim();
            entity.ReportType = CreateReportScheduleCommandHandler.NormalizeReportType(request.ReportType);
            entity.CronHint = string.IsNullOrWhiteSpace(request.CronHint) ? entity.CronHint : request.CronHint.Trim().ToUpperInvariant();
            entity.EmailTo = request.EmailTo!.Trim();
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteReportScheduleCommand : SettingsIdRequest, IRequest<bool> { }

    public class DeleteReportScheduleCommandHandler : IRequestHandler<DeleteReportScheduleCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteReportScheduleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteReportScheduleCommand request, CancellationToken cancellationToken)
        {
            ReportScheduleEntity? entity = await _context.ReportScheduleEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class RunDueReportSchedulesCommand : IRequest<int> { }

    public class RunDueReportSchedulesCommandHandler : IRequestHandler<RunDueReportSchedulesCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _email;
        private readonly ILogger<RunDueReportSchedulesCommandHandler> _logger;

        public RunDueReportSchedulesCommandHandler(
            IApplicationDbContext context,
            IEmailService email,
            ILogger<RunDueReportSchedulesCommandHandler> logger)
        {
            _context = context;
            _email = email;
            _logger = logger;
        }

        public async Task<int> Handle(RunDueReportSchedulesCommand request, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            DateTime today = now.Date;

            List<ReportScheduleEntity> due = await _context.ReportScheduleEntities
                .Where(x => !x.IsDeleted && x.IsActive
                    && (x.LastRunAt == null || x.LastRunAt.Value.Date < today))
                .ToListAsync(cancellationToken);

            int ran = 0;
            foreach (ReportScheduleEntity schedule in due)
            {
                if (!IsDue(schedule, today)) continue;

                string subject = $"[HRM] Báo cáo {schedule.ReportType} — {schedule.Name}";
                string body = BuildBody(schedule);
                try
                {
                    foreach (string to in schedule.EmailTo.Split([';', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        await _email.SendEmailAsync(to, subject, body);
                    }
                    schedule.LastRunAt = now;
                    schedule.UpdatedAt = now;
                    ran++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RunDueReportSchedules failed for {Code}", schedule.Code);
                }
            }

            if (ran > 0)
            {
                _ = await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("RunDueReportSchedules: ran={Count}", ran);
            return ran;
        }

        private static bool IsDue(ReportScheduleEntity schedule, DateTime today)
        {
            string hint = (schedule.CronHint ?? ReportCronHint.Daily).ToUpperInvariant();
            if (hint == ReportCronHint.Daily) return true;
            if (hint == ReportCronHint.Weekly) return today.DayOfWeek == DayOfWeek.Monday;
            if (hint == ReportCronHint.Monthly) return today.Day == 1;
            return true;
        }

        private static string BuildBody(ReportScheduleEntity schedule) =>
            $"<p>Báo cáo định kỳ <b>{schedule.Name}</b> ({schedule.ReportType}).</p>" +
            $"<p>Mã: {schedule.Code}</p><p>Thời điểm: {DateTime.UtcNow:u}</p>";
    }
}

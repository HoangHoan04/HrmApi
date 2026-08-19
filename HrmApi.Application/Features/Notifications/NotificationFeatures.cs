using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Notification;
using HrmApi.Domain.Entities.Notification;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Notifications
{
    public class GetNotificationsPagedQuery : PagedRequest, IRequest<PagedResult<NotificationDto>>
    {
        public bool? IsRead { get; set; }
        public string? Type { get; set; }
        public string? Keyword { get; set; }
    }

    public class GetNotificationsPagedQueryHandler : IRequestHandler<GetNotificationsPagedQuery, PagedResult<NotificationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetNotificationsPagedQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsPagedQuery request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUser.UserId ?? Guid.Empty;
            if (currentUserId == Guid.Empty)
            {
                return new PagedResult<NotificationDto>([], 0, request.PageIndex, request.PageSize);
            }

            IQueryable<NotificationEntity> query = _context.NotificationEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.UserId == currentUserId);

            if (request.IsRead.HasValue)
            {
                query = query.Where(x => x.IsRead == request.IsRead.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                query = query.Where(x => x.Type == request.Type.Trim());
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                string kw = request.Keyword.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(kw) || x.Content.ToLower().Contains(kw));
            }

            int total = await query.CountAsync(cancellationToken);
            List<NotificationDto> items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new NotificationDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    EmployeeId = x.EmployeeId,
                    Title = x.Title,
                    Content = x.Content,
                    Type = x.Type,
                    Severity = x.Severity,
                    TargetUrl = x.TargetUrl,
                    TargetType = x.TargetType,
                    TargetId = x.TargetId,
                    DataJson = x.DataJson,
                    IsRead = x.IsRead,
                    ReadAt = x.ReadAt,
                    IsBroadcast = x.IsBroadcast,
                    SenderId = x.SenderId,
                    CreatedAt = x.CreatedAt,
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<NotificationDto>(items, total, request.PageIndex, request.PageSize);
        }
    }

    public class GetUnreadNotificationCountQuery : IRequest<int>
    {
    }

    public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetUnreadNotificationCountQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUser.UserId ?? Guid.Empty;
            return currentUserId == Guid.Empty
                ? 0
                : await _context.NotificationEntities.AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.UserId == currentUserId && !x.IsRead, cancellationToken);
        }
    }

    public class MarkNotificationReadCommand : IRequest<bool>
    {
        public List<Guid>? Ids { get; set; }
        public Guid? Id { get; set; }
    }

    public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public MarkNotificationReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUser.UserId ?? Guid.Empty;
            if (currentUserId == Guid.Empty)
            {
                return false;
            }

            var targetIds = new List<Guid>();
            if (request.Id.HasValue && request.Id != Guid.Empty)
            {
                targetIds.Add(request.Id.Value);
            }

            if (request.Ids != null && request.Ids.Count > 0)
            {
                targetIds.AddRange(request.Ids);
            }

            if (targetIds.Count == 0)
            {
                return false;
            }

            List<NotificationEntity> entities = await _context.NotificationEntities
                .Where(x => !x.IsDeleted && x.UserId == currentUserId && targetIds.Contains(x.Id) && !x.IsRead)
                .ToListAsync(cancellationToken);

            DateTime now = DateTime.UtcNow;
            foreach (NotificationEntity? entity in entities)
            {
                entity.IsRead = true;
                entity.ReadAt = now;
                entity.UpdatedAt = now;
                entity.UpdatedBy = currentUserId;
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class MarkAllNotificationsReadCommand : IRequest<int>
    {
    }

    public class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public MarkAllNotificationsReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUser.UserId ?? Guid.Empty;
            if (currentUserId == Guid.Empty)
            {
                return 0;
            }

            List<NotificationEntity> unreadList = await _context.NotificationEntities
                .Where(x => !x.IsDeleted && x.UserId == currentUserId && !x.IsRead)
                .ToListAsync(cancellationToken);

            DateTime now = DateTime.UtcNow;
            foreach (NotificationEntity? entity in unreadList)
            {
                entity.IsRead = true;
                entity.ReadAt = now;
                entity.UpdatedAt = now;
                entity.UpdatedBy = currentUserId;
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return unreadList.Count;
        }
    }

    public class DeleteNotificationCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public DeleteNotificationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUser.UserId ?? Guid.Empty;
            if (currentUserId == Guid.Empty)
            {
                return false;
            }

            NotificationEntity? entity = await _context.NotificationEntities
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id && x.UserId == currentUserId, cancellationToken);

            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = currentUserId;

            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class SendBroadcastNotificationCommand : BroadcastNotificationDto, IRequest<int>
    {
    }

    public class SendBroadcastNotificationCommandHandler : IRequestHandler<SendBroadcastNotificationCommand, int>
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUser;

        public SendBroadcastNotificationCommandHandler(INotificationService notificationService, ICurrentUserService currentUser)
        {
            _notificationService = notificationService;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(SendBroadcastNotificationCommand request, CancellationToken cancellationToken)
        {
            return await _notificationService.SendBroadcastAsync(request, _currentUser.UserId, cancellationToken);
        }
    }

    public class GetNotificationSettingsQuery : IRequest<NotificationSettingDto>
    {
    }

    public class GetNotificationSettingsQueryHandler : IRequestHandler<GetNotificationSettingsQuery, NotificationSettingDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetNotificationSettingsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<NotificationSettingDto> Handle(GetNotificationSettingsQuery request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUser.UserId ?? Guid.Empty;
            NotificationSettingEntity? setting = await _context.NotificationSettingEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UserId == currentUserId, cancellationToken);

            return setting == null
                ? new NotificationSettingDto
                {
                    UserId = currentUserId,
                    EmailEnabled = true,
                    PushEnabled = true,
                    InAppEnabled = true,
                    NotifyOnLeave = true,
                    NotifyOnOvertime = true,
                    NotifyOnAttendance = true,
                    NotifyOnPayslip = true,
                    NotifyOnContract = true,
                    NotifyOnRecruitment = true,
                }
                : new NotificationSettingDto
                {
                    UserId = setting.UserId,
                    EmailEnabled = setting.EmailEnabled,
                    PushEnabled = setting.PushEnabled,
                    InAppEnabled = setting.InAppEnabled,
                    NotifyOnLeave = setting.NotifyOnLeave,
                    NotifyOnOvertime = setting.NotifyOnOvertime,
                    NotifyOnAttendance = setting.NotifyOnAttendance,
                    NotifyOnPayslip = setting.NotifyOnPayslip,
                    NotifyOnContract = setting.NotifyOnContract,
                    NotifyOnRecruitment = setting.NotifyOnRecruitment,
                };
        }
    }

    public class UpdateNotificationSettingsCommand : NotificationSettingDto, IRequest<bool>
    {
    }

    public class UpdateNotificationSettingsCommandHandler : IRequestHandler<UpdateNotificationSettingsCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public UpdateNotificationSettingsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUser.UserId ?? Guid.Empty;
            if (currentUserId == Guid.Empty)
            {
                return false;
            }

            NotificationSettingEntity? setting = await _context.NotificationSettingEntities
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UserId == currentUserId, cancellationToken);

            if (setting == null)
            {
                setting = new NotificationSettingEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    EmployeeId = _currentUser.EmployeeId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                };
                _ = _context.NotificationSettingEntities.Add(setting);
            }

            setting.EmailEnabled = request.EmailEnabled;
            setting.PushEnabled = request.PushEnabled;
            setting.InAppEnabled = request.InAppEnabled;
            setting.NotifyOnLeave = request.NotifyOnLeave;
            setting.NotifyOnOvertime = request.NotifyOnOvertime;
            setting.NotifyOnAttendance = request.NotifyOnAttendance;
            setting.NotifyOnPayslip = request.NotifyOnPayslip;
            setting.NotifyOnContract = request.NotifyOnContract;
            setting.NotifyOnRecruitment = request.NotifyOnRecruitment;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = currentUserId;

            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

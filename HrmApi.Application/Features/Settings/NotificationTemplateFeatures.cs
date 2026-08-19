using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Entities.Settings;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Settings
{
    public class GetNotificationTemplatesPagedQuery : SettingsPagedQuery, IRequest<PagedResult<NotificationTemplateDto>>
    {
        public string? Channel { get; set; }
    }

    public class GetNotificationTemplatesPagedQueryHandler : IRequestHandler<GetNotificationTemplatesPagedQuery, PagedResult<NotificationTemplateDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetNotificationTemplatesPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<NotificationTemplateDto>> Handle(GetNotificationTemplatesPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<NotificationTemplateEntity> query = _context.NotificationTemplateEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.Channel))
            {
                query = query.Where(x => x.Channel == request.Channel.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || (x.Subject != null && x.Subject.ToLower().Contains(s)));
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<NotificationTemplateEntity> rows = await query.OrderBy(x => x.Code)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<NotificationTemplateDto>(rows.Select(ToDto).ToList(), total, pageIndex, pageSize);
        }

        internal static NotificationTemplateDto ToDto(NotificationTemplateEntity e)
        {
            return new()
            {
                Id = e.Id,
                Code = e.Code,
                Channel = e.Channel,
                Subject = e.Subject,
                Body = e.Body,
                IsActive = e.IsActive,
                Note = e.Note,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            };
        }
    }

    public class GetNotificationTemplateByIdQuery : SettingsIdRequest, IRequest<NotificationTemplateDto?> { }

    public class GetNotificationTemplateByIdQueryHandler : IRequestHandler<GetNotificationTemplateByIdQuery, NotificationTemplateDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetNotificationTemplateByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationTemplateDto?> Handle(GetNotificationTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            NotificationTemplateEntity? e = await _context.NotificationTemplateEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : GetNotificationTemplatesPagedQueryHandler.ToDto(e);
        }
    }

    public class CreateNotificationTemplateCommand : NotificationTemplateCommandFields, IRequest<Guid> { }

    public class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateNotificationTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateNotificationTemplateCommand request, CancellationToken cancellationToken)
        {
            Validate(request);
            string code = request.Code!.Trim().ToUpperInvariant();
            if (await _context.NotificationTemplateEntities.AnyAsync(x => !x.IsDeleted && x.Code == code, cancellationToken))
            {
                throw new InvalidOperationException("Mã mẫu thông báo đã tồn tại.");
            }

            NotificationTemplateEntity entity = new()
            {
                Code = code,
                Channel = NormalizeChannel(request.Channel),
                Subject = string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim(),
                Body = request.Body!.Trim(),
                IsActive = request.IsActive ?? true,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.NotificationTemplateEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal static void Validate(NotificationTemplateCommandFields request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new InvalidOperationException("Mã mẫu thông báo là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                throw new InvalidOperationException("Nội dung mẫu thông báo là bắt buộc.");
            }
        }

        internal static string NormalizeChannel(string? channel)
        {
            string c = (channel ?? NotificationChannel.Email).Trim().ToUpperInvariant();
            return c is NotificationChannel.Email or NotificationChannel.Sms
                ? c
                : throw new InvalidOperationException("Channel phải là EMAIL hoặc SMS.");
        }
    }

    public class UpdateNotificationTemplateCommand : NotificationTemplateCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateNotificationTemplateCommandHandler : IRequestHandler<UpdateNotificationTemplateCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateNotificationTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateNotificationTemplateCommand request, CancellationToken cancellationToken)
        {
            NotificationTemplateEntity? entity = await _context.NotificationTemplateEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            CreateNotificationTemplateCommandHandler.Validate(request);
            string code = request.Code!.Trim().ToUpperInvariant();
            if (await _context.NotificationTemplateEntities.AnyAsync(x => !x.IsDeleted && x.Code == code && x.Id != request.Id, cancellationToken))
            {
                throw new InvalidOperationException("Mã mẫu thông báo đã tồn tại.");
            }

            entity.Code = code;
            entity.Channel = CreateNotificationTemplateCommandHandler.NormalizeChannel(request.Channel);
            entity.Subject = string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim();
            entity.Body = request.Body!.Trim();
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteNotificationTemplateCommand : SettingsIdRequest, IRequest<bool> { }

    public class DeleteNotificationTemplateCommandHandler : IRequestHandler<DeleteNotificationTemplateCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteNotificationTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteNotificationTemplateCommand request, CancellationToken cancellationToken)
        {
            NotificationTemplateEntity? entity = await _context.NotificationTemplateEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

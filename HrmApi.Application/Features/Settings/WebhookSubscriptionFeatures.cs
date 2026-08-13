using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Entities.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Settings
{
    public class GetWebhookSubscriptionsPagedQuery : SettingsPagedQuery, IRequest<PagedResult<WebhookSubscriptionDto>> { }

    public class GetWebhookSubscriptionsPagedQueryHandler : IRequestHandler<GetWebhookSubscriptionsPagedQuery, PagedResult<WebhookSubscriptionDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetWebhookSubscriptionsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<WebhookSubscriptionDto>> Handle(GetWebhookSubscriptionsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<WebhookSubscriptionEntity> query = _context.WebhookSubscriptionEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s) || x.Url.ToLower().Contains(s) || x.EventTypes.ToLower().Contains(s));
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<WebhookSubscriptionEntity> rows = await query.OrderBy(x => x.Name)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<WebhookSubscriptionDto>(rows.Select(ToDto).ToList(), total, pageIndex, pageSize);
        }

        internal static WebhookSubscriptionDto ToDto(WebhookSubscriptionEntity e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            Url = e.Url,
            EventTypes = e.EventTypes,
            Secret = e.Secret,
            IsActive = e.IsActive,
            Note = e.Note,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
        };
    }

    public class GetWebhookSubscriptionByIdQuery : SettingsIdRequest, IRequest<WebhookSubscriptionDto?> { }

    public class GetWebhookSubscriptionByIdQueryHandler : IRequestHandler<GetWebhookSubscriptionByIdQuery, WebhookSubscriptionDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetWebhookSubscriptionByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<WebhookSubscriptionDto?> Handle(GetWebhookSubscriptionByIdQuery request, CancellationToken cancellationToken)
        {
            WebhookSubscriptionEntity? e = await _context.WebhookSubscriptionEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : GetWebhookSubscriptionsPagedQueryHandler.ToDto(e);
        }
    }

    public class CreateWebhookSubscriptionCommand : WebhookSubscriptionCommandFields, IRequest<Guid> { }

    public class CreateWebhookSubscriptionCommandHandler : IRequestHandler<CreateWebhookSubscriptionCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateWebhookSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateWebhookSubscriptionCommand request, CancellationToken cancellationToken)
        {
            Validate(request);
            WebhookSubscriptionEntity entity = new()
            {
                Name = request.Name!.Trim(),
                Url = request.Url!.Trim(),
                EventTypes = request.EventTypes!.Trim(),
                Secret = string.IsNullOrWhiteSpace(request.Secret) ? null : request.Secret.Trim(),
                IsActive = request.IsActive ?? true,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.WebhookSubscriptionEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal static void Validate(WebhookSubscriptionCommandFields request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Tên webhook là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.Url)) throw new InvalidOperationException("URL webhook là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.EventTypes)) throw new InvalidOperationException("EventTypes là bắt buộc.");
            if (!Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out Uri? uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new InvalidOperationException("URL webhook không hợp lệ.");
        }
    }

    public class UpdateWebhookSubscriptionCommand : WebhookSubscriptionCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateWebhookSubscriptionCommandHandler : IRequestHandler<UpdateWebhookSubscriptionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateWebhookSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateWebhookSubscriptionCommand request, CancellationToken cancellationToken)
        {
            WebhookSubscriptionEntity? entity = await _context.WebhookSubscriptionEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            CreateWebhookSubscriptionCommandHandler.Validate(request);
            entity.Name = request.Name!.Trim();
            entity.Url = request.Url!.Trim();
            entity.EventTypes = request.EventTypes!.Trim();
            entity.Secret = string.IsNullOrWhiteSpace(request.Secret) ? null : request.Secret.Trim();
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteWebhookSubscriptionCommand : SettingsIdRequest, IRequest<bool> { }

    public class DeleteWebhookSubscriptionCommandHandler : IRequestHandler<DeleteWebhookSubscriptionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteWebhookSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteWebhookSubscriptionCommand request, CancellationToken cancellationToken)
        {
            WebhookSubscriptionEntity? entity = await _context.WebhookSubscriptionEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

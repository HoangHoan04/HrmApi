using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Entities.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Integrations
{
    public class GetZaloOaConfigsPagedQuery : SettingsPagedQuery, IRequest<PagedResult<ZaloOaConfigDto>> { }

    public class GetZaloOaConfigsPagedQueryHandler : IRequestHandler<GetZaloOaConfigsPagedQuery, PagedResult<ZaloOaConfigDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetZaloOaConfigsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<ZaloOaConfigDto>> Handle(GetZaloOaConfigsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ZaloOaConfigEntity> query = _context.ZaloOaConfigEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.OaId.ToLower().Contains(s) || x.AppId.ToLower().Contains(s));
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<ZaloOaConfigEntity> rows = await query.OrderByDescending(x => x.IsActive).ThenBy(x => x.OaId)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<ZaloOaConfigDto>(rows.Select(ToDto).ToList(), total, pageIndex, pageSize);
        }

        internal static ZaloOaConfigDto ToDto(ZaloOaConfigEntity e) => new()
        {
            Id = e.Id,
            OaId = e.OaId,
            AppId = e.AppId,
            SecretKey = Mask(e.SecretKey),
            AccessToken = string.IsNullOrEmpty(e.AccessToken) ? null : Mask(e.AccessToken),
            RefreshToken = string.IsNullOrEmpty(e.RefreshToken) ? null : Mask(e.RefreshToken!),
            IsActive = e.IsActive,
            Note = e.Note,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
        };

        private static string Mask(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (key.Length <= 4) return "****";
            return new string('*', Math.Min(8, key.Length - 4)) + key[^4..];
        }
    }

    public class GetZaloOaConfigByIdQuery : SettingsIdRequest, IRequest<ZaloOaConfigDto?> { }

    public class GetZaloOaConfigByIdQueryHandler : IRequestHandler<GetZaloOaConfigByIdQuery, ZaloOaConfigDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetZaloOaConfigByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ZaloOaConfigDto?> Handle(GetZaloOaConfigByIdQuery request, CancellationToken cancellationToken)
        {
            ZaloOaConfigEntity? e = await _context.ZaloOaConfigEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (e == null) return null;
            return new ZaloOaConfigDto
            {
                Id = e.Id,
                OaId = e.OaId,
                AppId = e.AppId,
                SecretKey = e.SecretKey,
                AccessToken = e.AccessToken,
                RefreshToken = e.RefreshToken,
                IsActive = e.IsActive,
                Note = e.Note,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            };
        }
    }

    public class CreateZaloOaConfigCommand : ZaloOaConfigCommandFields, IRequest<Guid> { }

    public class CreateZaloOaConfigCommandHandler : IRequestHandler<CreateZaloOaConfigCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateZaloOaConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateZaloOaConfigCommand request, CancellationToken cancellationToken)
        {
            Validate(request);
            ZaloOaConfigEntity entity = new()
            {
                OaId = request.OaId!.Trim(),
                AppId = request.AppId!.Trim(),
                SecretKey = request.SecretKey!.Trim(),
                AccessToken = string.IsNullOrWhiteSpace(request.AccessToken) ? null : request.AccessToken.Trim(),
                RefreshToken = string.IsNullOrWhiteSpace(request.RefreshToken) ? null : request.RefreshToken.Trim(),
                IsActive = request.IsActive ?? true,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.ZaloOaConfigEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal static void Validate(ZaloOaConfigCommandFields request)
        {
            if (string.IsNullOrWhiteSpace(request.OaId)) throw new InvalidOperationException("OaId là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.AppId)) throw new InvalidOperationException("AppId là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.SecretKey)) throw new InvalidOperationException("SecretKey là bắt buộc.");
        }
    }

    public class UpdateZaloOaConfigCommand : ZaloOaConfigCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateZaloOaConfigCommandHandler : IRequestHandler<UpdateZaloOaConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateZaloOaConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateZaloOaConfigCommand request, CancellationToken cancellationToken)
        {
            ZaloOaConfigEntity? entity = await _context.ZaloOaConfigEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            CreateZaloOaConfigCommandHandler.Validate(request);
            entity.OaId = request.OaId!.Trim();
            entity.AppId = request.AppId!.Trim();
            entity.SecretKey = request.SecretKey!.Trim();
            entity.AccessToken = string.IsNullOrWhiteSpace(request.AccessToken) ? null : request.AccessToken.Trim();
            entity.RefreshToken = string.IsNullOrWhiteSpace(request.RefreshToken) ? null : request.RefreshToken.Trim();
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteZaloOaConfigCommand : SettingsIdRequest, IRequest<bool> { }

    public class DeleteZaloOaConfigCommandHandler : IRequestHandler<DeleteZaloOaConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteZaloOaConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteZaloOaConfigCommand request, CancellationToken cancellationToken)
        {
            ZaloOaConfigEntity? entity = await _context.ZaloOaConfigEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class SendZaloTestCommand : ZaloSendTestRequest, IRequest<string> { }

    public class SendZaloTestCommandHandler : IRequestHandler<SendZaloTestCommand, string>
    {
        private readonly IApplicationDbContext _context;
        public SendZaloTestCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<string> Handle(SendZaloTestCommand request, CancellationToken cancellationToken)
        {
            ZaloOaConfigEntity? config = null;
            if (request.ConfigId.HasValue && request.ConfigId != Guid.Empty)
            {
                config = await _context.ZaloOaConfigEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ConfigId && !x.IsDeleted, cancellationToken);
            }
            else
            {
                config = await _context.ZaloOaConfigEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.IsActive)
                    .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (config == null)
                throw new InvalidOperationException("Chưa cấu hình Zalo OA active.");

            string userId = string.IsNullOrWhiteSpace(request.UserId) ? "zalo-user" : request.UserId.Trim();
            string message = string.IsNullOrWhiteSpace(request.Message) ? "HRM Zalo test" : request.Message.Trim();
            return $"Zalo stub OK → oa={config.OaId}, to={userId}, len={message.Length}";
        }
    }
}

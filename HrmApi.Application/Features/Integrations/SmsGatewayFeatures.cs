using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Entities.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Integrations
{
    public class GetSmsGatewayConfigsPagedQuery : SettingsPagedQuery, IRequest<PagedResult<SmsGatewayConfigDto>> { }

    public class GetSmsGatewayConfigsPagedQueryHandler : IRequestHandler<GetSmsGatewayConfigsPagedQuery, PagedResult<SmsGatewayConfigDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetSmsGatewayConfigsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<SmsGatewayConfigDto>> Handle(GetSmsGatewayConfigsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<SmsGatewayConfigEntity> query = _context.SmsGatewayConfigEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Provider.ToLower().Contains(s) || x.ApiUrl.ToLower().Contains(s)
                    || (x.SenderId != null && x.SenderId.ToLower().Contains(s)));
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<SmsGatewayConfigEntity> rows = await query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Provider)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<SmsGatewayConfigDto>(rows.Select(ToDto).ToList(), total, pageIndex, pageSize);
        }

        internal static SmsGatewayConfigDto ToDto(SmsGatewayConfigEntity e) => new()
        {
            Id = e.Id,
            Provider = e.Provider,
            ApiUrl = e.ApiUrl,
            ApiKey = MaskKey(e.ApiKey),
            SenderId = e.SenderId,
            IsActive = e.IsActive,
            Note = e.Note,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
        };

        private static string MaskKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (key.Length <= 4) return "****";
            return new string('*', Math.Min(8, key.Length - 4)) + key[^4..];
        }
    }

    public class GetSmsGatewayConfigByIdQuery : SettingsIdRequest, IRequest<SmsGatewayConfigDto?> { }

    public class GetSmsGatewayConfigByIdQueryHandler : IRequestHandler<GetSmsGatewayConfigByIdQuery, SmsGatewayConfigDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetSmsGatewayConfigByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<SmsGatewayConfigDto?> Handle(GetSmsGatewayConfigByIdQuery request, CancellationToken cancellationToken)
        {
            SmsGatewayConfigEntity? e = await _context.SmsGatewayConfigEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (e == null) return null;
            return new SmsGatewayConfigDto
            {
                Id = e.Id,
                Provider = e.Provider,
                ApiUrl = e.ApiUrl,
                ApiKey = e.ApiKey,
                SenderId = e.SenderId,
                IsActive = e.IsActive,
                Note = e.Note,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            };
        }
    }

    public class CreateSmsGatewayConfigCommand : SmsGatewayConfigCommandFields, IRequest<Guid> { }

    public class CreateSmsGatewayConfigCommandHandler : IRequestHandler<CreateSmsGatewayConfigCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateSmsGatewayConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateSmsGatewayConfigCommand request, CancellationToken cancellationToken)
        {
            Validate(request);
            SmsGatewayConfigEntity entity = new()
            {
                Provider = request.Provider!.Trim(),
                ApiUrl = request.ApiUrl!.Trim(),
                ApiKey = request.ApiKey!.Trim(),
                SenderId = string.IsNullOrWhiteSpace(request.SenderId) ? null : request.SenderId.Trim(),
                IsActive = request.IsActive ?? true,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.SmsGatewayConfigEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        internal static void Validate(SmsGatewayConfigCommandFields request)
        {
            if (string.IsNullOrWhiteSpace(request.Provider)) throw new InvalidOperationException("Provider là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.ApiUrl)) throw new InvalidOperationException("ApiUrl là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.ApiKey)) throw new InvalidOperationException("ApiKey là bắt buộc.");
            if (!Uri.TryCreate(request.ApiUrl.Trim(), UriKind.Absolute, out _))
                throw new InvalidOperationException("ApiUrl không hợp lệ.");
        }
    }

    public class UpdateSmsGatewayConfigCommand : SmsGatewayConfigCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateSmsGatewayConfigCommandHandler : IRequestHandler<UpdateSmsGatewayConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateSmsGatewayConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateSmsGatewayConfigCommand request, CancellationToken cancellationToken)
        {
            SmsGatewayConfigEntity? entity = await _context.SmsGatewayConfigEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            CreateSmsGatewayConfigCommandHandler.Validate(request);
            entity.Provider = request.Provider!.Trim();
            entity.ApiUrl = request.ApiUrl!.Trim();
            entity.ApiKey = request.ApiKey!.Trim();
            entity.SenderId = string.IsNullOrWhiteSpace(request.SenderId) ? null : request.SenderId.Trim();
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteSmsGatewayConfigCommand : SettingsIdRequest, IRequest<bool> { }

    public class DeleteSmsGatewayConfigCommandHandler : IRequestHandler<DeleteSmsGatewayConfigCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteSmsGatewayConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteSmsGatewayConfigCommand request, CancellationToken cancellationToken)
        {
            SmsGatewayConfigEntity? entity = await _context.SmsGatewayConfigEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class SendSmsTestCommand : SmsSendTestRequest, IRequest<string> { }

    public class SendSmsTestCommandHandler : IRequestHandler<SendSmsTestCommand, string>
    {
        private readonly IApplicationDbContext _context;
        public SendSmsTestCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<string> Handle(SendSmsTestCommand request, CancellationToken cancellationToken)
        {
            SmsGatewayConfigEntity? config = null;
            if (request.ConfigId.HasValue && request.ConfigId != Guid.Empty)
            {
                config = await _context.SmsGatewayConfigEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ConfigId && !x.IsDeleted, cancellationToken);
            }
            else
            {
                config = await _context.SmsGatewayConfigEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.IsActive)
                    .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (config == null)
                throw new InvalidOperationException("Chưa cấu hình SMS gateway active.");

            string phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? "0000000000" : request.PhoneNumber.Trim();
            string message = string.IsNullOrWhiteSpace(request.Message) ? "HRM SMS test" : request.Message.Trim();

            return $"SMS stub OK → provider={config.Provider}, to={phone}, sender={config.SenderId ?? "-"}, len={message.Length}";
        }
    }
}

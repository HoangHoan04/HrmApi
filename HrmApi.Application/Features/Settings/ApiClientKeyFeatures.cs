using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Entities.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HrmApi.Application.Features.Settings
{
    public class GetApiClientKeysPagedQuery : SettingsPagedQuery, IRequest<PagedResult<ApiClientKeyDto>> { }

    public class GetApiClientKeysPagedQueryHandler : IRequestHandler<GetApiClientKeysPagedQuery, PagedResult<ApiClientKeyDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetApiClientKeysPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ApiClientKeyDto>> Handle(GetApiClientKeysPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ApiClientKeyEntity> query = _context.ApiClientKeyEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s) || x.KeyPrefix.ToLower().Contains(s));
            }

            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 20 : request.PageSize;
            int total = await query.CountAsync(cancellationToken);
            List<ApiClientKeyEntity> rows = await query.OrderByDescending(x => x.CreatedAt)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<ApiClientKeyDto>(rows.Select(ToDto).ToList(), total, pageIndex, pageSize);
        }

        internal static ApiClientKeyDto ToDto(ApiClientKeyEntity e)
        {
            return new()
            {
                Id = e.Id,
                Name = e.Name,
                KeyPrefix = e.KeyPrefix,
                CompanyId = e.CompanyId,
                IsActive = e.IsActive,
                ExpiresAt = e.ExpiresAt,
                Note = e.Note,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            };
        }
    }

    public class GetApiClientKeyByIdQuery : SettingsIdRequest, IRequest<ApiClientKeyDto?> { }

    public class GetApiClientKeyByIdQueryHandler : IRequestHandler<GetApiClientKeyByIdQuery, ApiClientKeyDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetApiClientKeyByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiClientKeyDto?> Handle(GetApiClientKeyByIdQuery request, CancellationToken cancellationToken)
        {
            ApiClientKeyEntity? e = await _context.ApiClientKeyEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            return e == null ? null : GetApiClientKeysPagedQueryHandler.ToDto(e);
        }
    }

    public class CreateApiClientKeyCommand : ApiClientKeyCommandFields, IRequest<ApiClientKeyCreateResultDto> { }

    public class CreateApiClientKeyCommandHandler : IRequestHandler<CreateApiClientKeyCommand, ApiClientKeyCreateResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public CreateApiClientKeyCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<ApiClientKeyCreateResultDto> Handle(CreateApiClientKeyCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên API key là bắt buộc.");
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                bool companyOk = await _context.CompanyEntities.AnyAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken);
                if (!companyOk)
                {
                    throw new InvalidOperationException("Công ty không tồn tại.");
                }
            }

            string plaintext = "hrm_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
            string prefix = plaintext[..Math.Min(12, plaintext.Length)];
            string hash = HashKey(plaintext);

            ApiClientKeyEntity entity = new()
            {
                Name = request.Name!.Trim(),
                KeyHash = hash,
                KeyPrefix = prefix,
                CompanyId = request.CompanyId,
                IsActive = request.IsActive ?? true,
                ExpiresAt = request.ExpiresAt,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.ApiClientKeyEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            ApiClientKeyDto dto = GetApiClientKeysPagedQueryHandler.ToDto(entity);
            return new ApiClientKeyCreateResultDto
            {
                Id = dto.Id,
                Name = dto.Name,
                KeyPrefix = dto.KeyPrefix,
                CompanyId = dto.CompanyId,
                IsActive = dto.IsActive,
                ExpiresAt = dto.ExpiresAt,
                Note = dto.Note,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                PlaintextKey = plaintext,
            };
        }

        internal static string HashKey(string plaintext)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }

    public class UpdateApiClientKeyCommand : ApiClientKeyCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateApiClientKeyCommandHandler : IRequestHandler<UpdateApiClientKeyCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateApiClientKeyCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpdateApiClientKeyCommand request, CancellationToken cancellationToken)
        {
            ApiClientKeyEntity? entity = await _context.ApiClientKeyEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Tên API key là bắt buộc.");
            }

            entity.Name = request.Name!.Trim();
            entity.CompanyId = request.CompanyId;
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.ExpiresAt = request.ExpiresAt;
            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class DeleteApiClientKeyCommand : SettingsIdRequest, IRequest<bool> { }

    public class DeleteApiClientKeyCommandHandler : IRequestHandler<DeleteApiClientKeyCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public DeleteApiClientKeyCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteApiClientKeyCommand request, CancellationToken cancellationToken)
        {
            ApiClientKeyEntity? entity = await _context.ApiClientKeyEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

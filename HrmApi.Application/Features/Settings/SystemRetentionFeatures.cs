using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Entities.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Settings
{
    public class GetSystemRetentionConfigQuery : IRequest<SystemRetentionConfigDto> { }

    public class GetSystemRetentionConfigQueryHandler : IRequestHandler<GetSystemRetentionConfigQuery, SystemRetentionConfigDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public GetSystemRetentionConfigQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<SystemRetentionConfigDto> Handle(GetSystemRetentionConfigQuery request, CancellationToken cancellationToken)
        {
            SystemRetentionConfigEntity entity = await EnsureAsync(cancellationToken);
            return ToDto(entity);
        }

        internal async Task<SystemRetentionConfigEntity> EnsureAsync(CancellationToken cancellationToken)
        {
            SystemRetentionConfigEntity? entity = await _context.SystemRetentionConfigEntities
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null) return entity;

            entity = new SystemRetentionConfigEntity
            {
                SoftDeleteRetentionDays = 365,
                IsPurgeEnabled = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? Guid.Empty,
            };
            _ = _context.SystemRetentionConfigEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        internal static SystemRetentionConfigDto ToDto(SystemRetentionConfigEntity e) => new()
        {
            Id = e.Id,
            SoftDeleteRetentionDays = e.SoftDeleteRetentionDays,
            IsPurgeEnabled = e.IsPurgeEnabled,
            Note = e.Note,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
        };
    }

    public class UpdateSystemRetentionConfigCommand : SystemRetentionConfigCommandFields, IRequest<SystemRetentionConfigDto> { }

    public class UpdateSystemRetentionConfigCommandHandler : IRequestHandler<UpdateSystemRetentionConfigCommand, SystemRetentionConfigDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpdateSystemRetentionConfigCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<SystemRetentionConfigDto> Handle(UpdateSystemRetentionConfigCommand request, CancellationToken cancellationToken)
        {
            var ensureHandler = new GetSystemRetentionConfigQueryHandler(_context, _currentUser);
            SystemRetentionConfigEntity entity = await ensureHandler.EnsureAsync(cancellationToken);

            if (request.SoftDeleteRetentionDays.HasValue)
            {
                if (request.SoftDeleteRetentionDays < 1)
                    throw new InvalidOperationException("SoftDeleteRetentionDays phải >= 1.");
                entity.SoftDeleteRetentionDays = request.SoftDeleteRetentionDays.Value;
            }

            if (request.IsPurgeEnabled.HasValue)
                entity.IsPurgeEnabled = request.IsPurgeEnabled.Value;

            entity.Note = request.Note;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return GetSystemRetentionConfigQueryHandler.ToDto(entity);
        }
    }
}

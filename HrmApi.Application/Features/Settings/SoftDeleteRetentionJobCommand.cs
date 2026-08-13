using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrmApi.Application.Features.Settings
{
    public class SoftDeleteRetentionJobCommand : IRequest<SoftDeleteRetentionJobResult> { }

    public class SoftDeleteRetentionJobResult
    {
        public int SoftDeleteRetentionDays { get; set; }
        public bool IsPurgeEnabled { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SoftDeleteRetentionJobCommandHandler : IRequestHandler<SoftDeleteRetentionJobCommand, SoftDeleteRetentionJobResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<SoftDeleteRetentionJobCommandHandler> _logger;

        public SoftDeleteRetentionJobCommandHandler(
            IApplicationDbContext context,
            ILogger<SoftDeleteRetentionJobCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SoftDeleteRetentionJobResult> Handle(SoftDeleteRetentionJobCommand request, CancellationToken cancellationToken)
        {
            SystemRetentionConfigEntity? config = await _context.SystemRetentionConfigEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            int days = config?.SoftDeleteRetentionDays ?? 365;
            bool purge = config?.IsPurgeEnabled ?? false;

            string message = purge
                ? $"Purge enabled (retention {days} days) — hard delete stub skipped."
                : $"Purge disabled (retention {days} days) — no-op.";

            _logger.LogInformation("SoftDeleteRetentionJob: {Message}", message);

            return new SoftDeleteRetentionJobResult
            {
                SoftDeleteRetentionDays = days,
                IsPurgeEnabled = purge,
                Message = message,
            };
        }
    }
}

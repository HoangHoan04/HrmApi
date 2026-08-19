using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrmApi.Application.Features.TransferEmployees.Commands
{
    public class ApplyDueTransfersCommand : IRequest<ApplyDueTransfersResult> { }

    public class ApplyDueTransfersResult
    {
        public int AppliedCount { get; set; }
        public int FailedCount { get; set; }
    }

    public class ApplyDueTransfersCommandHandler : IRequestHandler<ApplyDueTransfersCommand, ApplyDueTransfersResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMediator _mediator;
        private readonly ILogger<ApplyDueTransfersCommandHandler> _logger;

        public ApplyDueTransfersCommandHandler(
            IApplicationDbContext context,
            IMediator mediator,
            ILogger<ApplyDueTransfersCommandHandler> logger)
        {
            _context = context;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApplyDueTransfersResult> Handle(ApplyDueTransfersCommand request, CancellationToken cancellationToken)
        {
            DateTime today = DateTime.UtcNow.Date;
            List<Guid> dueIds = await _context.TransferEmployeeEntities
                .AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.Status == TransferStatus.Approved
                    && x.EffectiveDate.Date <= today)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            int applied = 0;
            int failed = 0;

            foreach (Guid id in dueIds)
            {
                try
                {
                    bool ok = await _mediator.Send(new ApplyTransferEmployeeCommand { Id = id, Force = false }, cancellationToken);
                    if (ok)
                    {
                        applied++;
                    }
                    else
                    {
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogWarning(ex, "ApplyDueTransfers failed for transfer {TransferId}", id);
                }
            }

            _logger.LogInformation("ApplyDueTransfers: applied={Applied}, failed={Failed}", applied, failed);
            return new ApplyDueTransfersResult { AppliedCount = applied, FailedCount = failed };
        }
    }
}

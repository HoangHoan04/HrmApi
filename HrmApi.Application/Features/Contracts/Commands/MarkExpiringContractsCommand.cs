using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrmApi.Application.Features.Contracts.Commands
{
    public class MarkExpiringContractsCommand : IRequest<MarkExpiringContractsResult>
    {
        public int WithinDays { get; set; } = 30;
    }

    public class MarkExpiringContractsResult
    {
        public int MarkedCount { get; set; }
        public int ReviewRenewalsCreated { get; set; }
    }

    public class MarkExpiringContractsCommandHandler : IRequestHandler<MarkExpiringContractsCommand, MarkExpiringContractsResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<MarkExpiringContractsCommandHandler> _logger;

        public MarkExpiringContractsCommandHandler(
            IApplicationDbContext context,
            ILogger<MarkExpiringContractsCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MarkExpiringContractsResult> Handle(MarkExpiringContractsCommand request, CancellationToken cancellationToken)
        {
            int withinDays = request.WithinDays > 0 ? request.WithinDays : 30;
            DateTime today = DateTime.UtcNow.Date;
            DateTime maxDate = today.AddDays(withinDays);

            List<ContractEntity> contracts = await _context.ContractEntities
                .Where(x => !x.IsDeleted
                    && x.Status != ContractStatus.Terminated
                    && x.Status != ContractStatus.Liquidated
                    && x.Status != ContractStatus.Expired
                    && x.Status != ContractStatus.Draft
                    && x.EndDate.HasValue
                    && x.EndDate.Value.Date >= today
                    && x.EndDate.Value.Date <= maxDate
                    && (x.Status == ContractStatus.Active || x.Status == ContractStatus.PendingSign || x.Status == ContractStatus.ExpiringSoon))
                .ToListAsync(cancellationToken);

            int marked = 0;
            int renewals = 0;

            foreach (ContractEntity contract in contracts)
            {
                if (contract.Status != ContractStatus.ExpiringSoon)
                {
                    contract.Status = ContractStatus.ExpiringSoon;
                    contract.UpdatedAt = DateTime.UtcNow;
                    marked++;
                }

                bool openReviewExists = await _context.ReviewRenewalEntities.AnyAsync(x =>
                    !x.IsDeleted
                    && x.ContractId == contract.Id
                    && (x.Status == ReviewRenewalStatus.PendingReview
                        || x.Status == ReviewRenewalStatus.PendingApproval
                        || x.Status == ReviewRenewalStatus.Approved),
                    cancellationToken);

                if (!openReviewExists)
                {
                    _ = _context.ReviewRenewalEntities.Add(new ReviewRenewalEntity
                    {
                        ContractId = contract.Id,
                        EmployeeId = contract.EmployeeId,
                        ReviewDate = today,
                        ReviewedBy = "SYSTEM",
                        Status = ReviewRenewalStatus.PendingReview,
                        Note = $"Tự động tạo trước hết hạn {withinDays} ngày",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                    });
                    renewals++;
                }
            }

            if (marked > 0 || renewals > 0)
            {
                _ = await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation(
                "MarkExpiringContracts: marked={Marked}, renewalsCreated={Renewals}, withinDays={Days}",
                marked, renewals, withinDays);

            return new MarkExpiringContractsResult
            {
                MarkedCount = marked,
                ReviewRenewalsCreated = renewals,
            };
        }
    }
}

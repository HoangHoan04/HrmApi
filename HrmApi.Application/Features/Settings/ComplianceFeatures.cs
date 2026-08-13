using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Settings;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Settings
{
    public class GetComplianceSummaryQuery : ComplianceSummaryRequest, IRequest<ComplianceSummaryDto> { }

    public class GetComplianceSummaryQueryHandler : IRequestHandler<GetComplianceSummaryQuery, ComplianceSummaryDto>
    {
        private readonly IApplicationDbContext _context;
        public GetComplianceSummaryQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ComplianceSummaryDto> Handle(GetComplianceSummaryQuery request, CancellationToken cancellationToken)
        {
            int withinDays = request.WithinDays is > 0 ? request.WithinDays.Value : 30;
            DateTime today = DateTime.UtcNow.Date;
            DateTime maxDate = today.AddDays(withinDays);

            IQueryable<Domain.Entities.Contract.ContractEntity> contracts = _context.ContractEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && (x.Status == ContractStatus.Active || x.Status == ContractStatus.ExpiringSoon)
                    && x.EndDate.HasValue
                    && x.EndDate.Value.Date >= today
                    && x.EndDate.Value.Date <= maxDate);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                contracts = contracts.Where(x => x.CompanyId == request.CompanyId);

            int expiringContracts = await contracts.CountAsync(cancellationToken);

            IQueryable<Domain.Entities.Employee.EmployeeFileEntity> files = _context.EmployeeFileEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.IsCurrent
                    && x.ExpiryDate.HasValue
                    && x.ExpiryDate.Value.Date >= today
                    && x.ExpiryDate.Value.Date <= maxDate);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                files = files.Where(x => _context.EmployeeEntities.Any(e =>
                    e.Id == x.EmployeeId && !e.IsDeleted && e.CompanyId == request.CompanyId));
            }

            int expiringFiles = await files.CountAsync(cancellationToken);

            IQueryable<Domain.Entities.EmployeeMovement.TransferEmployeeEntity> transfers = _context.TransferEmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && (x.Status == TransferStatus.Pending || x.Status == TransferStatus.Approved));

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                Guid companyId = request.CompanyId.Value;
                transfers = transfers.Where(x =>
                    _context.EmployeeEntities.Any(e => e.Id == x.EmployeeId && !e.IsDeleted && e.CompanyId == companyId)
                    || _context.TransferEmployeePositionEntities.Any(d =>
                        !d.IsDeleted && d.TransferEmployeeId == x.Id
                        && (d.OldCompanyId == companyId || d.NewCompanyId == companyId)));
            }

            int pendingTransfers = await transfers.CountAsync(cancellationToken);

            return new ComplianceSummaryDto
            {
                ExpiringContractCount = expiringContracts,
                ExpiringFileCount = expiringFiles,
                PendingTransferCount = pendingTransfers,
                WithinDays = withinDays,
            };
        }
    }
}

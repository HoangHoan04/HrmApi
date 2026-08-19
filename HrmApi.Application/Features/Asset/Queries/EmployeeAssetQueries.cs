using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Asset;
using HrmApi.Domain.Entities.Asset;
using HrmApi.Domain.Entities.Employee;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Asset.Queries
{
    public class GetEmployeeAssetsQuery : IRequest<EmployeeAssetSummaryDto?>
    {
        public Guid EmployeeId { get; set; }
    }

    public class GetEmployeeAssetsQueryHandler : IRequestHandler<GetEmployeeAssetsQuery, EmployeeAssetSummaryDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetEmployeeAssetsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeAssetSummaryDto?> Handle(GetEmployeeAssetsQuery request, CancellationToken cancellationToken)
        {
            EmployeeEntity? emp = await _context.EmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken);
            if (emp == null)
            {
                return null;
            }

            List<AssetAssignmentEntity> assignments = await _context.AssetAssignmentEntities.AsNoTracking()
                .Include(x => x.Asset)
                    .ThenInclude(a => a != null ? a.AssetType : null)
                .Include(x => x.Company)
                .Include(x => x.Branch)
                .Where(x => x.EmployeeId == request.EmployeeId && !x.IsDeleted)
                .OrderByDescending(x => x.IssuedAt)
                .ToListAsync(cancellationToken);

            string empName = emp.FullName ?? (emp.LastName + " " + emp.FirstName).Trim();

            List<AssetAssignmentDto> dtos = assignments.Select(a => new AssetAssignmentDto
            {
                Id = a.Id,
                AssetId = a.AssetId,
                AssetCode = a.Asset?.Code,
                AssetName = a.Asset?.Name,
                SerialNumber = a.Asset?.SerialNumber,
                AssetTypeName = a.Asset?.AssetType?.Name,
                EmployeeId = a.EmployeeId,
                EmployeeCode = emp.Code,
                EmployeeName = empName,
                CompanyId = a.CompanyId,
                CompanyName = a.Company?.Name,
                BranchId = a.BranchId,
                BranchName = a.Branch?.Name,
                IssuedAt = a.IssuedAt,
                ReturnedAt = a.ReturnedAt,
                ConditionOnIssue = a.ConditionOnIssue,
                ConditionOnReturn = a.ConditionOnReturn,
                Note = a.Note,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();

            return new EmployeeAssetSummaryDto
            {
                EmployeeId = emp.Id,
                EmployeeCode = emp.Code,
                EmployeeName = empName,
                CurrentHoldingAssets = dtos.Where(x => x.IsHolding).ToList(),
                PastAssetHistories = dtos.Where(x => !x.IsHolding).ToList()
            };
        }
    }

    public class CheckEmployeeAssetClearanceQuery : IRequest<EmployeeAssetClearanceDto>
    {
        public Guid EmployeeId { get; set; }
    }

    public class CheckEmployeeAssetClearanceQueryHandler : IRequestHandler<CheckEmployeeAssetClearanceQuery, EmployeeAssetClearanceDto>
    {
        private readonly IApplicationDbContext _context;

        public CheckEmployeeAssetClearanceQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeAssetClearanceDto> Handle(CheckEmployeeAssetClearanceQuery request, CancellationToken cancellationToken)
        {
            List<AssetAssignmentEntity> activeAssignments = await _context.AssetAssignmentEntities.AsNoTracking()
                .Include(x => x.Asset)
                    .ThenInclude(a => a != null ? a.AssetType : null)
                .Include(x => x.Employee)
                .Where(x => x.EmployeeId == request.EmployeeId && !x.IsDeleted && !x.ReturnedAt.HasValue)
                .ToListAsync(cancellationToken);

            List<AssetAssignmentDto> unreturnedDtos = activeAssignments.Select(a => new AssetAssignmentDto
            {
                Id = a.Id,
                AssetId = a.AssetId,
                AssetCode = a.Asset?.Code,
                AssetName = a.Asset?.Name,
                SerialNumber = a.Asset?.SerialNumber,
                AssetTypeName = a.Asset?.AssetType?.Name,
                EmployeeId = a.EmployeeId,
                EmployeeCode = a.Employee?.Code,
                EmployeeName = a.Employee != null ? (a.Employee.FullName ?? $"{a.Employee.LastName} {a.Employee.FirstName}".Trim()) : null,
                CompanyId = a.CompanyId,
                BranchId = a.BranchId,
                IssuedAt = a.IssuedAt,
                ConditionOnIssue = a.ConditionOnIssue,
                Note = a.Note
            }).ToList();

            return new EmployeeAssetClearanceDto
            {
                EmployeeId = request.EmployeeId,
                HasUnreturnedAssets = unreturnedDtos.Count > 0,
                UnreturnedCount = unreturnedDtos.Count,
                UnreturnedAssets = unreturnedDtos
            };
        }
    }
}

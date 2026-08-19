using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Recruitment;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Recruitment
{
    public class GetHeadcountTreeQuery : HeadcountTreeQuery, IRequest<List<HeadcountNodeDto>> { }

    public class GetHeadcountTreeQueryHandler : IRequestHandler<GetHeadcountTreeQuery, List<HeadcountNodeDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetHeadcountTreeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<HeadcountNodeDto>> Handle(GetHeadcountTreeQuery request, CancellationToken cancellationToken)
        {
            IQueryable<CompanyEntity> companiesQuery = _context.CompanyEntities.AsNoTracking().Where(x => !x.IsDeleted);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                companiesQuery = companiesQuery.Where(x => x.Id == request.CompanyId);
            }

            List<CompanyEntity> companies = await companiesQuery.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            if (companies.Count == 0)
            {
                return [];
            }

            List<Guid> companyIds = companies.Select(x => x.Id).ToList();

            IQueryable<BranchEntity> branchesQuery = _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId.HasValue && companyIds.Contains(x.CompanyId.Value));
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                branchesQuery = branchesQuery.Where(x => x.Id == request.BranchId);
            }

            List<BranchEntity> branches = await branchesQuery.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            List<Guid> branchIds = branches.Select(x => x.Id).ToList();

            List<DepartmentEntity> departments = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId.HasValue && companyIds.Contains(x.CompanyId.Value)
                    && (
                        (x.BranchId.HasValue && branchIds.Contains(x.BranchId.Value))
                        || (!x.BranchId.HasValue && companyIds.Contains(x.CompanyId.Value))
                    ))
                .OrderBy(x => x.Code)
                .ToListAsync(cancellationToken);

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                departments = departments.Where(d => d.BranchId == request.BranchId).ToList();
            }

            List<Guid> departmentIds = departments.Select(x => x.Id).ToList();

            List<PartEntity> parts = await _context.PartEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.DepartmentId.HasValue && departmentIds.Contains(x.DepartmentId.Value))
                .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code)
                .ToListAsync(cancellationToken);
            List<Guid> partIds = parts.Select(x => x.Id).ToList();

            List<PositionEntity> positions = await _context.PositionEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && (
                    (x.PartId.HasValue && partIds.Contains(x.PartId.Value))
                    || (x.DepartmentId.HasValue && departmentIds.Contains(x.DepartmentId.Value) && !x.PartId.HasValue)))
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync(cancellationToken);

            List<Guid> masterIds = positions
                .Where(x => x.PositionMasterId.HasValue)
                .Select(x => x.PositionMasterId!.Value)
                .Distinct()
                .ToList();
            Dictionary<Guid, (string Code, string Name)> masters = masterIds.Count == 0
                ? []
                : await _context.PositionMasterEntities.AsNoTracking()
                    .Where(x => masterIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (x.Code, x.Name), cancellationToken);

            List<Guid> partMasterIds = parts
                .Where(x => x.PartMasterId.HasValue && string.IsNullOrWhiteSpace(x.Name))
                .Select(x => x.PartMasterId!.Value)
                .Distinct()
                .ToList();
            Dictionary<Guid, (string Code, string Name)> partMasters = partMasterIds.Count == 0
                ? []
                : await _context.PartMasterEntities.AsNoTracking()
                    .Where(x => partMasterIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => (x.Code, x.Name), cancellationToken);

            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId.HasValue && companyIds.Contains(x.CompanyId.Value))
                .Select(x => new { x.CompanyId, x.BranchId, x.DepartmentId, x.PartId, x.PositionId, x.Status })
                .ToListAsync(cancellationToken);

            var active = employees.Where(x => RecruitmentMapper.IsActiveEmployeeStatus(x.Status)).ToList();

            Dictionary<Guid, int> companyActual = active
                .Where(x => x.CompanyId.HasValue)
                .GroupBy(x => x.CompanyId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
            Dictionary<Guid, int> branchActual = active
                .Where(x => x.BranchId.HasValue)
                .GroupBy(x => x.BranchId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
            Dictionary<Guid, int> deptActual = active
                .Where(x => x.DepartmentId.HasValue)
                .GroupBy(x => x.DepartmentId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
            Dictionary<Guid, int> partActual = active
                .Where(x => x.PartId.HasValue)
                .GroupBy(x => x.PartId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
            Dictionary<Guid, int> positionActual = active
                .Where(x => x.PositionId.HasValue)
                .GroupBy(x => x.PositionId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            List<HeadcountNodeDto> result = [];

            foreach (CompanyEntity? company in companies)
            {
                List<BranchEntity> companyBranches = branches.Where(b => b.CompanyId == company.Id).ToList();
                bool hasBranches = companyBranches.Count > 0;
                int companyCount = companyActual.GetValueOrDefault(company.Id);

                int? companyPlanned;
                bool companyEditable;
                bool companyAggregated;

                if (hasBranches)
                {
                    companyPlanned = companyBranches.Any(b => b.MaxEmployeeCapacity.HasValue)
                        ? companyBranches.Where(b => b.MaxEmployeeCapacity.HasValue).Sum(b => b.MaxEmployeeCapacity!.Value)
                        : null;
                    companyEditable = false;
                    companyAggregated = true;
                }
                else
                {
                    companyPlanned = company.MaxEmployeeCapacity;
                    companyEditable = true;
                    companyAggregated = false;
                }

                result.Add(Node(
                    HeadcountNodeType.Company,
                    company.Id,
                    null,
                    company.Code,
                    company.Name,
                    companyPlanned,
                    companyCount,
                    depth: 0,
                    isEditable: companyEditable,
                    isAggregated: companyAggregated,
                    childBranchCount: companyBranches.Count));

                if (hasBranches)
                {
                    foreach (BranchEntity? branch in companyBranches)
                    {
                        int branchCount = branchActual.GetValueOrDefault(branch.Id);
                        result.Add(Node(
                            HeadcountNodeType.Branch,
                            branch.Id,
                            company.Id,
                            branch.Code,
                            branch.Name,
                            branch.MaxEmployeeCapacity,
                            branchCount,
                            depth: 1));

                        AppendDepartments(
                            result,
                            departments.Where(d => d.BranchId == branch.Id),
                            parentId: branch.Id,
                            deptDepth: 2,
                            parts,
                            partMasters,
                            positions,
                            masters,
                            deptActual,
                            partActual,
                            positionActual);
                    }
                }

                AppendDepartments(
                    result,
                    departments.Where(d => d.CompanyId == company.Id && !d.BranchId.HasValue),
                    parentId: company.Id,
                    deptDepth: 1,
                    parts,
                    partMasters,
                    positions,
                    masters,
                    deptActual,
                    partActual,
                    positionActual);
            }

            return result;
        }

        private static void AppendDepartments(
            List<HeadcountNodeDto> result,
            IEnumerable<Domain.Entities.Organization.DepartmentEntity> depts,
            Guid parentId,
            int deptDepth,
            List<Domain.Entities.Organization.PartEntity> parts,
            Dictionary<Guid, (string Code, string Name)> partMasters,
            List<Domain.Entities.Organization.PositionEntity> positions,
            Dictionary<Guid, (string Code, string Name)> masters,
            Dictionary<Guid, int> deptActual,
            Dictionary<Guid, int> partActual,
            Dictionary<Guid, int> positionActual)
        {
            foreach (DepartmentEntity dept in depts)
            {
                int deptCount = deptActual.GetValueOrDefault(dept.Id);
                result.Add(Node(
                    HeadcountNodeType.Department,
                    dept.Id,
                    parentId,
                    dept.Code,
                    dept.Name,
                    dept.Limit,
                    deptCount,
                    deptDepth));

                List<PartEntity> deptParts = parts.Where(p => p.DepartmentId == dept.Id).ToList();
                foreach (PartEntity? part in deptParts)
                {
                    string partCode = !string.IsNullOrWhiteSpace(part.Code)
                        ? part.Code!
                        : (part.PartMasterId.HasValue && partMasters.TryGetValue(part.PartMasterId.Value, out (string Code, string Name) pmc)
                            ? pmc.Code
                            : part.Id.ToString("N")[..8]);
                    string partName = !string.IsNullOrWhiteSpace(part.Name)
                        ? part.Name!
                        : (part.PartMasterId.HasValue && partMasters.TryGetValue(part.PartMasterId.Value, out (string Code, string Name) pmn)
                            ? pmn.Name
                            : partCode);
                    int partCount = partActual.GetValueOrDefault(part.Id);
                    result.Add(Node(
                        HeadcountNodeType.Part,
                        part.Id,
                        dept.Id,
                        partCode,
                        partName,
                        part.Limit,
                        partCount,
                        deptDepth + 1));

                    foreach (PositionEntity? pos in positions.Where(p => p.PartId == part.Id))
                    {
                        result.Add(PositionNode(pos, part.Id, masters, positionActual, deptDepth + 2));
                    }
                }

                foreach (PositionEntity? pos in positions.Where(p => p.DepartmentId == dept.Id && !p.PartId.HasValue))
                {
                    result.Add(PositionNode(pos, dept.Id, masters, positionActual, deptDepth + 1));
                }
            }
        }

        private static HeadcountNodeDto PositionNode(
            Domain.Entities.Organization.PositionEntity pos,
            Guid parentId,
            Dictionary<Guid, (string Code, string Name)> masters,
            Dictionary<Guid, int> positionActual,
            int depth)
        {
            string code = pos.PositionMasterId.HasValue && masters.TryGetValue(pos.PositionMasterId.Value, out (string Code, string Name) m)
                ? m.Code : pos.Id.ToString("N")[..8];
            string name = pos.PositionMasterId.HasValue && masters.TryGetValue(pos.PositionMasterId.Value, out (string Code, string Name) mn)
                ? mn.Name : code;
            int count = positionActual.GetValueOrDefault(pos.Id);
            return Node(HeadcountNodeType.Position, pos.Id, parentId, code, name, pos.QuantityStandard, count, depth);
        }

        private static HeadcountNodeDto Node(
            string nodeType,
            Guid id,
            Guid? parentId,
            string code,
            string name,
            int? planned,
            int actual,
            int depth,
            bool isEditable = true,
            bool isAggregated = false,
            int childBranchCount = 0)
        {
            return new()
            {
                NodeType = nodeType,
                Id = id,
                ParentId = parentId,
                Code = code,
                Name = name,
                PlannedLimit = planned,
                ActualCount = actual,
                Vacancy = planned.HasValue ? planned.Value - actual : null,
                Level = depth,
                Depth = depth,
                IsEditable = isEditable,
                IsAggregated = isAggregated,
                ChildBranchCount = childBranchCount,
            };
        }
    }

    public class UpsertHeadcountRowCommand : UpsertHeadcountRowFields, IRequest<bool> { }

    public class UpsertHeadcountRowCommandHandler : IRequestHandler<UpsertHeadcountRowCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpsertHeadcountRowCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UpsertHeadcountRowCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Id là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(request.NodeType))
            {
                throw new InvalidOperationException("NodeType là bắt buộc.");
            }

            if (request.PlannedLimit.HasValue && request.PlannedLimit.Value < 0)
            {
                throw new InvalidOperationException("Định biên phải >= 0.");
            }

            string nodeType = request.NodeType.Trim().ToUpperInvariant();
            DateTime now = DateTime.UtcNow;
            Guid? userId = _currentUser.UserId;

            switch (nodeType)
            {
                case HeadcountNodeType.Company:
                    {
                        CompanyEntity entity = await _context.CompanyEntities
                            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                            ?? throw new InvalidOperationException("Công ty không tồn tại.");

                        bool hasBranches = await _context.BranchEntities.AnyAsync(
                            x => !x.IsDeleted && x.CompanyId == entity.Id,
                            cancellationToken);

                        if (hasBranches)
                        {
                            throw new InvalidOperationException(
                                "Công ty đã có chi nhánh — định biên cấp công ty được tính tổng từ chi nhánh, không chỉnh trực tiếp.");
                        }

                        entity.MaxEmployeeCapacity = request.PlannedLimit;
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = userId;
                        break;
                    }
                case HeadcountNodeType.Branch:
                    {
                        BranchEntity entity = await _context.BranchEntities
                            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                            ?? throw new InvalidOperationException("Chi nhánh không tồn tại.");
                        entity.MaxEmployeeCapacity = request.PlannedLimit;
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = userId;
                        break;
                    }
                case HeadcountNodeType.Department:
                    {
                        DepartmentEntity entity = await _context.DepartmentEntities
                            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                            ?? throw new InvalidOperationException("Phòng ban không tồn tại.");
                        entity.Limit = request.PlannedLimit ?? 0;
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = userId;
                        break;
                    }
                case HeadcountNodeType.Part:
                    {
                        PartEntity entity = await _context.PartEntities
                            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                            ?? throw new InvalidOperationException("Tổ/nhóm không tồn tại.");
                        entity.Limit = request.PlannedLimit;
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = userId;
                        break;
                    }
                case HeadcountNodeType.Position:
                    {
                        PositionEntity entity = await _context.PositionEntities
                            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                            ?? throw new InvalidOperationException("Chức danh không tồn tại.");
                        entity.QuantityStandard = request.PlannedLimit;
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = userId;
                        break;
                    }
                default:
                    throw new InvalidOperationException("NodeType không hợp lệ.");
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

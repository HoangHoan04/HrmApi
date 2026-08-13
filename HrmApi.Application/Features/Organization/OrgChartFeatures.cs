using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Organization
{
    public class GetOrgChartTreeQuery : GetOrgChartTreeRequest, IRequest<OrgChartNodeDto?> { }

    public class GetOrgChartTreeQueryHandler : IRequestHandler<GetOrgChartTreeQuery, OrgChartNodeDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetOrgChartTreeQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<OrgChartNodeDto?> Handle(GetOrgChartTreeQuery request, CancellationToken cancellationToken)
        {
            if (request.CompanyId == Guid.Empty)
                throw new InvalidOperationException("CompanyId là bắt buộc.");

            var company = await _context.CompanyEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken);
            if (company == null) return null;

            var branches = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId == request.CompanyId)
                .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var departments = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId == request.CompanyId)
                .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var parts = request.IncludeParts
                ? await _context.PartEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.CompanyId == request.CompanyId)
                    .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
                    .ToListAsync(cancellationToken)
                : [];

            var managerIds = branches.Select(x => x.ManagerId)
                .Concat(departments.Select(x => x.ManagerId))
                .Concat(parts.Select(x => x.ManagerId))
                .Where(x => x.HasValue && x != Guid.Empty)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var managerNames = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => managerIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.FullName ?? (x.LastName + " " + x.FirstName) })
                .ToDictionaryAsync(x => x.Id, x => x.Name.Trim(), cancellationToken);

            var empCounts = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId == request.CompanyId)
                .GroupBy(x => new { x.BranchId, x.DepartmentId, x.PartId })
                .Select(g => new { g.Key.BranchId, g.Key.DepartmentId, g.Key.PartId, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int CountFor(Guid? branchId, Guid? deptId, Guid? partId) =>
                empCounts.Where(x =>
                        (!partId.HasValue || x.PartId == partId)
                        && (!deptId.HasValue || x.DepartmentId == deptId)
                        && (!branchId.HasValue || x.BranchId == branchId))
                    .Sum(x => x.Count);

            string? Mgr(Guid? id) =>
                id.HasValue && managerNames.TryGetValue(id.Value, out var n) ? n : null;

            var root = new OrgChartNodeDto
            {
                Id = company.Id,
                NodeType = OrgChartNodeTypes.Company,
                Code = company.Code,
                Name = company.Name,
                CompanyId = company.Id,
                DisplayOrder = 0,
                EmployeeCount = empCounts.Sum(x => x.Count),
            };

            if (branches.Count > 0)
            {
                var branchNodes = branches.ToDictionary(
                    b => b.Id,
                    b => new OrgChartNodeDto
                    {
                        Id = b.Id,
                        NodeType = OrgChartNodeTypes.Branch,
                        Code = b.Code,
                        Name = b.Name,
                        ParentId = b.ParentBranchId ?? company.Id,
                        CompanyId = company.Id,
                        BranchId = b.Id,
                        DisplayOrder = b.DisplayOrder,
                        ManagerName = Mgr(b.ManagerId),
                        EmployeeCount = CountFor(b.Id, null, null),
                    });

                foreach (var b in branches)
                {
                    var node = branchNodes[b.Id];
                    if (b.ParentBranchId.HasValue && branchNodes.TryGetValue(b.ParentBranchId.Value, out var parent))
                        parent.Children.Add(node);
                    else
                        root.Children.Add(node);
                }

                AttachDepartments(departments, parts, branchNodes, root, company.Id, Mgr, CountFor, underBranch: true);
            }
            else
            {
                AttachDepartments(departments, parts, null, root, company.Id, Mgr, CountFor, underBranch: false);
            }

            return root;
        }

        private static void AttachDepartments(
            List<Domain.Entities.Organization.DepartmentEntity> departments,
            List<Domain.Entities.Organization.PartEntity> parts,
            Dictionary<Guid, OrgChartNodeDto>? branchNodes,
            OrgChartNodeDto root,
            Guid companyId,
            Func<Guid?, string?> mgr,
            Func<Guid?, Guid?, Guid?, int> countFor,
            bool underBranch)
        {
            var deptNodes = departments.ToDictionary(
                d => d.Id,
                d => new OrgChartNodeDto
                {
                    Id = d.Id,
                    NodeType = OrgChartNodeTypes.Department,
                    Code = d.Code,
                    Name = d.Name,
                    ParentId = d.ParentDepartmentId
                               ?? d.BranchId
                               ?? companyId,
                    CompanyId = companyId,
                    BranchId = d.BranchId,
                    DisplayOrder = d.DisplayOrder,
                    ManagerName = mgr(d.ManagerId),
                    EmployeeCount = countFor(d.BranchId, d.Id, null),
                });

            foreach (var d in departments)
            {
                var node = deptNodes[d.Id];
                if (d.ParentDepartmentId.HasValue && deptNodes.TryGetValue(d.ParentDepartmentId.Value, out var parentDept))
                {
                    parentDept.Children.Add(node);
                    continue;
                }

                if (underBranch && d.BranchId.HasValue && branchNodes != null
                    && branchNodes.TryGetValue(d.BranchId.Value, out var parentBranch))
                {
                    parentBranch.Children.Add(node);
                }
                else
                {
                    root.Children.Add(node);
                }
            }

            foreach (var p in parts)
            {
                if (!p.DepartmentId.HasValue || !deptNodes.TryGetValue(p.DepartmentId.Value, out var parent))
                    continue;

                parent.Children.Add(new OrgChartNodeDto
                {
                    Id = p.Id,
                    NodeType = OrgChartNodeTypes.Part,
                    Code = p.Code ?? string.Empty,
                    Name = p.Name ?? string.Empty,
                    ParentId = p.DepartmentId,
                    CompanyId = companyId,
                    BranchId = p.BranchId,
                    DisplayOrder = p.DisplayOrder,
                    ManagerName = mgr(p.ManagerId),
                    EmployeeCount = countFor(p.BranchId, p.DepartmentId, p.Id),
                });
            }
        }
    }

    public class ReparentOrgChartNodeCommand : ReparentOrgChartNodeRequest, IRequest<bool> { }

    public class ReparentOrgChartNodeCommandHandler : IRequestHandler<ReparentOrgChartNodeCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ReparentOrgChartNodeCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ReparentOrgChartNodeCommand request, CancellationToken cancellationToken)
        {
            var type = (request.NodeType ?? string.Empty).Trim().ToUpperInvariant();
            return type switch
            {
                OrgChartNodeTypes.Branch => await ReparentBranchAsync(request, cancellationToken),
                OrgChartNodeTypes.Department => await ReparentDepartmentAsync(request, cancellationToken),
                OrgChartNodeTypes.Part => await ReparentPartAsync(request, cancellationToken),
                _ => throw new InvalidOperationException("Loại node không hỗ trợ kéo thả (chỉ BRANCH/DEPARTMENT/PART)."),
            };
        }

        private async Task<bool> ReparentBranchAsync(ReparentOrgChartNodeCommand request, CancellationToken ct)
        {
            var entity = await _context.BranchEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException("Chi nhánh không tồn tại.");

            Guid? newParentBranchId = null;
            if (request.NewParentId.HasValue && request.NewParentId != Guid.Empty
                && request.NewParentId != entity.CompanyId)
            {
                if (request.NewParentId == entity.Id)
                    throw new InvalidOperationException("Không thể đặt cha là chính nó.");

                var parent = await _context.BranchEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.NewParentId && !x.IsDeleted, ct)
                    ?? throw new InvalidOperationException("Chi nhánh cha không tồn tại.");

                if (parent.CompanyId != entity.CompanyId)
                    throw new InvalidOperationException("Chi nhánh cha phải cùng công ty.");

                var cursor = parent.ParentBranchId;
                while (cursor.HasValue)
                {
                    if (cursor == entity.Id)
                        throw new InvalidOperationException("Không thể tạo vòng lặp phân cấp chi nhánh.");
                    cursor = await _context.BranchEntities.AsNoTracking()
                        .Where(x => x.Id == cursor).Select(x => x.ParentBranchId).FirstOrDefaultAsync(ct);
                }

                newParentBranchId = parent.Id;
            }

            var old = new { entity.ParentBranchId, entity.DisplayOrder };
            entity.ParentBranchId = newParentBranchId;
            if (request.DisplayOrder.HasValue)
                entity.DisplayOrder = request.DisplayOrder.Value;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE, "BranchEntity", entity.Id, old,
                new { entity.ParentBranchId, entity.DisplayOrder },
                "OrgChart: đổi cha chi nhánh " + entity.Code);
            return true;
        }

        private async Task<bool> ReparentDepartmentAsync(ReparentOrgChartNodeCommand request, CancellationToken ct)
        {
            var entity = await _context.DepartmentEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException("Phòng ban không tồn tại.");

            Guid? newParentDeptId = null;
            Guid? newBranchId = entity.BranchId;

            if (request.NewParentId.HasValue && request.NewParentId != Guid.Empty)
            {
                if (request.NewParentId == entity.Id)
                    throw new InvalidOperationException("Không thể đặt cha là chính nó.");

                var parentDept = await _context.DepartmentEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.NewParentId && !x.IsDeleted, ct);
                if (parentDept != null)
                {
                    if (parentDept.CompanyId != entity.CompanyId)
                        throw new InvalidOperationException("Phòng ban cha phải cùng công ty.");

                    var cursor = parentDept.ParentDepartmentId;
                    while (cursor.HasValue)
                    {
                        if (cursor == entity.Id)
                            throw new InvalidOperationException("Không thể tạo vòng lặp phân cấp phòng ban.");
                        cursor = await _context.DepartmentEntities.AsNoTracking()
                            .Where(x => x.Id == cursor).Select(x => x.ParentDepartmentId).FirstOrDefaultAsync(ct);
                    }

                    newParentDeptId = parentDept.Id;
                    newBranchId = parentDept.BranchId;
                }
                else
                {
                    var parentBranch = await _context.BranchEntities.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == request.NewParentId && !x.IsDeleted, ct);
                    if (parentBranch != null)
                    {
                        if (parentBranch.CompanyId != entity.CompanyId)
                            throw new InvalidOperationException("Chi nhánh phải cùng công ty.");
                        newBranchId = parentBranch.Id;
                        newParentDeptId = null;
                    }
                    else if (request.NewParentId == entity.CompanyId)
                    {
                        newBranchId = null;
                        newParentDeptId = null;
                    }
                    else
                    {
                        throw new InvalidOperationException("Parent không hợp lệ.");
                    }
                }
            }

            if (request.NewBranchId.HasValue)
                newBranchId = request.NewBranchId == Guid.Empty ? null : request.NewBranchId;

            var old = new { entity.ParentDepartmentId, entity.BranchId, entity.Level, entity.DisplayOrder };
            entity.ParentDepartmentId = newParentDeptId;
            entity.BranchId = newBranchId;
            entity.Level = newParentDeptId.HasValue
                ? (await _context.DepartmentEntities.AsNoTracking()
                       .Where(x => x.Id == newParentDeptId)
                       .Select(x => x.Level).FirstOrDefaultAsync(ct)) + 1
                : 1;
            if (request.DisplayOrder.HasValue)
                entity.DisplayOrder = request.DisplayOrder.Value;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE, "DepartmentEntity", entity.Id, old,
                new { entity.ParentDepartmentId, entity.BranchId, entity.Level, entity.DisplayOrder },
                "OrgChart: đổi cha phòng ban " + entity.Code);
            return true;
        }

        private async Task<bool> ReparentPartAsync(ReparentOrgChartNodeCommand request, CancellationToken ct)
        {
            var entity = await _context.PartEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException("Bộ phận không tồn tại.");

            if (!request.NewParentId.HasValue || request.NewParentId == Guid.Empty)
                throw new InvalidOperationException("Bộ phận phải thuộc một phòng ban.");

            var dept = await _context.DepartmentEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.NewParentId && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException("Phòng ban đích không tồn tại.");

            if (dept.CompanyId != entity.CompanyId)
                throw new InvalidOperationException("Phòng ban đích phải cùng công ty.");

            var old = new { entity.DepartmentId, entity.BranchId, entity.DisplayOrder };
            entity.DepartmentId = dept.Id;
            entity.BranchId = dept.BranchId;
            if (request.DisplayOrder.HasValue)
                entity.DisplayOrder = request.DisplayOrder.Value;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE, "PartEntity", entity.Id, old,
                new { entity.DepartmentId, entity.BranchId, entity.DisplayOrder },
                "OrgChart: chuyển bộ phận " + entity.Code);
            return true;
        }
    }
}

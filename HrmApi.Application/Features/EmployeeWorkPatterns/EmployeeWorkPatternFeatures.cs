using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.EmployeeWorkPatterns
{
    public class EmployeeWorkPatternDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid ShiftMasterId { get; set; }
        public string? ShiftMasterCode { get; set; }
        public string? ShiftMasterName { get; set; }
        public TimeSpan? ShiftStartTime { get; set; }
        public TimeSpan? ShiftEndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        public WorkPatternType PatternType { get; set; }
        public bool WorkOnMonday { get; set; }
        public bool WorkOnTuesday { get; set; }
        public bool WorkOnWednesday { get; set; }
        public bool WorkOnThursday { get; set; }
        public bool WorkOnFriday { get; set; }
        public bool WorkOnSaturday { get; set; }
        public bool WorkOnSunday { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
        public string WorkDaysLabel { get; set; } = string.Empty;
    }

    public class GetEmployeeWorkPatternsPagedQuery : PagedRequest, IRequest<PagedResult<EmployeeWorkPatternDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetEmployeeWorkPatternsPagedQueryHandler
        : IRequestHandler<GetEmployeeWorkPatternsPagedQuery, PagedResult<EmployeeWorkPatternDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetEmployeeWorkPatternsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<EmployeeWorkPatternDto>> Handle(
            GetEmployeeWorkPatternsPagedQuery request,
            CancellationToken cancellationToken)
        {
            IQueryable<EmployeeWorkPatternEntity> query = _context.EmployeeWorkPatternEntities.AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (request.ShiftMasterId.HasValue) query = query.Where(x => x.ShiftMasterId == request.ShiftMasterId);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);

            int total = await query.CountAsync(cancellationToken);
            List<EmployeeWorkPatternEntity> rows = await query
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.EffectiveFrom)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<EmployeeWorkPatternDto>(
                await MapAsync(rows, cancellationToken),
                total,
                request.PageIndex,
                request.PageSize);
        }

        private async Task<List<EmployeeWorkPatternDto>> MapAsync(
            List<EmployeeWorkPatternEntity> rows,
            CancellationToken cancellationToken)
        {
            List<Guid> empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> shiftIds = rows.Select(x => x.ShiftMasterId).Distinct().ToList();
            List<Guid> branchIds = rows.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();

            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => new { x.Code, Name = x.FullName ?? $"{x.LastName} {x.FirstName}".Trim() },
                    cancellationToken);

            var shifts = await _context.ShiftMasterEntities.AsNoTracking()
                .Where(x => shiftIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x, cancellationToken);

            var branches = branchIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

            return rows.Select(row =>
            {
                employees.TryGetValue(row.EmployeeId, out var emp);
                shifts.TryGetValue(row.ShiftMasterId, out ShiftMasterEntity? shift);
                string? branchName = row.BranchId.HasValue && branches.TryGetValue(row.BranchId.Value, out string? bn)
                    ? bn
                    : null;

                return new EmployeeWorkPatternDto
                {
                    Id = row.Id,
                    EmployeeId = row.EmployeeId,
                    EmployeeCode = emp?.Code,
                    EmployeeName = emp?.Name,
                    ShiftMasterId = row.ShiftMasterId,
                    ShiftMasterCode = shift?.Code,
                    ShiftMasterName = shift?.Name,
                    ShiftStartTime = shift?.StartTime,
                    ShiftEndTime = shift?.EndTime,
                    BreakStartTime = shift?.BreakStartTime,
                    BreakEndTime = shift?.BreakEndTime,
                    PatternType = row.PatternType,
                    WorkOnMonday = row.WorkOnMonday,
                    WorkOnTuesday = row.WorkOnTuesday,
                    WorkOnWednesday = row.WorkOnWednesday,
                    WorkOnThursday = row.WorkOnThursday,
                    WorkOnFriday = row.WorkOnFriday,
                    WorkOnSaturday = row.WorkOnSaturday,
                    WorkOnSunday = row.WorkOnSunday,
                    EffectiveFrom = row.EffectiveFrom,
                    EffectiveTo = row.EffectiveTo,
                    BranchId = row.BranchId,
                    BranchName = branchName,
                    Note = row.Note,
                    IsActive = row.IsActive,
                    WorkDaysLabel = BuildWorkDaysLabel(row),
                };
            }).ToList();
        }

        private static string BuildWorkDaysLabel(EmployeeWorkPatternEntity row)
        {
            List<string> days = [];
            if (row.WorkOnMonday) days.Add("T2");
            if (row.WorkOnTuesday) days.Add("T3");
            if (row.WorkOnWednesday) days.Add("T4");
            if (row.WorkOnThursday) days.Add("T5");
            if (row.WorkOnFriday) days.Add("T6");
            if (row.WorkOnSaturday) days.Add("T7");
            if (row.WorkOnSunday) days.Add("CN");
            return days.Count == 0 ? "—" : string.Join(", ", days);
        }
    }

    public class UpsertEmployeeWorkPatternCommand : IRequest<Guid>
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid ShiftMasterId { get; set; }
        public WorkPatternType PatternType { get; set; } = WorkPatternType.FIXED_WEEKLY;
        public bool WorkOnMonday { get; set; } = true;
        public bool WorkOnTuesday { get; set; } = true;
        public bool WorkOnWednesday { get; set; } = true;
        public bool WorkOnThursday { get; set; } = true;
        public bool WorkOnFriday { get; set; } = true;
        public bool WorkOnSaturday { get; set; }
        public bool WorkOnSunday { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public Guid? BranchId { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpsertEmployeeWorkPatternCommandHandler : IRequestHandler<UpsertEmployeeWorkPatternCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        public UpsertEmployeeWorkPatternCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<Guid> Handle(UpsertEmployeeWorkPatternCommand request, CancellationToken cancellationToken)
        {
            if (request.EmployeeId == Guid.Empty)
                throw new InvalidOperationException("Chọn nhân viên.");
            if (request.ShiftMasterId == Guid.Empty)
                throw new InvalidOperationException("Chọn ca làm việc (ShiftMaster).");
            if (request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom)
                throw new InvalidOperationException("Ngày kết thúc phải >= ngày bắt đầu.");

            bool anyDay = request.WorkOnMonday || request.WorkOnTuesday || request.WorkOnWednesday
                || request.WorkOnThursday || request.WorkOnFriday || request.WorkOnSaturday || request.WorkOnSunday;
            if (!anyDay)
                throw new InvalidOperationException("Chọn ít nhất 1 ngày làm trong tuần.");

            _ = await _context.EmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");

            _ = await _context.ShiftMasterEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ShiftMasterId && !x.IsDeleted && x.IsActive, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy ca làm việc hoặc ca đã ngưng.");

            List<EmployeeWorkPatternEntity> others = await _context.EmployeeWorkPatternEntities
                .Where(x => x.EmployeeId == request.EmployeeId
                    && !x.IsDeleted
                    && x.IsActive
                    && (!request.Id.HasValue || x.Id != request.Id.Value))
                .ToListAsync(cancellationToken);

            foreach (EmployeeWorkPatternEntity other in others)
            {
                if (WorkPatternHelper.RangesOverlap(
                        other.EffectiveFrom, other.EffectiveTo,
                        request.EffectiveFrom, request.EffectiveTo))
                {
                    throw new InvalidOperationException(
                        "Nhân viên đã có ca mặc định hiệu lực trùng khoảng ngày. Hãy kết thúc pattern cũ trước khi tạo mới.");
                }
            }

            EmployeeWorkPatternEntity entity;
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                entity = await _context.EmployeeWorkPatternEntities
                    .FirstOrDefaultAsync(x => x.Id == request.Id.Value && !x.IsDeleted, cancellationToken)
                    ?? throw new InvalidOperationException("Không tìm thấy ca mặc định.");
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                entity = new EmployeeWorkPatternEntity
                {
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                };
                _ = _context.EmployeeWorkPatternEntities.Add(entity);
            }

            entity.EmployeeId = request.EmployeeId;
            entity.ShiftMasterId = request.ShiftMasterId;
            entity.PatternType = request.PatternType == 0 ? WorkPatternType.FIXED_WEEKLY : request.PatternType;
            entity.WorkOnMonday = request.WorkOnMonday;
            entity.WorkOnTuesday = request.WorkOnTuesday;
            entity.WorkOnWednesday = request.WorkOnWednesday;
            entity.WorkOnThursday = request.WorkOnThursday;
            entity.WorkOnFriday = request.WorkOnFriday;
            entity.WorkOnSaturday = request.WorkOnSaturday;
            entity.WorkOnSunday = request.WorkOnSunday;
            entity.EffectiveFrom = request.EffectiveFrom;
            entity.EffectiveTo = request.EffectiveTo;
            entity.BranchId = request.BranchId;
            entity.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            entity.IsActive = request.IsActive;

            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }

    public class BulkUpsertEmployeeWorkPatternCommand : IRequest<BulkEmployeeWorkPatternResult>
    {
        public List<Guid> EmployeeIds { get; set; } = [];
        public Guid ShiftMasterId { get; set; }
        public WorkPatternType PatternType { get; set; } = WorkPatternType.FIXED_WEEKLY;
        public bool WorkOnMonday { get; set; } = true;
        public bool WorkOnTuesday { get; set; } = true;
        public bool WorkOnWednesday { get; set; } = true;
        public bool WorkOnThursday { get; set; } = true;
        public bool WorkOnFriday { get; set; } = true;
        public bool WorkOnSaturday { get; set; }
        public bool WorkOnSunday { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public Guid? BranchId { get; set; }
        public string? Note { get; set; }
        public bool CloseOverlapping { get; set; } = true;
    }

    public class BulkEmployeeWorkPatternResult
    {
        public int Created { get; set; }
        public int ClosedPrevious { get; set; }
        public int Skipped { get; set; }
    }

    public class BulkUpsertEmployeeWorkPatternCommandHandler
        : IRequestHandler<BulkUpsertEmployeeWorkPatternCommand, BulkEmployeeWorkPatternResult>
    {
        private readonly IApplicationDbContext _context;
        public BulkUpsertEmployeeWorkPatternCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<BulkEmployeeWorkPatternResult> Handle(
            BulkUpsertEmployeeWorkPatternCommand request,
            CancellationToken cancellationToken)
        {
            if (request.EmployeeIds.Count == 0)
                throw new InvalidOperationException("Chọn ít nhất 1 nhân viên.");
            if (request.ShiftMasterId == Guid.Empty)
                throw new InvalidOperationException("Chọn ca làm việc.");
            if (request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom)
                throw new InvalidOperationException("Ngày kết thúc phải >= ngày bắt đầu.");

            _ = await _context.ShiftMasterEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ShiftMasterId && !x.IsDeleted && x.IsActive, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy ca làm việc.");

            BulkEmployeeWorkPatternResult result = new();
            foreach (Guid employeeId in request.EmployeeIds.Distinct())
            {
                bool empExists = await _context.EmployeeEntities.AsNoTracking()
                    .AnyAsync(x => x.Id == employeeId && !x.IsDeleted, cancellationToken);
                if (!empExists)
                {
                    result.Skipped++;
                    continue;
                }

                List<EmployeeWorkPatternEntity> overlapping = await _context.EmployeeWorkPatternEntities
                    .Where(x => x.EmployeeId == employeeId && !x.IsDeleted && x.IsActive)
                    .ToListAsync(cancellationToken);

                overlapping = overlapping
                    .Where(x => WorkPatternHelper.RangesOverlap(
                        x.EffectiveFrom, x.EffectiveTo, request.EffectiveFrom, request.EffectiveTo))
                    .ToList();

                if (overlapping.Count > 0)
                {
                    if (!request.CloseOverlapping)
                    {
                        result.Skipped++;
                        continue;
                    }

                    foreach (EmployeeWorkPatternEntity old in overlapping)
                    {
                        DateOnly closeTo = request.EffectiveFrom.AddDays(-1);
                        if (closeTo < old.EffectiveFrom)
                        {
                            old.IsActive = false;
                            old.EffectiveTo = old.EffectiveFrom;
                        }
                        else
                        {
                            old.EffectiveTo = closeTo;
                        }
                        old.UpdatedAt = DateTime.UtcNow;
                        result.ClosedPrevious++;
                    }
                }

                _ = _context.EmployeeWorkPatternEntities.Add(new EmployeeWorkPatternEntity
                {
                    EmployeeId = employeeId,
                    ShiftMasterId = request.ShiftMasterId,
                    PatternType = request.PatternType == 0 ? WorkPatternType.FIXED_WEEKLY : request.PatternType,
                    WorkOnMonday = request.WorkOnMonday,
                    WorkOnTuesday = request.WorkOnTuesday,
                    WorkOnWednesday = request.WorkOnWednesday,
                    WorkOnThursday = request.WorkOnThursday,
                    WorkOnFriday = request.WorkOnFriday,
                    WorkOnSaturday = request.WorkOnSaturday,
                    WorkOnSunday = request.WorkOnSunday,
                    EffectiveFrom = request.EffectiveFrom,
                    EffectiveTo = request.EffectiveTo,
                    BranchId = request.BranchId,
                    Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                });
                result.Created++;
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return result;
        }
    }

    public class DeactivateEmployeeWorkPatternCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public DateOnly? EffectiveTo { get; set; }
    }

    public class DeactivateEmployeeWorkPatternCommandHandler : IRequestHandler<DeactivateEmployeeWorkPatternCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        public DeactivateEmployeeWorkPatternCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<bool> Handle(DeactivateEmployeeWorkPatternCommand request, CancellationToken cancellationToken)
        {
            EmployeeWorkPatternEntity? entity = await _context.EmployeeWorkPatternEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null) return false;

            entity.IsActive = false;
            entity.EffectiveTo = request.EffectiveTo ?? DateOnly.FromDateTime(DateTime.UtcNow);
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

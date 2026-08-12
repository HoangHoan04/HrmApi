using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using HrmApi.Application.DTOs.RegisterDayOff;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.DayOffAllocations
{
    public class DayOffAllocationDto
    {
        public Guid Id { get; set; }
        public Guid DayOffConfigId { get; set; }
        public string? DayOffConfigName { get; set; }
        public DayOffType DayOffType { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public decimal PendingDays { get; set; }
        public string? Note { get; set; }
    }

    public class GetDayOffAllocationsPagedQuery : PagedRequest, IRequest<PagedResult<DayOffAllocationDto>>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? DayOffConfigId { get; set; }
        public int? Year { get; set; }
    }

    public class GetDayOffAllocationsPagedQueryHandler : IRequestHandler<GetDayOffAllocationsPagedQuery, PagedResult<DayOffAllocationDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetDayOffAllocationsPagedQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PagedResult<DayOffAllocationDto>> Handle(GetDayOffAllocationsPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<DayOffConfigEmployeeEntity> query = _context.DayOffConfigEmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);
            if (request.DayOffConfigId.HasValue) query = query.Where(x => x.DayOffConfigId == request.DayOffConfigId);
            if (request.Year.HasValue) query = query.Where(x => x.Year == request.Year);

            int total = await query.CountAsync(cancellationToken);
            List<DayOffConfigEmployeeEntity> rows = await query
                .OrderByDescending(x => x.Year).ThenBy(x => x.EmployeeId)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            List<DayOffAllocationDto> items = await MapAsync(rows, cancellationToken);
            return new PagedResult<DayOffAllocationDto>(items, total, request.PageIndex, request.PageSize);
        }

        private async Task<List<DayOffAllocationDto>> MapAsync(List<DayOffConfigEmployeeEntity> rows, CancellationToken cancellationToken)
        {
            List<Guid> empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
            List<Guid> cfgIds = rows.Select(x => x.DayOffConfigId).Distinct().ToList();

            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.FullName, x.Code, x.LastName, x.FirstName }, cancellationToken);
            var configs = await _context.DayOffConfigEntities.AsNoTracking()
                .Where(x => cfgIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.Name, x.DayOffType }, cancellationToken);

            List<DayOffAllocationDto> result = [];
            foreach (DayOffConfigEmployeeEntity row in rows)
            {
                employees.TryGetValue(row.EmployeeId, out var emp);
                configs.TryGetValue(row.DayOffConfigId, out var cfg);
                DateOnly ys = new(row.Year, 1, 1);
                DateOnly ye = new(row.Year, 12, 31);
                decimal pending = await _context.RegisterDayOffEntities.AsNoTracking()
                    .Where(x => x.EmployeeId == row.EmployeeId
                        && x.DayOffConfigId == row.DayOffConfigId
                        && !x.IsDeleted
                        && x.Status == DayOffStatus.PENDING
                        && x.FromDate <= ye && x.ToDate >= ys)
                    .SumAsync(x => x.TotalDays, cancellationToken);

                result.Add(new DayOffAllocationDto
                {
                    Id = row.Id,
                    DayOffConfigId = row.DayOffConfigId,
                    DayOffConfigName = cfg?.Name,
                    DayOffType = cfg?.DayOffType ?? DayOffType.ANNUAL,
                    EmployeeId = row.EmployeeId,
                    EmployeeName = emp?.FullName ?? $"{emp?.LastName} {emp?.FirstName}".Trim(),
                    EmployeeCode = emp?.Code,
                    Year = row.Year,
                    AllocatedDays = row.AllocatedDays,
                    UsedDays = row.UsedDays,
                    RemainingDays = Math.Max(0, row.AllocatedDays - row.UsedDays - pending),
                    PendingDays = pending,
                    Note = row.Note,
                });
            }
            return result;
        }
    }

    public class UpsertDayOffAllocationCommand : IRequest<Guid>
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid DayOffConfigId { get; set; }
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public string? Note { get; set; }
    }

    public class UpsertDayOffAllocationCommandHandler : IRequestHandler<UpsertDayOffAllocationCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public UpsertDayOffAllocationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(UpsertDayOffAllocationCommand request, CancellationToken cancellationToken)
        {
            if (request.EmployeeId == Guid.Empty || request.DayOffConfigId == Guid.Empty)
                throw new InvalidOperationException("Nhân viên và loại nghỉ là bắt buộc.");
            if (request.Year < 2000 || request.Year > 2100)
                throw new InvalidOperationException("Năm không hợp lệ.");
            if (request.AllocatedDays < 0)
                throw new InvalidOperationException("Số ngày cấp phải >= 0.");

            bool empOk = await _context.EmployeeEntities.AnyAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken);
            if (!empOk) throw new InvalidOperationException("Nhân viên không tồn tại.");
            bool cfgOk = await _context.DayOffConfigEntities.AnyAsync(x => x.Id == request.DayOffConfigId && !x.IsDeleted, cancellationToken);
            if (!cfgOk) throw new InvalidOperationException("Cấu hình nghỉ không tồn tại.");

            DayOffConfigEmployeeEntity? entity = null;
            if (request.Id.HasValue && request.Id != Guid.Empty)
            {
                entity = await _context.DayOffConfigEmployeeEntities
                    .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            }
            entity ??= await _context.DayOffConfigEmployeeEntities
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == request.EmployeeId
                    && x.DayOffConfigId == request.DayOffConfigId
                    && x.Year == request.Year
                    && !x.IsDeleted, cancellationToken);

            if (entity == null)
            {
                entity = new DayOffConfigEmployeeEntity
                {
                    EmployeeId = request.EmployeeId,
                    DayOffConfigId = request.DayOffConfigId,
                    Year = request.Year,
                    UsedDays = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId ?? Guid.Empty,
                };
                _ = _context.DayOffConfigEmployeeEntities.Add(entity);
            }

            entity.AllocatedDays = request.AllocatedDays;
            entity.RemainingDays = Math.Max(0, entity.AllocatedDays - entity.UsedDays);
            entity.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _currentUser.UserId;

            _ = await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }

    public class PreviewLeaveDaysQuery : PreviewLeaveDaysRequest, IRequest<PreviewLeaveDaysDto> { }

    public class PreviewLeaveDaysQueryHandler : IRequestHandler<PreviewLeaveDaysQuery, PreviewLeaveDaysDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public PreviewLeaveDaysQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PreviewLeaveDaysDto> Handle(PreviewLeaveDaysQuery request, CancellationToken cancellationToken)
        {
            if (request.ToDate < request.FromDate)
                throw new InvalidOperationException("Ngày kết thúc phải >= ngày bắt đầu.");

            Guid? companyId = request.CompanyId;
            if (!companyId.HasValue)
            {
                Guid? employeeId = request.EmployeeId;
                if (!employeeId.HasValue || employeeId == Guid.Empty)
                {
                    try
                    {
                        employeeId = await Features.RegisterDayOffs.Commands.CurrentEmployeeHelper.ResolveAsync(
                            _context, _currentUser, cancellationToken);
                    }
                    catch
                    {
                        employeeId = null;
                    }
                }

                if (employeeId.HasValue)
                {
                    companyId = await _context.EmployeeEntities.AsNoTracking()
                        .Where(x => x.Id == employeeId)
                        .Select(x => x.CompanyId)
                        .FirstOrDefaultAsync(cancellationToken);
                }
            }

            SaturdayPolicy policy = await LeaveDayCalculator.ResolveSaturdayPolicyAsync(_context, companyId, cancellationToken);
            decimal total = await LeaveDayCalculator.CountWorkingDaysAsync(
                _context, request.FromDate, request.ToDate, companyId, request.Session, cancellationToken);

            return new PreviewLeaveDaysDto
            {
                TotalDays = total,
                SaturdayPolicy = policy,
                Session = request.Session,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
            };
        }
    }
}

using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Employee;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Employees.Commands
{
    public class SetEmployeeLifecycleStatusCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ContractType { get; set; }
        public DateTime? ResignationDate { get; set; }
        public string? ResignationReason { get; set; }
        public string? Note { get; set; }
    }

    public class SetEmployeeLifecycleStatusCommandHandler : IRequestHandler<SetEmployeeLifecycleStatusCommand, bool>
    {
        private static readonly HashSet<string> Allowed =
        [
            EmployeeWorkStatus.Working,
            EmployeeWorkStatus.Probation,
            EmployeeWorkStatus.Official,
            EmployeeWorkStatus.OnLeave,
            EmployeeWorkStatus.Suspended,
            EmployeeWorkStatus.Resigned,
            EmployeeWorkStatus.Retired,
        ];

        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public SetEmployeeLifecycleStatusCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(SetEmployeeLifecycleStatusCommand request, CancellationToken cancellationToken)
        {
            var status = (request.Status ?? string.Empty).Trim().ToUpperInvariant();
            if (!Allowed.Contains(status))
            {
                throw new InvalidOperationException("Trạng thái vòng đời không hợp lệ.");
            }

            var employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (employee == null) return false;

            var oldValue = EmployeeMapper.ToLogObject(employee);
            employee.Status = status;

            if (!string.IsNullOrWhiteSpace(request.ContractType))
            {
                employee.ContractType = request.ContractType.Trim();
            }
            else if (status == EmployeeWorkStatus.Probation)
            {
                employee.ContractType = EmployeeContractType.Probation;
            }
            else if (status == EmployeeWorkStatus.Official
                     && string.Equals(employee.ContractType, EmployeeContractType.Probation, StringComparison.OrdinalIgnoreCase))
            {
                employee.ContractType = EmployeeContractType.Indefinite;
            }

            if (status is EmployeeWorkStatus.Resigned or EmployeeWorkStatus.Retired)
            {
                employee.ResignationDate = request.ResignationDate ?? DateTime.UtcNow.Date;
                employee.ResignationReason = string.IsNullOrWhiteSpace(request.ResignationReason)
                    ? employee.ResignationReason
                    : request.ResignationReason.Trim();
            }
            else if (status is EmployeeWorkStatus.Working or EmployeeWorkStatus.Probation or EmployeeWorkStatus.Official)
            {
                employee.ResignationDate = null;
                employee.ResignationReason = null;
            }

            employee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "EmployeeEntity",
                employee.Id,
                oldValue,
                EmployeeMapper.ToLogObject(employee),
                string.IsNullOrWhiteSpace(request.Note)
                    ? $"Chuyển vòng đời nhân viên → {status}"
                    : $"Chuyển vòng đời → {status}: {request.Note.Trim()}");

            return true;
        }
    }

    public class BulkChangeDirectManagerCommand : IRequest<int>
    {
        public List<Guid> EmployeeIds { get; set; } = [];
        public Guid? DirectManagerId { get; set; }
    }

    public class BulkChangeDirectManagerCommandHandler : IRequestHandler<BulkChangeDirectManagerCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public BulkChangeDirectManagerCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<int> Handle(BulkChangeDirectManagerCommand request, CancellationToken cancellationToken)
        {
            var ids = request.EmployeeIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
            {
                throw new InvalidOperationException("Cần chọn ít nhất một nhân viên.");
            }

            Guid? managerId = request.DirectManagerId.HasValue && request.DirectManagerId != Guid.Empty
                ? request.DirectManagerId
                : null;

            if (managerId.HasValue)
            {
                if (ids.Contains(managerId.Value))
                {
                    throw new InvalidOperationException("Không thể gán quản lý trực tiếp là một trong các nhân viên được chọn.");
                }

                bool exists = await _context.EmployeeEntities
                    .AnyAsync(x => x.Id == managerId && !x.IsDeleted, cancellationToken);
                if (!exists)
                {
                    throw new InvalidOperationException("Quản lý trực tiếp không tồn tại.");
                }
            }

            var employees = await _context.EmployeeEntities
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            var changed = 0;
            foreach (var employee in employees)
            {
                if (employee.DirectManagerId == managerId) continue;
                var old = employee.DirectManagerId;
                employee.DirectManagerId = managerId;
                employee.UpdatedAt = DateTime.UtcNow;
                changed++;

                await _actionLog.LogActionAsync(
                    ActionType.UPDATE,
                    "EmployeeEntity",
                    employee.Id,
                    new { DirectManagerId = old },
                    new { DirectManagerId = managerId },
                    managerId.HasValue
                        ? "Đổi quản lý trực tiếp (bulk)"
                        : "Gỡ quản lý trực tiếp (bulk)");
            }

            if (changed > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return changed;
        }
    }

    public class GetEmployeeChangeTimelineQuery : IRequest<List<EmployeeChangeTimelineItemDto>>
    {
        public Guid EmployeeId { get; set; }
        public int Take { get; set; } = 100;
    }

    public class GetEmployeeChangeTimelineQueryHandler
        : IRequestHandler<GetEmployeeChangeTimelineQuery, List<EmployeeChangeTimelineItemDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetEmployeeChangeTimelineQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<EmployeeChangeTimelineItemDto>> Handle(
            GetEmployeeChangeTimelineQuery request,
            CancellationToken cancellationToken)
        {
            if (request.EmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("EmployeeId là bắt buộc.");
            }

            var take = request.Take <= 0 ? 100 : Math.Min(request.Take, 300);
            var items = new List<EmployeeChangeTimelineItemDto>();

            var employee = await _context.EmployeeEntities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
            if (employee == null) return items;

            items.Add(new EmployeeChangeTimelineItemDto
            {
                At = employee.CreatedAt,
                Kind = EmployeeTimelineKind.Profile,
                Title = "Tạo hồ sơ nhân viên",
                Summary = $"{employee.Code} · {employee.FullName}",
                RefId = employee.Id,
                RefType = "EmployeeEntity",
            });

            var salaries = await _context.EmployeeSalaryHistoryEntities.AsNoTracking()
                .Where(x => x.EmployeeId == request.EmployeeId && !x.IsDeleted)
                .OrderByDescending(x => x.EffectiveDate)
                .Take(take)
                .ToListAsync(cancellationToken);
            foreach (var s in salaries)
            {
                items.Add(new EmployeeChangeTimelineItemDto
                {
                    At = s.EffectiveDate,
                    Kind = EmployeeTimelineKind.Salary,
                    Title = "Thay đổi lương",
                    Summary = $"{s.ChangeType}: {s.OldBasicSalary} → {s.NewBasicSalary}"
                              + (string.IsNullOrWhiteSpace(s.Reason) ? "" : $" · {s.Reason}"),
                    RefId = s.Id,
                    RefType = "EmployeeSalaryHistoryEntity",
                });
            }

            var transfers = await _context.TransferEmployeeEntities.AsNoTracking()
                .Where(x => x.EmployeeId == request.EmployeeId && !x.IsDeleted)
                .OrderByDescending(x => x.EffectiveDate)
                .Take(take)
                .ToListAsync(cancellationToken);
            foreach (var t in transfers)
            {
                items.Add(new EmployeeChangeTimelineItemDto
                {
                    At = t.EffectiveDate,
                    Kind = EmployeeTimelineKind.Transfer,
                    Title = $"Điều chuyển {t.Code}",
                    Summary = $"{t.TransferType} · {t.Status}"
                              + (string.IsNullOrWhiteSpace(t.Reason) ? "" : $" · {t.Reason}"),
                    RefId = t.Id,
                    RefType = "TransferEmployeeEntity",
                });
            }

            var files = await _context.EmployeeFileEntities.AsNoTracking()
                .Where(x => x.EmployeeId == request.EmployeeId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync(cancellationToken);
            foreach (var f in files)
            {
                items.Add(new EmployeeChangeTimelineItemDto
                {
                    At = f.CreatedAt,
                    Kind = EmployeeTimelineKind.File,
                    Title = f.IsCurrent
                        ? $"Tài liệu {f.FileCategory} (v{f.VersionNo})"
                        : $"Tài liệu {f.FileCategory} (v{f.VersionNo}, cũ)",
                    Summary = f.FileName
                              + (f.ExpiryDate.HasValue ? $" · HSD {f.ExpiryDate:yyyy-MM-dd}" : ""),
                    RefId = f.Id,
                    RefType = "EmployeeFileEntity",
                });
            }

            var dependentIds = await _context.EmployeeDependentEntities.AsNoTracking()
                .Where(x => x.EmployeeId == request.EmployeeId).Select(x => x.Id).ToListAsync(cancellationToken);
            var educationIds = await _context.EmployeeEducationEntities.AsNoTracking()
                .Where(x => x.EmployeeId == request.EmployeeId).Select(x => x.Id).ToListAsync(cancellationToken);
            var certificateIds = await _context.EmployeeCertificateEntities.AsNoTracking()
                .Where(x => x.EmployeeId == request.EmployeeId).Select(x => x.Id).ToListAsync(cancellationToken);
            var fileIds = files.Select(x => x.Id).ToList();
            var salaryIds = salaries.Select(x => x.Id).ToList();
            var transferIds = transfers.Select(x => x.Id).ToList();
            var childIds = dependentIds
                .Concat(educationIds)
                .Concat(certificateIds)
                .Concat(fileIds)
                .Concat(salaryIds)
                .Concat(transferIds)
                .Distinct()
                .ToList();

            var logs = await _context.ActionLogEntities.AsNoTracking()
                .Where(x =>
                    (x.EntityName == "EmployeeEntity" && x.EntityId == request.EmployeeId)
                    || (x.EntityId.HasValue && childIds.Contains(x.EntityId.Value)))
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync(cancellationToken);

            foreach (var log in logs)
            {
                items.Add(new EmployeeChangeTimelineItemDto
                {
                    At = log.CreatedAt,
                    Kind = EmployeeTimelineKind.Audit,
                    Title = string.IsNullOrWhiteSpace(log.CreatedNote) ? (log.ActionType ?? EmployeeTimelineKind.Audit) : log.CreatedNote!,
                    Summary = $"{log.EntityName} · {log.ActionType}",
                    RefId = log.Id,
                    RefType = "ActionLogEntity",
                });
            }

            return items
                .OrderByDescending(x => x.At)
                .Take(take)
                .ToList();
        }
    }

    public class GetExpiringEmployeeFilesQuery : IRequest<List<EmployeeExpiringFileDto>>
    {
        public int DaysAhead { get; set; } = 30;
        public Guid? CompanyId { get; set; }
        public bool IncludeExpired { get; set; } = true;
    }

    public class GetExpiringEmployeeFilesQueryHandler
        : IRequestHandler<GetExpiringEmployeeFilesQuery, List<EmployeeExpiringFileDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetExpiringEmployeeFilesQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<EmployeeExpiringFileDto>> Handle(
            GetExpiringEmployeeFilesQuery request,
            CancellationToken cancellationToken)
        {
            var days = request.DaysAhead < 0 ? 0 : request.DaysAhead;
            var today = DateTime.UtcNow.Date;
            var until = today.AddDays(days);

            var query =
                from f in _context.EmployeeFileEntities.AsNoTracking()
                join e in _context.EmployeeEntities.AsNoTracking() on f.EmployeeId equals e.Id
                where !f.IsDeleted && !e.IsDeleted && f.IsCurrent && f.ExpiryDate.HasValue
                select new { f, e };

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.e.CompanyId == request.CompanyId);
            }

            if (request.IncludeExpired)
            {
                query = query.Where(x => x.f.ExpiryDate!.Value.Date <= until);
            }
            else
            {
                query = query.Where(x => x.f.ExpiryDate!.Value.Date >= today && x.f.ExpiryDate.Value.Date <= until);
            }

            var rows = await query
                .OrderBy(x => x.f.ExpiryDate)
                .Take(500)
                .ToListAsync(cancellationToken);

            return rows.Select(x =>
            {
                var expiry = x.f.ExpiryDate!.Value.Date;
                return new EmployeeExpiringFileDto
                {
                    Id = x.f.Id,
                    EmployeeId = x.e.Id,
                    EmployeeCode = x.e.Code,
                    EmployeeName = x.e.FullName ?? $"{x.e.LastName} {x.e.FirstName}".Trim(),
                    FileCategory = x.f.FileCategory,
                    FileName = x.f.FileName,
                    ExpiryDate = x.f.ExpiryDate,
                    VersionNo = x.f.VersionNo,
                    IsExpired = expiry < today,
                    DaysUntilExpiry = (expiry - today).Days,
                };
            }).ToList();
        }
    }
}

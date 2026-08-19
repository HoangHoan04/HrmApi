using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Payroll;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Salaries.Commands
{
    public class CreateSalaryCommand : SalaryCommandFields, IRequest<Guid> { }

    public class CreateSalaryCommandHandler : IRequestHandler<CreateSalaryCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CreateSalaryCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<Guid> Handle(CreateSalaryCommand request, CancellationToken cancellationToken)
        {
            if (!request.EmployeeId.HasValue || request.EmployeeId == Guid.Empty)
            {
                throw new InvalidOperationException("Nhân viên là bắt buộc.");
            }
            if (!request.Year.HasValue || request.Year < 2000 || request.Year > 2100)
            {
                throw new InvalidOperationException("Năm kỳ lương không hợp lệ.");
            }
            if (!request.Month.HasValue || request.Month < 1 || request.Month > 12)
            {
                throw new InvalidOperationException("Tháng kỳ lương không hợp lệ.");
            }

            EmployeeEntity employee = await _context.EmployeeEntities
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Nhân viên không tồn tại.");

            bool exists = await _context.SalaryEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.EmployeeId == request.EmployeeId
                && x.Year == request.Year
                && x.Month == request.Month, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Nhân viên đã có phiếu lương cho kỳ này.");
            }

            SalaryConfigEntity? config = null;
            if (request.SalaryConfigId.HasValue && request.SalaryConfigId != Guid.Empty)
            {
                config = await _context.SalaryConfigEntities
                    .FirstOrDefaultAsync(x => x.Id == request.SalaryConfigId && !x.IsDeleted && x.IsActive, cancellationToken)
                    ?? throw new InvalidOperationException("Cấu hình lương không tồn tại hoặc đã bị vô hiệu.");
            }

            string periodCode = $"{request.Year.Value:D4}-{request.Month.Value:D2}";
            DateTime? payDate = request.PayDate;
            if (!payDate.HasValue && config?.DefaultPayDay is > 0 and <= 31)
            {
                int payMonth = request.Month.Value == 12 ? 1 : request.Month.Value + 1;
                int payYear = request.Month.Value == 12 ? request.Year.Value + 1 : request.Year.Value;
                int day = Math.Min(config.DefaultPayDay.Value, DateTime.DaysInMonth(payYear, payMonth));
                payDate = new DateTime(payYear, payMonth, day, 0, 0, 0, DateTimeKind.Utc);
            }

            SalaryEntity entity = new()
            {
                EmployeeId = request.EmployeeId.Value,
                SalaryConfigId = config?.Id,
                Year = request.Year.Value,
                Month = request.Month.Value,
                PeriodCode = periodCode,
                PayDate = payDate,
                Status = string.IsNullOrWhiteSpace(request.Status) ? SalaryStatus.Draft : request.Status.Trim(),
                CompanyId = employee.CompanyId,
                BranchId = employee.BranchId,
                DepartmentId = employee.DepartmentId,
                PositionId = employee.PositionId,
                StandardWorkingDays = request.StandardWorkingDays ?? config?.StandardWorkingDays,
                ActualWorkingDays = request.ActualWorkingDays,
                BasicSalary = request.BasicSalary ?? 0,
                InsuranceSalary = request.InsuranceSalary ?? request.BasicSalary,
                Currency = string.IsNullOrWhiteSpace(request.Currency)
                    ? (config?.Currency ?? "VND")
                    : request.Currency.Trim().ToUpperInvariant(),
                PayslipFileUrl = string.IsNullOrWhiteSpace(request.PayslipFileUrl) ? null : request.PayslipFileUrl.Trim(),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            List<SalaryLineItemEntity> lines = BuildLineItems(request, config, entity);
            entity.LineItems = lines;
            SalaryMapper.RecalculateTotals(entity);

            _ = _context.SalaryEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "SalaryEntity",
                entity.Id,
                null,
                SalaryMapper.ToLogObject(entity),
                $"Tạo phiếu lương {entity.PeriodCode}");

            return entity.Id;
        }

        internal static List<SalaryLineItemEntity> BuildLineItems(
            SalaryCommandFields request,
            SalaryConfigEntity? config,
            SalaryEntity entity)
        {
            List<SalaryLineItemEntity> lines = [];
            if (request.LineItems is { Count: > 0 })
            {
                int order = 0;
                foreach (SalaryLineItemCommandFields item in request.LineItems)
                {
                    if (string.IsNullOrWhiteSpace(item.ItemCode) || string.IsNullOrWhiteSpace(item.ItemType))
                    {
                        continue;
                    }
                    lines.Add(new SalaryLineItemEntity
                    {
                        ItemType = item.ItemType.Trim().ToUpperInvariant(),
                        ItemCode = item.ItemCode.Trim().ToUpperInvariant(),
                        ItemName = string.IsNullOrWhiteSpace(item.ItemName) ? item.ItemCode.Trim() : item.ItemName.Trim(),
                        Amount = Math.Abs(item.Amount ?? 0),
                        DisplayOrder = item.DisplayOrder ?? order++,
                        Note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim(),
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
            }
            else if (request.BasicSalary is > 0)
            {
                lines.Add(new SalaryLineItemEntity
                {
                    ItemType = SalaryItemType.Income,
                    ItemCode = SalaryItemCode.Basic,
                    ItemName = "Lương cơ bản",
                    Amount = request.BasicSalary.Value,
                    DisplayOrder = 1,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            bool autoIns = request.AutoGenerateInsuranceLines != false;
            if (autoIns && config != null)
            {
                decimal insuranceBase = request.InsuranceSalary
                    ?? request.BasicSalary
                    ?? lines.FirstOrDefault(x => x.ItemCode == SalaryItemCode.Basic)?.Amount
                    ?? 0;
                if (insuranceBase > 0)
                {
                    void AddDeduction(string code, string name, decimal rate, int order)
                    {
                        if (lines.Any(x => x.ItemCode == code))
                        {
                            return;
                        }

                        decimal amount = Math.Round(insuranceBase * rate / 100m, 0, MidpointRounding.AwayFromZero);
                        if (amount <= 0)
                        {
                            return;
                        }

                        lines.Add(new SalaryLineItemEntity
                        {
                            ItemType = SalaryItemType.Deduction,
                            ItemCode = code,
                            ItemName = name,
                            Amount = amount,
                            DisplayOrder = order,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                        });
                    }

                    AddDeduction(SalaryItemCode.Bhxh, "BHXH", config.BhxhEmployeeRate, 100);
                    AddDeduction(SalaryItemCode.Bhyt, "BHYT", config.BhytEmployeeRate, 101);
                    AddDeduction(SalaryItemCode.Bhtn, "BHTN", config.BhtnEmployeeRate, 102);
                    entity.InsuranceSalary = insuranceBase;
                }
            }

            return lines;
        }
    }

    public class UpdateSalaryCommand : SalaryCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateSalaryCommandHandler : IRequestHandler<UpdateSalaryCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateSalaryCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateSalaryCommand request, CancellationToken cancellationToken)
        {
            SalaryEntity? entity = await _context.SalaryEntities
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is SalaryStatus.Paid or SalaryStatus.Cancelled)
            {
                throw new InvalidOperationException("Không thể cập nhật phiếu lương đã chi trả hoặc đã hủy.");
            }

            object oldValue = SalaryMapper.ToLogObject(entity);

            if (request.SalaryConfigId.HasValue)
            {
                entity.SalaryConfigId = request.SalaryConfigId == Guid.Empty ? null : request.SalaryConfigId;
            }
            if (request.PayDate.HasValue)
            {
                entity.PayDate = request.PayDate;
            }
            if (request.StandardWorkingDays.HasValue)
            {
                entity.StandardWorkingDays = request.StandardWorkingDays;
            }
            if (request.ActualWorkingDays.HasValue)
            {
                entity.ActualWorkingDays = request.ActualWorkingDays;
            }
            if (request.BasicSalary.HasValue)
            {
                entity.BasicSalary = request.BasicSalary.Value;
            }
            if (request.InsuranceSalary.HasValue)
            {
                entity.InsuranceSalary = request.InsuranceSalary;
            }
            if (!string.IsNullOrWhiteSpace(request.Currency))
            {
                entity.Currency = request.Currency.Trim().ToUpperInvariant();
            }
            if (request.PayslipFileUrl != null)
            {
                entity.PayslipFileUrl = string.IsNullOrWhiteSpace(request.PayslipFileUrl)
                    ? null
                    : request.PayslipFileUrl.Trim();
            }
            if (request.Note != null)
            {
                entity.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            }
            if (!string.IsNullOrWhiteSpace(request.Status)
                && request.Status is not (SalaryStatus.Approved or SalaryStatus.Paid or SalaryStatus.Cancelled))
            {
                entity.Status = request.Status.Trim();
            }

            if (request.LineItems != null)
            {
                foreach (SalaryLineItemEntity old in entity.LineItems.Where(x => !x.IsDeleted))
                {
                    old.IsDeleted = true;
                    old.UpdatedAt = DateTime.UtcNow;
                }

                SalaryConfigEntity? config = null;
                if (entity.SalaryConfigId.HasValue)
                {
                    config = await _context.SalaryConfigEntities.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == entity.SalaryConfigId, cancellationToken);
                }

                List<SalaryLineItemEntity> lines = CreateSalaryCommandHandler.BuildLineItems(request, config, entity);
                foreach (SalaryLineItemEntity line in lines)
                {
                    line.SalaryId = entity.Id;
                    _ = _context.SalaryLineItemEntities.Add(line);
                    entity.LineItems.Add(line);
                }
            }

            SalaryMapper.RecalculateTotals(entity);
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "SalaryEntity",
                entity.Id,
                oldValue,
                SalaryMapper.ToLogObject(entity),
                $"Cập nhật phiếu lương {entity.PeriodCode}");

            return true;
        }
    }

    public class ApproveSalaryCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? ApprovedBy { get; set; }
    }

    public class ApproveSalaryCommandHandler : IRequestHandler<ApproveSalaryCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ApproveSalaryCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(ApproveSalaryCommand request, CancellationToken cancellationToken)
        {
            SalaryEntity? entity = await _context.SalaryEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is not (SalaryStatus.Draft or SalaryStatus.Processing))
            {
                throw new InvalidOperationException("Chỉ duyệt phiếu lương ở trạng thái Nháp hoặc Đang xử lý.");
            }

            object oldValue = SalaryMapper.ToLogObject(entity);
            entity.Status = SalaryStatus.Approved;
            entity.ApprovedDate = DateTime.UtcNow;
            entity.ApprovedBy = string.IsNullOrWhiteSpace(request.ApprovedBy) ? null : request.ApprovedBy.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "SalaryEntity",
                entity.Id,
                oldValue,
                SalaryMapper.ToLogObject(entity),
                $"Duyệt phiếu lương {entity.PeriodCode}");
            return true;
        }
    }

    public class MarkSalaryPaidCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? PayslipFileUrl { get; set; }
    }

    public class MarkSalaryPaidCommandHandler : IRequestHandler<MarkSalaryPaidCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public MarkSalaryPaidCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(MarkSalaryPaidCommand request, CancellationToken cancellationToken)
        {
            SalaryEntity? entity = await _context.SalaryEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is not (SalaryStatus.Approved or SalaryStatus.Processing))
            {
                throw new InvalidOperationException("Chỉ đánh dấu đã chi trả cho phiếu lương đã duyệt hoặc đang xử lý.");
            }

            object oldValue = SalaryMapper.ToLogObject(entity);
            entity.Status = SalaryStatus.Paid;
            entity.PaidDate = request.PaidDate ?? DateTime.UtcNow.Date;
            entity.PayDate ??= entity.PaidDate;
            if (!string.IsNullOrWhiteSpace(request.PayslipFileUrl))
            {
                entity.PayslipFileUrl = request.PayslipFileUrl.Trim();
            }
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "SalaryEntity",
                entity.Id,
                oldValue,
                SalaryMapper.ToLogObject(entity),
                $"Chi trả phiếu lương {entity.PeriodCode}");
            return true;
        }
    }

    public class CancelSalaryCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? Note { get; set; }
    }

    public class CancelSalaryCommandHandler : IRequestHandler<CancelSalaryCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public CancelSalaryCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(CancelSalaryCommand request, CancellationToken cancellationToken)
        {
            SalaryEntity? entity = await _context.SalaryEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status == SalaryStatus.Paid)
            {
                throw new InvalidOperationException("Không thể hủy phiếu lương đã chi trả.");
            }

            object oldValue = SalaryMapper.ToLogObject(entity);
            entity.Status = SalaryStatus.Cancelled;
            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                entity.Note = request.Note.Trim();
            }
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "SalaryEntity",
                entity.Id,
                oldValue,
                SalaryMapper.ToLogObject(entity),
                $"Hủy phiếu lương {entity.PeriodCode}");
            return true;
        }
    }
}

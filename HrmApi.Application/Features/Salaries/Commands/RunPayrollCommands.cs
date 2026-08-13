using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Salary;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Payroll;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Salaries.Commands
{
    public class PayrollRunRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? SalaryConfigId { get; set; }
        public List<Guid>? EmployeeIds { get; set; }
        public bool OverwriteDrafts { get; set; } = true;
        public bool IncludeDefaultAllowances { get; set; } = true;
        public bool ComputePit { get; set; } = true;
        public decimal OvertimeRateMultiplier { get; set; } = 1.5m;
    }

    public class PayrollPreviewItemDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? ExistingSalaryId { get; set; }
        public string? ExistingStatus { get; set; }
        public bool CanCreate { get; set; }
        public string? SkipReason { get; set; }
        public decimal BasicSalaryFull { get; set; }
        public decimal BasicSalaryPaid { get; set; }
        public decimal? StandardWorkingDays { get; set; }
        public decimal? ActualWorkingDays { get; set; }
        public int TotalOtMinutes { get; set; }
        public int TotalNightMinutes { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal NetSalary { get; set; }
        public decimal PitAmount { get; set; }
        public List<SalaryLineItemDto> LineItems { get; set; } = [];
    }

    public class PayrollPreviewResultDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int ReadyCount { get; set; }
        public int SkippedCount { get; set; }
        public decimal TotalGross { get; set; }
        public decimal TotalNet { get; set; }
        public List<PayrollPreviewItemDto> Items { get; set; } = [];
    }

    public class PayrollRunResultDto
    {
        public int CreatedOrUpdated { get; set; }
        public int Skipped { get; set; }
        public List<Guid> SalaryIds { get; set; } = [];
        public List<string> Warnings { get; set; } = [];
    }

    public class PreviewPayrollRunQuery : PayrollRunRequest, IRequest<PayrollPreviewResultDto> { }

    public class PreviewPayrollRunQueryHandler : IRequestHandler<PreviewPayrollRunQuery, PayrollPreviewResultDto>
    {
        private readonly IApplicationDbContext _context;
        public PreviewPayrollRunQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<PayrollPreviewResultDto> Handle(PreviewPayrollRunQuery request, CancellationToken cancellationToken)
        {
            var engine = new PayrollRunEngine(_context);
            return await engine.PreviewAsync(request, cancellationToken);
        }
    }

    public class RunPayrollCommand : PayrollRunRequest, IRequest<PayrollRunResultDto> { }

    public class RunPayrollCommandHandler : IRequestHandler<RunPayrollCommand, PayrollRunResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;

        public RunPayrollCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
        }

        public async Task<PayrollRunResultDto> Handle(RunPayrollCommand request, CancellationToken cancellationToken)
        {
            var engine = new PayrollRunEngine(_context);
            PayrollPreviewResultDto preview = await engine.PreviewAsync(request, cancellationToken);
            var result = new PayrollRunResultDto();

            foreach (PayrollPreviewItemDto item in preview.Items.Where(x => x.CanCreate))
            {
                if (item.ExistingSalaryId.HasValue)
                {
                    if (!request.OverwriteDrafts)
                    {
                        result.Skipped++;
                        result.Warnings.Add($"{item.EmployeeCode}: đã có phiếu, bỏ qua.");
                        continue;
                    }

                    SalaryEntity? existing = await _context.SalaryEntities
                        .Include(x => x.LineItems)
                        .FirstOrDefaultAsync(x => x.Id == item.ExistingSalaryId.Value && !x.IsDeleted, cancellationToken);
                    if (existing == null || existing.Status is not (SalaryStatus.Draft or SalaryStatus.Processing))
                    {
                        result.Skipped++;
                        result.Warnings.Add($"{item.EmployeeCode}: phiếu không phải DRAFT/PROCESSING.");
                        continue;
                    }

                    ApplyPreviewToEntity(existing, item, preview.PeriodCode, request);
                    foreach (var old in existing.LineItems.Where(x => !x.IsDeleted).ToList())
                    {
                        old.IsDeleted = true;
                        old.UpdatedAt = DateTime.UtcNow;
                    }
                    foreach (var line in BuildEntitiesFromDto(item.LineItems))
                        existing.LineItems.Add(line);
                    SalaryMapper.RecalculateTotals(existing);
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = _currentUser.UserId;
                    result.SalaryIds.Add(existing.Id);
                    result.CreatedOrUpdated++;
                }
                else
                {
                    var employee = await _context.EmployeeEntities.AsNoTracking()
                        .FirstAsync(x => x.Id == item.EmployeeId, cancellationToken);
                    var entity = new SalaryEntity
                    {
                        EmployeeId = item.EmployeeId,
                        Year = request.Year,
                        Month = request.Month,
                        PeriodCode = preview.PeriodCode,
                        Status = SalaryStatus.Draft,
                        CompanyId = employee.CompanyId,
                        BranchId = employee.BranchId,
                        DepartmentId = employee.DepartmentId,
                        PositionId = employee.PositionId,
                        Currency = "VND",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = _currentUser.UserId ?? Guid.Empty,
                        IsDeleted = false,
                        LineItems = BuildEntitiesFromDto(item.LineItems),
                    };
                    ApplyPreviewToEntity(entity, item, preview.PeriodCode, request);
                    SalaryMapper.RecalculateTotals(entity);
                    _context.SalaryEntities.Add(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    result.SalaryIds.Add(entity.Id);
                    result.CreatedOrUpdated++;
                }
            }

            result.Skipped += preview.SkippedCount;
            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "SalaryEntity",
                null,
                null,
                new { request.Year, request.Month, result.CreatedOrUpdated, result.Skipped },
                $"Chạy lương kỳ {preview.PeriodCode}");

            return result;
        }

        private static void ApplyPreviewToEntity(
            SalaryEntity entity,
            PayrollPreviewItemDto item,
            string periodCode,
            PayrollRunRequest request)
        {
            entity.PeriodCode = periodCode;
            entity.Year = request.Year;
            entity.Month = request.Month;
            entity.Status = SalaryStatus.Draft;
            entity.StandardWorkingDays = item.StandardWorkingDays;
            entity.ActualWorkingDays = item.ActualWorkingDays;
            entity.BasicSalary = item.BasicSalaryPaid;
            entity.InsuranceSalary = item.BasicSalaryFull;
            entity.Note = string.IsNullOrWhiteSpace(item.SkipReason)
                ? $"Auto từ TimekeepingSummary {periodCode}"
                : item.SkipReason;
            if (request.SalaryConfigId.HasValue)
                entity.SalaryConfigId = request.SalaryConfigId;
        }

        private static List<SalaryLineItemEntity> BuildEntitiesFromDto(List<SalaryLineItemDto> lines)
        {
            return lines.Select((l, i) => new SalaryLineItemEntity
            {
                ItemType = l.ItemType,
                ItemCode = l.ItemCode,
                ItemName = l.ItemName,
                Amount = l.Amount,
                DisplayOrder = l.DisplayOrder > 0 ? l.DisplayOrder : i + 1,
                Note = l.Note,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            }).ToList();
        }
    }

    public class FinalizePayrollPeriodCommand : IRequest<int>
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public bool GeneratePayslipHtml { get; set; } = true;
    }

    public class FinalizePayrollPeriodCommandHandler : IRequestHandler<FinalizePayrollPeriodCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;
        private readonly IWebhookDeliveryService _webhooks;

        public FinalizePayrollPeriodCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser,
            IWebhookDeliveryService webhooks)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
            _webhooks = webhooks;
        }

        public async Task<int> Handle(FinalizePayrollPeriodCommand request, CancellationToken cancellationToken)
        {
            if (request.Year < 2000 || request.Month is < 1 or > 12)
                throw new InvalidOperationException("Năm/tháng không hợp lệ.");

            var query = _context.SalaryEntities
                .Include(x => x.LineItems)
                .Where(x => !x.IsDeleted
                    && x.Year == request.Year
                    && x.Month == request.Month
                    && (x.Status == SalaryStatus.Draft || x.Status == SalaryStatus.Processing));

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                query = query.Where(x => x.BranchId == request.BranchId);

            List<SalaryEntity> salaries = await query.ToListAsync(cancellationToken);
            if (salaries.Count == 0)
                throw new InvalidOperationException("Không có phiếu DRAFT/PROCESSING để chốt.");

            string actor = _currentUser.UserId?.ToString() ?? "system";
            DateTime now = DateTime.UtcNow;
            var empIds = salaries.Select(x => x.EmployeeId).Distinct().ToList();

            foreach (var salary in salaries)
            {
                salary.Status = SalaryStatus.Approved;
                salary.ApprovedDate = now;
                salary.ApprovedBy = actor;
                salary.UpdatedAt = now;
                salary.UpdatedBy = _currentUser.UserId;

                if (request.GeneratePayslipHtml && string.IsNullOrWhiteSpace(salary.PayslipFileUrl))
                {
                    salary.PayslipFileUrl = $"/api/v1/salary/payslip-html?id={salary.Id}";
                }
            }

            var advances = await _context.AdvanceEntities
                .Where(x => !x.IsDeleted
                    && empIds.Contains(x.EmployeeId)
                    && x.DeductYear == request.Year
                    && x.DeductMonth == request.Month
                    && x.Status == SlipStatus.Approved)
                .ToListAsync(cancellationToken);
            foreach (var a in advances)
            {
                a.Status = SlipStatus.Applied;
                a.UpdatedAt = now;
            }

            var deductions = await _context.DeductionSlipEntities
                .Where(x => !x.IsDeleted
                    && empIds.Contains(x.EmployeeId)
                    && x.ApplyYear == request.Year
                    && x.ApplyMonth == request.Month
                    && x.Status == SlipStatus.Approved)
                .ToListAsync(cancellationToken);
            foreach (var d in deductions)
            {
                d.Status = SlipStatus.Applied;
                d.UpdatedAt = now;
            }

            var additions = await _context.CashAdditionSlipEntities
                .Where(x => !x.IsDeleted
                    && empIds.Contains(x.EmployeeId)
                    && x.ApplyYear == request.Year
                    && x.ApplyMonth == request.Month
                    && x.Status == SlipStatus.Approved)
                .ToListAsync(cancellationToken);
            foreach (var c in additions)
            {
                c.Status = SlipStatus.Applied;
                c.UpdatedAt = now;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.APPROVE,
                "SalaryEntity",
                null,
                null,
                new { request.Year, request.Month, Count = salaries.Count },
                $"Chốt kỳ lương {request.Year:D4}-{request.Month:D2}");

            await _webhooks.PublishAsync(
                WebhookEventTypes.PayrollPeriodFinalized,
                new
                {
                    year = request.Year,
                    month = request.Month,
                    companyId = request.CompanyId,
                    branchId = request.BranchId,
                    count = salaries.Count,
                    salaryIds = salaries.Select(x => x.Id).ToList(),
                },
                cancellationToken);

            return salaries.Count;
        }
    }

    internal sealed class PayrollRunEngine
    {
        private readonly IApplicationDbContext _context;

        public PayrollRunEngine(IApplicationDbContext context) => _context = context;

        public async Task<PayrollPreviewResultDto> PreviewAsync(PayrollRunRequest request, CancellationToken ct)
        {
            if (request.Year < 2000 || request.Month is < 1 or > 12)
                throw new InvalidOperationException("Năm/tháng không hợp lệ.");

            string periodCode = $"{request.Year:D4}-{request.Month:D2}";
            DateOnly periodEnd = new(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month));
            DateTime periodEndDt = periodEnd.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            var summariesQuery = _context.TimekeepingSummaryEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Year == request.Year && x.Month == request.Month);
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                summariesQuery = summariesQuery.Where(x => x.CompanyId == request.CompanyId);
            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
                summariesQuery = summariesQuery.Where(x => x.BranchId == request.BranchId);
            if (request.EmployeeIds is { Count: > 0 })
                summariesQuery = summariesQuery.Where(x => request.EmployeeIds.Contains(x.EmployeeId));

            List<TimekeepingSummaryEntity> summaries = await summariesQuery.ToListAsync(ct);
            if (summaries.Count == 0)
                throw new InvalidOperationException("Chưa có TimekeepingSummary cho kỳ này. Hãy chạy Tổng hợp công trước.");

            var empIds = summaries.Select(x => x.EmployeeId).Distinct().ToList();
            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id) && !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, ct);

            var existingSalaries = await _context.SalaryEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Year == request.Year && x.Month == request.Month && empIds.Contains(x.EmployeeId))
                .ToDictionaryAsync(x => x.EmployeeId, ct);

            var salaryHistories = await _context.EmployeeSalaryHistoryEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && empIds.Contains(x.EmployeeId) && x.EffectiveDate <= periodEndDt)
                .OrderByDescending(x => x.EffectiveDate)
                .ToListAsync(ct);
            var historyMap = salaryHistories
                .GroupBy(x => x.EmployeeId)
                .ToDictionary(g => g.Key, g => g.First());

            var contracts = await _context.ContractEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && empIds.Contains(x.EmployeeId))
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(ct);
            var contractMap = contracts
                .GroupBy(x => x.EmployeeId)
                .ToDictionary(g => g.Key, g => g.First());

            var dependentCounts = await _context.EmployeeDependentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && empIds.Contains(x.EmployeeId)
                    && (x.DependentToDate == null || x.DependentToDate >= periodEndDt)
                    && (x.DependentFromDate == null || x.DependentFromDate <= periodEndDt))
                .GroupBy(x => x.EmployeeId)
                .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.EmployeeId, x => x.Count, ct);

            var advances = await _context.AdvanceEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && empIds.Contains(x.EmployeeId)
                    && x.DeductYear == request.Year && x.DeductMonth == request.Month
                    && (x.Status == SlipStatus.Approved || x.Status == SlipStatus.Applied))
                .ToListAsync(ct);

            var deductions = await _context.DeductionSlipEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && empIds.Contains(x.EmployeeId)
                    && x.ApplyYear == request.Year && x.ApplyMonth == request.Month
                    && (x.Status == SlipStatus.Approved || x.Status == SlipStatus.Applied))
                .ToListAsync(ct);

            var additions = await _context.CashAdditionSlipEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && empIds.Contains(x.EmployeeId)
                    && x.ApplyYear == request.Year && x.ApplyMonth == request.Month
                    && (x.Status == SlipStatus.Approved || x.Status == SlipStatus.Applied))
                .ToListAsync(ct);

            SalaryConfigEntity? defaultConfig = null;
            if (request.SalaryConfigId.HasValue)
            {
                defaultConfig = await _context.SalaryConfigEntities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.SalaryConfigId && !x.IsDeleted && x.IsActive, ct)
                    ?? throw new InvalidOperationException("Cấu hình lương không hợp lệ.");
            }

            var branchIds = employees.Values.Where(e => e.BranchId.HasValue).Select(e => e.BranchId!.Value).Distinct().ToList();
            var branches = branchIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _context.BranchEntities.AsNoTracking()
                    .Where(x => branchIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.GroupSalary ?? string.Empty, ct);

            var companyIds = employees.Values.Where(e => e.CompanyId.HasValue).Select(e => e.CompanyId!.Value).Distinct().ToList();
            var configs = await _context.SalaryConfigEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive
                    && (x.CompanyId == null || companyIds.Contains(x.CompanyId.Value)))
                .ToListAsync(ct);

            List<AllowanceEntity> allowances = [];
            if (request.IncludeDefaultAllowances)
            {
                allowances = await _context.AllowanceEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.IsActive
                        && x.DefaultAmount > 0
                        && (x.CompanyId == null || (request.CompanyId.HasValue && x.CompanyId == request.CompanyId)
                            || companyIds.Contains(x.CompanyId.Value)))
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync(ct);
            }

            var result = new PayrollPreviewResultDto
            {
                Year = request.Year,
                Month = request.Month,
                PeriodCode = periodCode,
            };

            foreach (var summary in summaries)
            {
                if (!employees.TryGetValue(summary.EmployeeId, out var emp))
                    continue;

                var item = new PayrollPreviewItemDto
                {
                    EmployeeId = emp.Id,
                    EmployeeCode = emp.Code,
                    EmployeeName = emp.FullName ?? $"{emp.LastName} {emp.FirstName}".Trim(),
                    ActualWorkingDays = summary.WorkingDays,
                    TotalOtMinutes = summary.TotalOtMinutes,
                    TotalNightMinutes = summary.TotalNightMinutes,
                };

                if (existingSalaries.TryGetValue(emp.Id, out var existing))
                {
                    item.ExistingSalaryId = existing.Id;
                    item.ExistingStatus = existing.Status;
                    if (existing.Status is not (SalaryStatus.Draft or SalaryStatus.Processing))
                    {
                        item.CanCreate = false;
                        item.SkipReason = $"Đã có phiếu {existing.Status}";
                        result.Items.Add(item);
                        result.SkippedCount++;
                        continue;
                    }
                }

                decimal basicFull = 0;
                if (historyMap.TryGetValue(emp.Id, out var hist))
                    basicFull = hist.NewBasicSalary;
                else if (contractMap.TryGetValue(emp.Id, out var contract))
                    basicFull = contract.BasicSalary ?? 0;

                if (basicFull <= 0)
                {
                    item.CanCreate = false;
                    item.SkipReason = "Chưa có lương cơ bản (SalaryHistory/Contract)";
                    result.Items.Add(item);
                    result.SkippedCount++;
                    continue;
                }

                item.BasicSalaryFull = basicFull;

                SalaryConfigEntity? config = defaultConfig;
                if (config == null && emp.BranchId.HasValue && branches.TryGetValue(emp.BranchId.Value, out var groupCode)
                    && !string.IsNullOrWhiteSpace(groupCode))
                {
                    config = configs.FirstOrDefault(c =>
                        c.Code.Equals(groupCode, StringComparison.OrdinalIgnoreCase)
                        && (c.CompanyId == null || c.CompanyId == emp.CompanyId));
                }
                config ??= configs.FirstOrDefault(c => c.CompanyId == emp.CompanyId)
                    ?? configs.FirstOrDefault(c => c.CompanyId == null);

                decimal standardDays = config?.StandardWorkingDays > 0 ? config.StandardWorkingDays : 26;
                item.StandardWorkingDays = standardDays;
                decimal actualDays = Math.Min(summary.WorkingDays, (int)standardDays);
                if (actualDays < 0) actualDays = 0;
                item.ActualWorkingDays = actualDays;

                decimal basicPaid = Math.Round(basicFull * (decimal)actualDays / standardDays, 0, MidpointRounding.AwayFromZero);
                item.BasicSalaryPaid = basicPaid;

                var lines = new List<SalaryLineItemEntity>
                {
                    PayrollLineFactory.Income(SalaryItemCode.Basic, "Lương cơ bản (theo công)", basicPaid, 1,
                        $"{actualDays}/{standardDays} ngày"),
                };

                int order = 10;
                foreach (var alw in allowances.Where(a => a.CompanyId == null || a.CompanyId == emp.CompanyId))
                {
                    lines.Add(PayrollLineFactory.Income(
                        alw.Code.ToUpperInvariant(),
                        alw.Name,
                        alw.DefaultAmount ?? 0,
                        order++,
                        "Phụ cấp mặc định"));
                }

                if (historyMap.TryGetValue(emp.Id, out var h2) && h2.Allowance is > 0)
                {
                    lines.Add(PayrollLineFactory.Income("ALLOWANCE", "Phụ cấp (lịch sử lương)", h2.Allowance.Value, order++));
                }

                foreach (var add in additions.Where(x => x.EmployeeId == emp.Id))
                {
                    lines.Add(PayrollLineFactory.Income(
                        SalaryItemCode.Bonus,
                        add.AdditionType ?? "Thưởng/thu nhập thêm",
                        add.Amount,
                        order++,
                        add.Reason));
                }

                if (summary.TotalOtMinutes > 0 && basicFull > 0)
                {
                    decimal hourly = basicFull / (standardDays * 8m);
                    decimal otHours = summary.TotalOtMinutes / 60m;
                    decimal otAmount = Math.Round(otHours * hourly * request.OvertimeRateMultiplier, 0, MidpointRounding.AwayFromZero);
                    if (otAmount > 0)
                    {
                        lines.Add(PayrollLineFactory.Income(
                            SalaryItemCode.Overtime,
                            "Làm thêm giờ",
                            otAmount,
                            50,
                            $"{summary.TotalOtMinutes} phút × {request.OvertimeRateMultiplier}x"));
                    }
                }

                decimal insuranceBase = basicFull;
                decimal bhTotal = 0;
                if (config != null && insuranceBase > 0)
                {
                    void AddBh(string code, string name, decimal rate, int ord)
                    {
                        decimal amt = Math.Round(insuranceBase * rate / 100m, 0, MidpointRounding.AwayFromZero);
                        if (amt <= 0) return;
                        lines.Add(PayrollLineFactory.Deduction(code, name, amt, ord));
                        bhTotal += amt;
                    }
                    AddBh(SalaryItemCode.Bhxh, "BHXH", config.BhxhEmployeeRate, 100);
                    AddBh(SalaryItemCode.Bhyt, "BHYT", config.BhytEmployeeRate, 101);
                    AddBh(SalaryItemCode.Bhtn, "BHTN", config.BhtnEmployeeRate, 102);
                }

                foreach (var adv in advances.Where(x => x.EmployeeId == emp.Id))
                {
                    lines.Add(PayrollLineFactory.Deduction(SalaryItemCode.Advance, "Tạm ứng", adv.Amount, 110, adv.Reason));
                }

                foreach (var ded in deductions.Where(x => x.EmployeeId == emp.Id))
                {
                    lines.Add(PayrollLineFactory.Deduction(
                        SalaryItemCode.OtherDeduction,
                        ded.DeductionType ?? "Khấu trừ",
                        ded.Amount,
                        120,
                        ded.Reason));
                }

                decimal gross = lines.Where(x => x.ItemType == SalaryItemType.Income).Sum(x => x.Amount);
                decimal pit = 0;
                if (request.ComputePit)
                {
                    dependentCounts.TryGetValue(emp.Id, out int deps);
                    decimal taxable = VietnamPitCalculator.ComputeTaxableIncome(gross, bhTotal, deps);
                    pit = VietnamPitCalculator.ComputeMonthlyTax(taxable);
                    if (pit > 0)
                    {
                        lines.Add(PayrollLineFactory.Deduction(SalaryItemCode.Pit, "Thuế TNCN", pit, 130,
                            $"Phụ thuộc: {deps}"));
                    }
                }

                decimal deduction = lines.Where(x => x.ItemType == SalaryItemType.Deduction).Sum(x => x.Amount);
                item.GrossSalary = gross;
                item.TotalDeduction = deduction;
                item.NetSalary = gross - deduction;
                item.PitAmount = pit;
                item.LineItems = lines.Select(SalaryMapper.ToLineDto).ToList();
                item.CanCreate = true;
                result.Items.Add(item);
                result.ReadyCount++;
                result.TotalGross += gross;
                result.TotalNet += item.NetSalary;
            }

            result.TotalEmployees = result.Items.Count;
            return result;
        }
    }
}

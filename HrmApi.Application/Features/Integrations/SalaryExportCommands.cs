using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Payroll;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace HrmApi.Application.Features.Integrations
{
    public class SalaryExportPeriodRequest
    {
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class SalaryExportFileResult
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text/csv; charset=utf-8";
        public byte[] Content { get; set; } = [];
    }

    public class ExportBankPayrollFileCommand : SalaryExportPeriodRequest, IRequest<SalaryExportFileResult> { }

    public class ExportBankPayrollFileCommandHandler : IRequestHandler<ExportBankPayrollFileCommand, SalaryExportFileResult>
    {
        private readonly IApplicationDbContext _context;
        public ExportBankPayrollFileCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SalaryExportFileResult> Handle(ExportBankPayrollFileCommand request, CancellationToken cancellationToken)
        {
            SalaryExportHelpers.ValidatePeriod(request);
            List<SalaryEntity> salaries = await SalaryExportHelpers.LoadFinalizedAsync(_context, request, cancellationToken);
            List<Guid> empIds = salaries.Select(x => x.EmployeeId).Distinct().ToList();
            Dictionary<Guid, EmployeeEntity> employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            StringBuilder sb = new();
            _ = sb.AppendLine("AccountNumber,AccountName,Amount,EmployeeCode,EmployeeName,BankName,Period");
            foreach (SalaryEntity s in salaries.OrderBy(x => employees.GetValueOrDefault(x.EmployeeId)?.Code))
            {
                _ = employees.TryGetValue(s.EmployeeId, out EmployeeEntity? emp);
                string account = emp?.BankAccountNumber ?? string.Empty;
                string name = emp?.BankAccountHolder ?? emp?.FullName ?? string.Empty;
                string code = emp?.Code ?? string.Empty;
                string empName = emp?.FullName ?? string.Empty;
                string bank = emp?.BankName ?? string.Empty;
                _ = sb.Append(SalaryExportHelpers.Csv(account)).Append(',')
                  .Append(SalaryExportHelpers.Csv(name)).Append(',')
                  .Append(s.NetSalary.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                  .Append(SalaryExportHelpers.Csv(code)).Append(',')
                  .Append(SalaryExportHelpers.Csv(empName)).Append(',')
                  .Append(SalaryExportHelpers.Csv(bank)).Append(',')
                  .Append(SalaryExportHelpers.Csv(s.PeriodCode)).AppendLine();
            }

            return SalaryExportHelpers.ToCsvResult($"bank-payroll-{request.PeriodYear:D4}-{request.PeriodMonth:D2}.csv", sb);
        }
    }

    public class ExportBhxhFileCommand : SalaryExportPeriodRequest, IRequest<SalaryExportFileResult> { }

    public class ExportBhxhFileCommandHandler : IRequestHandler<ExportBhxhFileCommand, SalaryExportFileResult>
    {
        private readonly IApplicationDbContext _context;
        public ExportBhxhFileCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SalaryExportFileResult> Handle(ExportBhxhFileCommand request, CancellationToken cancellationToken)
        {
            SalaryExportHelpers.ValidatePeriod(request);
            List<SalaryEntity> salaries = await _context.SalaryEntities.AsNoTracking()
                .Include(x => x.LineItems)
                .Where(x => !x.IsDeleted
                    && x.Year == request.PeriodYear
                    && x.Month == request.PeriodMonth
                    && (x.Status == SalaryStatus.Approved || x.Status == SalaryStatus.Paid)
                    && (!request.CompanyId.HasValue || request.CompanyId == Guid.Empty || x.CompanyId == request.CompanyId))
                .ToListAsync(cancellationToken);

            List<Guid> empIds = salaries.Select(x => x.EmployeeId).Distinct().ToList();
            Dictionary<Guid, EmployeeEntity> employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            StringBuilder sb = new();
            _ = sb.AppendLine("EmployeeCode,EmployeeName,SocialInsuranceNumber,InsuranceSalary,BHXH,BHYT,BHTN,Period");
            foreach (SalaryEntity s in salaries.OrderBy(x => employees.GetValueOrDefault(x.EmployeeId)?.Code))
            {
                _ = employees.TryGetValue(s.EmployeeId, out EmployeeEntity? emp);
                decimal bhxh = SumLine(s, SalaryItemCode.Bhxh);
                decimal bhyt = SumLine(s, SalaryItemCode.Bhyt);
                decimal bhtn = SumLine(s, SalaryItemCode.Bhtn);
                _ = sb.Append(SalaryExportHelpers.Csv(emp?.Code)).Append(',')
                  .Append(SalaryExportHelpers.Csv(emp?.FullName)).Append(',')
                  .Append(SalaryExportHelpers.Csv(emp?.SocialInsuranceNumber)).Append(',')
                  .Append((s.InsuranceSalary ?? 0).ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                  .Append(bhxh.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                  .Append(bhyt.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                  .Append(bhtn.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                  .Append(SalaryExportHelpers.Csv(s.PeriodCode)).AppendLine();
            }

            return SalaryExportHelpers.ToCsvResult($"bhxh-{request.PeriodYear:D4}-{request.PeriodMonth:D2}.csv", sb);
        }

        private static decimal SumLine(SalaryEntity s, string code)
        {
            return (s.LineItems ?? []).Where(x => !x.IsDeleted && x.ItemCode == code).Sum(x => x.Amount);
        }
    }

    public class ExportAccountingFileCommand : SalaryExportPeriodRequest, IRequest<SalaryExportFileResult> { }

    public class ExportAccountingFileCommandHandler : IRequestHandler<ExportAccountingFileCommand, SalaryExportFileResult>
    {
        private readonly IApplicationDbContext _context;
        public ExportAccountingFileCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SalaryExportFileResult> Handle(ExportAccountingFileCommand request, CancellationToken cancellationToken)
        {
            SalaryExportHelpers.ValidatePeriod(request);
            List<SalaryEntity> salaries = await SalaryExportHelpers.LoadFinalizedAsync(_context, request, cancellationToken);

            StringBuilder sb = new();
            _ = sb.AppendLine("JournalDate,Account,Debit,Credit,Description,Period,EmployeeCode,Ref");
            string journalDate = new DateOnly(request.PeriodYear, request.PeriodMonth,
                DateTime.DaysInMonth(request.PeriodYear, request.PeriodMonth)).ToString("yyyy-MM-dd");

            List<Guid> empIds = salaries.Select(x => x.EmployeeId).Distinct().ToList();
            Dictionary<Guid, string> codes = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            foreach (SalaryEntity s in salaries)
            {
                string empCode = codes.GetValueOrDefault(s.EmployeeId) ?? s.EmployeeId.ToString();
                string period = s.PeriodCode;
                AppendJournal(sb, journalDate, "642", s.GrossSalary, 0, $"Chi phí lương {empCode}", period, empCode, s.Id);
                AppendJournal(sb, journalDate, "334", 0, s.GrossSalary, $"Phải trả lương {empCode}", period, empCode, s.Id);
                if (s.TotalDeduction > 0)
                {
                    AppendJournal(sb, journalDate, "334", s.TotalDeduction, 0, $"Khấu trừ lương {empCode}", period, empCode, s.Id);
                    AppendJournal(sb, journalDate, "3383", 0, s.TotalDeduction, $"Các khoản khấu trừ {empCode}", period, empCode, s.Id);
                }
                AppendJournal(sb, journalDate, "334", s.NetSalary, 0, $"Chi trả net {empCode}", period, empCode, s.Id);
                AppendJournal(sb, journalDate, "112", 0, s.NetSalary, $"Tiền gửi ngân hàng net {empCode}", period, empCode, s.Id);
            }

            return SalaryExportHelpers.ToCsvResult($"accounting-{request.PeriodYear:D4}-{request.PeriodMonth:D2}.csv", sb);
        }

        private static void AppendJournal(
            StringBuilder sb, string date, string account, decimal debit, decimal credit,
            string desc, string period, string empCode, Guid refId)
        {
            _ = sb.Append(date).Append(',')
              .Append(account).Append(',')
              .Append(debit.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
              .Append(credit.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
              .Append(SalaryExportHelpers.Csv(desc)).Append(',')
              .Append(SalaryExportHelpers.Csv(period)).Append(',')
              .Append(SalaryExportHelpers.Csv(empCode)).Append(',')
              .Append(refId).AppendLine();
        }
    }

    internal static class SalaryExportHelpers
    {
        public static void ValidatePeriod(SalaryExportPeriodRequest request)
        {
            if (request.PeriodYear < 2000 || request.PeriodMonth is < 1 or > 12)
            {
                throw new InvalidOperationException("Năm/tháng kỳ lương không hợp lệ.");
            }
        }

        public static async Task<List<SalaryEntity>> LoadFinalizedAsync(
            IApplicationDbContext context, SalaryExportPeriodRequest request, CancellationToken ct)
        {
            IQueryable<SalaryEntity> query = context.SalaryEntities.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && x.Year == request.PeriodYear
                    && x.Month == request.PeriodMonth
                    && (x.Status == SalaryStatus.Approved || x.Status == SalaryStatus.Paid));
            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            List<SalaryEntity> rows = await query.ToListAsync(ct);
            return rows.Count == 0 ? throw new InvalidOperationException("Không có phiếu lương đã chốt (APPROVED/PAID) cho kỳ này.") : rows;
        }

        public static string Csv(string? value)
        {
            string v = value ?? string.Empty;
            return v.Contains(',') || v.Contains('"') || v.Contains('\n') ? $"\"{v.Replace("\"", "\"\"")}\"" : v;
        }

        public static SalaryExportFileResult ToCsvResult(string fileName, StringBuilder sb)
        {
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] body = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] bytes = new byte[bom.Length + body.Length];
            Buffer.BlockCopy(bom, 0, bytes, 0, bom.Length);
            Buffer.BlockCopy(body, 0, bytes, bom.Length, body.Length);
            return new SalaryExportFileResult { FileName = fileName, Content = bytes };
        }
    }
}

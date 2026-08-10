using ClosedXML.Excel;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Employees.Commands
{
    public class ExportEmployeesExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Status { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportEmployeesExcelQueryHandler : IRequestHandler<ExportEmployeesExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportEmployeesExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportEmployeesExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<EmployeeEntity> query = _context.EmployeeEntities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                var fullName = request.FullName.Trim().ToLower();
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.ToLower().Contains(fullName))
                    || x.FirstName.ToLower().Contains(fullName)
                    || x.LastName.ToLower().Contains(fullName));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var status = request.Status.Trim().ToLower();
                query = query.Where(x => x.Status != null && x.Status.ToLower() == status);
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            List<EmployeeEntity> employees = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);

            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachNhanVien");
            EmployeeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < employees.Count; i++)
            {
                EmployeeExcelWriter.WriteEmployeeRow(worksheet, i + 2, employees[i], includeExportOnlyColumns: true);
            }

            EmployeeExcelWriter.ApplyColumnWidths(worksheet);
            EmployeeExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadEmployeeExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadEmployeeExcelTemplateQueryHandler : IRequestHandler<DownloadEmployeeExcelTemplateQuery, byte[]>
    {
        public Task<byte[]> Handle(DownloadEmployeeExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("MauImport");
            EmployeeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            EmployeeExcelWriter.ApplyColumnWidths(worksheet);
            EmployeeExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }

    public class ImportEmployeesExcelCommand : IRequest<EmployeeImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class EmployeeImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportEmployeesExcelCommandHandler : IRequestHandler<ImportEmployeesExcelCommand, EmployeeImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportEmployeesExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<EmployeeImportResultDto> Handle(ImportEmployeesExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new EmployeeImportResultDto();

            using var stream = new MemoryStream(request.FileContent);
            using var workbook = new XLWorkbook(stream);
            IXLWorksheet worksheet = workbook.Worksheet(1);
            List<IXLRangeRow> rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? [];

            result.TotalRows = rows.Count;

            foreach (IXLRangeRow? row in rows)
            {
                var rowNumber = row.RowNumber();
                try
                {
                    EmployeeCommandFields command = ReadRow(row);
                    if (string.IsNullOrWhiteSpace(command.Code)
                        && string.IsNullOrWhiteSpace(command.FirstName)
                        && string.IsNullOrWhiteSpace(command.LastName))
                    {
                        continue;
                    }

                    await CreateEmployeeCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var employee = new EmployeeEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    EmployeeMapper.ApplyCommandFields(employee, command);

                    _ = _context.EmployeeEntities.Add(employee);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "EmployeeEntity",
                        employee.Id,
                        null,
                        EmployeeMapper.ToLogObject(employee),
                        "Import Excel - Tạo mới nhân viên " + employee.FullName);

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add($"Dòng {rowNumber}: {ex.Message}");
                }
            }

            return result;
        }

        private static EmployeeCommandFields ReadRow(IXLRangeRow row)
        {
            return new EmployeeCommandFields
            {
                Code = GetCellString(row, 1),
                FirstName = GetCellString(row, 2),
                LastName = GetCellString(row, 3),
                FullName = GetCellString(row, 4),
                Phone = GetCellString(row, 5),
                SecondaryPhone = GetCellString(row, 6),
                Email = GetCellString(row, 7),
                CompanyEmail = GetCellString(row, 8),
                DayOfBirth = ParseDate(row.Cell(9)) ?? default,
                Nationality = GetCellString(row, 10),
                Ethnicity = GetCellString(row, 11),
                Religion = GetCellString(row, 12),
                IdentityCard = GetCellString(row, 13),
                PlaceOfIsssuance = GetCellString(row, 14),
                IssuanceDate = ParseDate(row.Cell(15)) ?? default,
                PermanentAddress = GetCellString(row, 16),
                NowAddress = GetCellString(row, 17),
                CurrentCity = GetCellString(row, 18),
                CurrentWard = GetCellString(row, 19),
                BankAccountNumber = GetCellString(row, 20),
                Bankname = GetCellString(row, 21),
                BankBranchName = GetCellString(row, 22),
                BankAccountHolder = GetCellString(row, 23),
                TaxCode = GetCellString(row, 24),
                SocialInsuranceNumber = GetCellString(row, 25),
                HealthInsuranceNumber = GetCellString(row, 26),
                Level = GetCellString(row, 27),
                WorkingMode = GetCellString(row, 28),
                ContractType = GetCellString(row, 29),
                Status = GetCellString(row, 30),
                JoinDate = ParseDate(row.Cell(31)) ?? default,
                ResignationDate = ParseDate(row.Cell(32)),
                ResignationReason = GetCellString(row, 33)
            };
        }

        private static string GetCellString(IXLRangeRow row, int column)
        {
            return row.Cell(column).GetString().Trim();
        }

        private static DateTime? ParseDate(IXLCell cell)
        {
            if (cell.IsEmpty())
            {
                return null;
            }

            if (cell.TryGetValue(out DateTime dateValue))
            {
                return dateValue;
            }

            return DateTime.TryParse(cell.GetString(), out DateTime parsed) ? parsed : null;
        }
    }

    internal sealed class EmployeeExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class EmployeeExcelColumns
    {
        public static readonly EmployeeExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã nhân viên", Required = true },
            new() { Title = "Họ", Required = true },
            new() { Title = "Tên", Required = true },
            new() { Title = "Họ tên đầy đủ", Required = false },
            new() { Title = "Số điện thoại", Required = true },
            new() { Title = "SĐT phụ", Required = false },
            new() { Title = "Email", Required = true },
            new() { Title = "Email công ty", Required = false },
            new() { Title = "Ngày sinh", Required = true },
            new() { Title = "Quốc tịch", Required = false },
            new() { Title = "Dân tộc", Required = false },
            new() { Title = "Tôn giáo", Required = false },
            new() { Title = "Số CCCD", Required = true },
            new() { Title = "Nơi cấp CCCD", Required = true },
            new() { Title = "Ngày cấp CCCD", Required = true },
            new() { Title = "Địa chỉ thường trú", Required = false },
            new() { Title = "Địa chỉ hiện tại", Required = false },
            new() { Title = "Tỉnh/TP hiện tại", Required = false },
            new() { Title = "Phường/Xã hiện tại", Required = false },
            new() { Title = "Số TK ngân hàng", Required = false },
            new() { Title = "Tên ngân hàng", Required = false },
            new() { Title = "Chi nhánh NH", Required = false },
            new() { Title = "Chủ tài khoản", Required = false },
            new() { Title = "Mã số thuế", Required = false },
            new() { Title = "Số BHXH", Required = false },
            new() { Title = "Số BHYT", Required = false },
            new() { Title = "Cấp bậc", Required = false },
            new() { Title = "Hình thức làm việc", Required = false },
            new() { Title = "Loại hợp đồng", Required = false },
            new() { Title = "Trạng thái NV", Required = false },
            new() { Title = "Ngày vào làm", Required = true },
            new() { Title = "Ngày nghỉ việc", Required = false },
            new() { Title = "Lý do nghỉ việc", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<EmployeeExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class EmployeeExcelWriter
    {
        private static readonly XLColor RequiredHeaderColor = XLColor.FromHtml("#FFC000");
        private static readonly XLColor OptionalHeaderColor = XLColor.FromHtml("#92D050");

        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = EmployeeExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (var col = 0; col < columns.Count; col++)
            {
                EmployeeExcelColumnDefinition definition = columns[col];
                IXLCell cell = worksheet.Cell(1, col + 1);
                cell.Value = definition.Required ? $"{definition.Title}*" : definition.Title;

                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.Black;
                cell.Style.Fill.BackgroundColor = definition.Required ? RequiredHeaderColor : OptionalHeaderColor;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteEmployeeRow(
            IXLWorksheet worksheet,
            int row,
            EmployeeEntity employee,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                employee.Code,
                employee.FirstName,
                employee.LastName,
                employee.FullName,
                employee.Phone,
                employee.SecondaryPhone,
                employee.Email,
                employee.CompanyEmail,
                employee.DayOfBirth.ToString("yyyy-MM-dd"),
                employee.Nationality,
                employee.Ethnicity,
                employee.Religion,
                employee.IdentityCard,
                employee.PlaceOfIsssuance,
                employee.IssuanceDate.ToString("yyyy-MM-dd"),
                employee.PermanentAddress,
                employee.NowAddress,
                employee.CurrentCity,
                employee.CurrentWard,
                employee.BankAccountNumber,
                employee.BankName,
                employee.BankBranchName,
                employee.BankAccountHolder,
                employee.TaxCode,
                employee.SocialInsuranceNumber,
                employee.HealthInsuranceNumber,
                employee.Level,
                employee.WorkingMode,
                employee.ContractType,
                employee.Status,
                employee.JoinDate.ToString("yyyy-MM-dd"),
                employee.ResignationDate?.ToString("yyyy-MM-dd"),
                employee.ResignationReason
            };

            if (includeExportOnlyColumns)
            {
                values.Add(employee.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
            }

            for (var col = 0; col < values.Count; col++)
            {
                IXLCell cell = worksheet.Cell(row, col + 1);
                cell.Value = values[col] ?? string.Empty;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }

        public static void ApplyColumnWidths(IXLWorksheet worksheet)
        {
            IXLColumns usedColumns = worksheet.ColumnsUsed();
            foreach (IXLColumn? column in usedColumns)
            {
                _ = column.AdjustToContents(8, 60);
                column.Width = Math.Max(column.Width + 2, 12);
            }
        }

        public static void FreezeHeaderRow(IXLWorksheet worksheet)
        {
            worksheet.SheetView.FreezeRows(1);
        }
    }
}

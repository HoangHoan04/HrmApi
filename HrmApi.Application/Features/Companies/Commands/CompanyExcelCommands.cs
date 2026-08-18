using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs;
using HrmApi.Application.Features.Companies.Commands;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Companies.Commands
{
    public class ExportCompaniesExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportCompaniesExcelQueryHandler : IRequestHandler<ExportCompaniesExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportCompaniesExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportCompaniesExcelQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CompanyEntities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var name = request.Name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            var companies = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachCongTy");
            CompanyExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < companies.Count; i++)
            {
                CompanyExcelWriter.WriteCompanyRow(worksheet, i + 2, companies[i], includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadCompanyExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadCompanyExcelTemplateQueryHandler : IRequestHandler<DownloadCompanyExcelTemplateQuery, byte[]>
    {
        public Task<byte[]> Handle(DownloadCompanyExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("MauImport");
            CompanyExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }

    public class ImportCompaniesExcelCommand : IRequest<CompanyImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class CompanyImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ImportCompaniesExcelCommandHandler : IRequestHandler<ImportCompaniesExcelCommand, CompanyImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportCompaniesExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<CompanyImportResultDto> Handle(ImportCompaniesExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new CompanyImportResultDto();

            using var stream = new MemoryStream(request.FileContent);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? new List<IXLRangeRow>();

            result.TotalRows = rows.Count;

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber();
                try
                {
                    var command = ReadRow(row);
                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        continue;
                    }

                    await CreateCompanyCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var company = new CompanyEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    CompanyMapper.ApplyCommandFields(company, command);

                    _context.CompanyEntities.Add(company);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "CompanyEntity",
                        company.Id,
                        null,
                        CompanyMapper.ToLogObject(company),
                        "Import Excel - Tạo mới công ty " + company.Name);

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

        private static CompanyCommandFields ReadRow(IXLRangeRow row)
        {
            return new CompanyCommandFields
            {
                Code = GetCellString(row, 1),
                Name = GetCellString(row, 2),
                Description = GetCellString(row, 3),
                Address = GetCellString(row, 4),
                TaxCode = GetCellString(row, 5),
                Hotline = GetCellString(row, 6),
                Email = GetCellString(row, 7),
                Website = GetCellString(row, 8),
                Fax = GetCellString(row, 9),
                Country = GetCellString(row, 10),
                City = GetCellString(row, 11),
                District = GetCellString(row, 12),
                Ward = GetCellString(row, 13),
                BusinessRegistrationCode = GetCellString(row, 14),
                FoundedDate = ParseDate(row.Cell(15)),
                OperatingStatus = GetCellString(row, 16),
                LegalRepresentative = GetCellString(row, 17),
                LegalRepresentativePosition = GetCellString(row, 18),
                CompanyType = GetCellString(row, 19),
                Industry = GetCellString(row, 20),
                BankAccountNumber = GetCellString(row, 21),
                BankName = GetCellString(row, 22),
                BankBranch = GetCellString(row, 23),
                PrefixMaleCode = GetCellString(row, 24),
                PrefixFemaleCode = GetCellString(row, 25),
                PrefixFullTimeCode = GetCellString(row, 26),
                PrefixPartTimeCode = GetCellString(row, 27),
                DayComputeSalary = ParseDate(row.Cell(28)),
                IsComputePrevMonth = ParseBool(row.Cell(29)),
                TimeZone = GetCellString(row, 30),
                DefaultLanguage = GetCellString(row, 31),
                LogoUrl = GetCellString(row, 32),
                IsActive = ParseBool(row.Cell(33)) ?? true,
                SocialInsuranceCode = GetCellString(row, 34)
            };
        }

        private static string GetCellString(IXLRangeRow row, int column)
        {
            IXLCell cell = row.Cell(column);
            if (cell.IsEmpty()) return string.Empty;
            try
            {
                var text = cell.GetFormattedString();
                if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
            }
            catch
            {
            }
            return cell.Value.ToString()?.Trim() ?? string.Empty;
        }

        private static DateTime? ParseDate(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue(out DateTime dateValue)) return DateTime.SpecifyKind(dateValue, DateTimeKind.Utc);
            var text = GetCellString(cell.AsRange().FirstRow(), 1);
            if (string.IsNullOrWhiteSpace(text)) return null;
            string[] formats = ["dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "yyyy/MM/dd", "MM/dd/yyyy"];
            if (DateTime.TryParseExact(text, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var exact))
                return DateTime.SpecifyKind(exact, DateTimeKind.Utc);
            if (DateTime.TryParse(text, System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), System.Globalization.DateTimeStyles.None, out var viDate))
                return DateTime.SpecifyKind(viDate, DateTimeKind.Utc);
            if (DateTime.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed))
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            if (DateTime.TryParse(text, out var localParsed))
                return DateTime.SpecifyKind(localParsed, DateTimeKind.Utc);
            return null;
        }

        private static bool? ParseBool(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue(out bool boolValue)) return boolValue;
            var text = cell.GetString().Trim().ToLower();
            return text switch
            {
                "1" or "true" or "có" or "co" or "yes" => true,
                "0" or "false" or "không" or "khong" or "no" => false,
                _ => null
            };
        }
    }

    internal sealed class CompanyExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class CompanyExcelColumns
    {
        public static readonly CompanyExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã công ty", Required = true },
            new() { Title = "Tên công ty", Required = true },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Địa chỉ", Required = false },
            new() { Title = "Mã số thuế", Required = false },
            new() { Title = "Hotline", Required = false },
            new() { Title = "Email", Required = false },
            new() { Title = "Website", Required = false },
            new() { Title = "Fax", Required = false },
            new() { Title = "Quốc gia", Required = false },
            new() { Title = "Thành phố/Tỉnh", Required = false },
            new() { Title = "Quận/Huyện", Required = false },
            new() { Title = "Phường/Xã", Required = false },
            new() { Title = "Mã ĐKKD", Required = false },
            new() { Title = "Ngày thành lập", Required = false },
            new() { Title = "Trạng thái hoạt động", Required = false },
            new() { Title = "Người đại diện PL", Required = false },
            new() { Title = "Chức danh ĐDPL", Required = false },
            new() { Title = "Loại hình DN", Required = false },
            new() { Title = "Ngành nghề", Required = false },
            new() { Title = "Số TK ngân hàng", Required = false },
            new() { Title = "Tên ngân hàng", Required = false },
            new() { Title = "Chi nhánh NH", Required = false },
            new() { Title = "Tiền tố mã NV nam", Required = false },
            new() { Title = "Tiền tố mã NV nữ", Required = false },
            new() { Title = "Tiền tố mã NV full-time", Required = false },
            new() { Title = "Tiền tố mã NV part-time", Required = false },
            new() { Title = "Ngày tính lương", Required = false },
            new() { Title = "Tính lương tháng trước", Required = false },
            new() { Title = "Múi giờ", Required = false },
            new() { Title = "Ngôn ngữ mặc định", Required = false },
            new() { Title = "Logo URL", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Mã BHXH", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<CompanyExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns) =>
            Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
    }

    internal static class CompanyExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = CompanyExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (var col = 0; col < columns.Count; col++)
            {
                var definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteCompanyRow(
            IXLWorksheet worksheet,
            int row,
            CompanyEntity company,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                company.Code,
                company.Name,
                company.Description,
                company.Address,
                company.TaxCode,
                company.Hotline,
                company.Email,
                company.Website,
                company.Fax,
                company.Country,
                company.City,
                company.District,
                company.Ward,
                company.BusinessRegistrationCode,
                company.FoundedDate?.ToString("yyyy-MM-dd"),
                company.OperatingStatus,
                company.LegalRepresentative,
                company.LegalRepresentativePosition,
                company.CompanyType,
                company.Industry,
                company.BankAccountNumber,
                company.BankName,
                company.BankBranch,
                company.PrefixMaleCode,
                company.PrefixFemaleCode,
                company.PrefixFullTimeCode,
                company.PrefixPartTimeCode,
                company.DayComputeSalary?.ToString("yyyy-MM-dd"),
                company.IsComputePrevMonth?.ToString(),
                company.TimeZone,
                company.DefaultLanguage,
                company.LogoUrl,
                company.IsActive.ToString(),
                company.SocialInsuranceCode,
            };

            if (includeExportOnlyColumns)
            {
                values.Add(company.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
            }

            for (var col = 0; col < values.Count; col++)
            {
                var cell = worksheet.Cell(row, col + 1);
                cell.Value = values[col] ?? string.Empty;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }
    }
}

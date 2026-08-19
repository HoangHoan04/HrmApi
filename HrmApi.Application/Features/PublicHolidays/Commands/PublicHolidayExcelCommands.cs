using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PublicHolidays.Commands
{
    public class ExportPublicHolidaysExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportPublicHolidaysExcelQueryHandler : IRequestHandler<ExportPublicHolidaysExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportPublicHolidaysExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportPublicHolidaysExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PublicHolidayEntity> query = _context.PublicHolidayEntities.AsNoTracking()
                .Include(x => x.Company);

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                string code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                string name = request.Name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId.Value);
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            List<PublicHolidayEntity> holidays = await query.OrderBy(x => x.HolidayDate).ToListAsync(cancellationToken);
            Dictionary<Guid, string> companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachNgayNghiLe");
            PublicHolidayExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < holidays.Count; i++)
            {
                PublicHolidayEntity holiday = holidays[i];
                string? companyCode = holiday.CompanyId.HasValue && companyDict.TryGetValue(holiday.CompanyId.Value, out string? cc) ? cc : null;
                PublicHolidayExcelWriter.WriteHolidayRow(worksheet, i + 2, holiday, companyCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadPublicHolidayExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadPublicHolidayExcelTemplateQueryHandler : IRequestHandler<DownloadPublicHolidayExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadPublicHolidayExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadPublicHolidayExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("NgayNghiLe");
            PublicHolidayExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            PublicHolidayExcelWriter.WriteTemplateSampleRow(worksheet);
            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            var companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "CongTy", "Mã công ty", "Tên công ty", companies.Select(x => (x.Code, x.Name)));

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class ImportPublicHolidaysExcelCommand : IRequest<PublicHolidayImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class PublicHolidayImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportPublicHolidaysExcelCommandHandler : IRequestHandler<ImportPublicHolidaysExcelCommand, PublicHolidayImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportPublicHolidaysExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<PublicHolidayImportResultDto> Handle(ImportPublicHolidaysExcelCommand request, CancellationToken cancellationToken)
        {
            PublicHolidayImportResultDto result = new();

            using MemoryStream stream = new(request.FileContent);
            using XLWorkbook workbook = new(stream);
            IXLWorksheet worksheet = workbook.Worksheet(1);
            List<IXLRangeRow> rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? [];

            result.TotalRows = rows.Count;

            Dictionary<string, Guid> companyDict = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            foreach (IXLRangeRow? row in rows)
            {
                int rowNumber = row.RowNumber();
                try
                {
                    string rawCompanyCode = ExcelHelper.GetCellString(row, 4).Trim();
                    Guid? companyId = null;

                    if (!string.IsNullOrWhiteSpace(rawCompanyCode))
                    {
                        if (!companyDict.TryGetValue(rawCompanyCode.ToLower(), out Guid matchedCompanyId))
                        {
                            throw new InvalidOperationException($"Mã công ty '{rawCompanyCode}' không tồn tại hoặc đã bị ngừng hoạt động.");
                        }
                        companyId = matchedCompanyId;
                    }

                    DateOnly? holidayDate = ExcelHelper.ParseDateOnly(row.Cell(3));

                    PublicHolidayCommandFields command = new()
                    {
                        Code = ExcelHelper.GetCellString(row, 1),
                        Name = ExcelHelper.GetCellString(row, 2),
                        HolidayDate = holidayDate ?? default,
                        CompanyId = companyId,
                        IsRecurringYearly = ExcelHelper.ParseBool(row.Cell(5)) ?? true,
                        Description = ExcelHelper.GetCellString(row, 6),
                        IsActive = ExcelHelper.ParseBool(row.Cell(7)) ?? true
                    };

                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("LE-TET-DUONG-LICH", StringComparison.OrdinalIgnoreCase) ||
                        command.Code.Equals("LE-MAU01", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(command.Code))
                    {
                        throw new InvalidOperationException("Mã ngày lễ là bắt buộc.");
                    }

                    if (string.IsNullOrWhiteSpace(command.Name))
                    {
                        throw new InvalidOperationException("Tên ngày lễ là bắt buộc.");
                    }

                    if (command.HolidayDate == default)
                    {
                        throw new InvalidOperationException("Ngày nghỉ lễ là bắt buộc (định dạng dd/MM/yyyy).");
                    }

                    bool exists = await _context.PublicHolidayEntities.AnyAsync(
                        x => !x.IsDeleted && x.Code.ToLower() == command.Code.Trim().ToLower(),
                        cancellationToken);
                    if (exists)
                    {
                        throw new InvalidOperationException($"Mã ngày lễ '{command.Code}' đã tồn tại.");
                    }

                    PublicHolidayEntity entity = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        HolidayDate = command.HolidayDate,
                        IsRecurringYearly = command.IsRecurringYearly ?? false,
                        IsActive = command.IsActive ?? true
                    };
                    PublicHolidayMapper.ApplyCommandFields(entity, command);

                    _ = _context.PublicHolidayEntities.Add(entity);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "PublicHolidayEntity",
                        entity.Id,
                        null,
                        PublicHolidayMapper.ToLogObject(entity),
                        "Import Excel - Tạo mới ngày nghỉ lễ " + entity.Name);

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
    }

    internal sealed class PublicHolidayExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class PublicHolidayExcelColumns
    {
        public static readonly PublicHolidayExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã ngày lễ", Required = true },
            new() { Title = "Tên ngày lễ", Required = true },
            new() { Title = "Ngày nghỉ lễ (dd/MM/yyyy)", Required = true },
            new() { Title = "Mã công ty (Parent)", Required = false },
            new() { Title = "Lặp lại hàng năm?", Required = false },
            new() { Title = "Mô tả / Ghi chú", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true }
        };

        public static IEnumerable<PublicHolidayExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class PublicHolidayExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<PublicHolidayExcelColumnDefinition> columns = PublicHolidayExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                PublicHolidayExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "LE-TET-DUONG-LICH",
                "Tết Dương Lịch",
                "01/01/2026",
                "CT01",
                "Có",
                "Nghỉ Tết Dương Lịch theo quy định của nhà nước",
                "Có"
            ];

            for (int col = 0; col < sampleValues.Count; col++)
            {
                IXLCell cell = worksheet.Cell(2, col + 1);
                cell.Value = sampleValues[col];
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.FontColor = XLColor.DarkGray;
            }
        }

        public static void WriteHolidayRow(
            IXLWorksheet worksheet,
            int row,
            PublicHolidayEntity holiday,
            string? companyCode,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                holiday.Code,
                holiday.Name,
                holiday.HolidayDate.ToString("dd/MM/yyyy"),
                companyCode,
                holiday.IsRecurringYearly ? "Có" : "Không",
                holiday.Description,
                holiday.IsActive ? "Có" : "Không"
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(holiday.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
            }

            for (int col = 0; col < values.Count; col++)
            {
                IXLCell cell = worksheet.Cell(row, col + 1);
                cell.Value = values[col] ?? string.Empty;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }
    }
}

using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HrmApi.Application.Features.ShiftMasters.Commands
{
    public class ExportShiftMastersExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportShiftMastersExcelQueryHandler : IRequestHandler<ExportShiftMastersExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportShiftMastersExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportShiftMastersExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ShiftMasterEntity> query = _context.ShiftMasterEntities.AsNoTracking()
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

            List<ShiftMasterEntity> shifts = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            Dictionary<Guid, string> companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachCaLamViec");
            ShiftMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < shifts.Count; i++)
            {
                ShiftMasterEntity shift = shifts[i];
                string? companyCode = shift.CompanyId.HasValue && companyDict.TryGetValue(shift.CompanyId.Value, out string? cc) ? cc : null;
                ShiftMasterExcelWriter.WriteShiftRow(worksheet, i + 2, shift, companyCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadShiftMasterExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadShiftMasterExcelTemplateQueryHandler : IRequestHandler<DownloadShiftMasterExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadShiftMasterExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadShiftMasterExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("CaLamViec");
            ShiftMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            ShiftMasterExcelWriter.WriteTemplateSampleRow(worksheet);
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

    public class ImportShiftMastersExcelCommand : IRequest<ShiftMasterImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class ShiftMasterImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportShiftMastersExcelCommandHandler : IRequestHandler<ImportShiftMastersExcelCommand, ShiftMasterImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportShiftMastersExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<ShiftMasterImportResultDto> Handle(ImportShiftMastersExcelCommand request, CancellationToken cancellationToken)
        {
            ShiftMasterImportResultDto result = new();

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

                    TimeSpan? startTime = ParseTimeSpan(row.Cell(5));
                    TimeSpan? endTime = ParseTimeSpan(row.Cell(6));
                    TimeSpan? breakStartTime = ParseTimeSpan(row.Cell(7));
                    TimeSpan? breakEndTime = ParseTimeSpan(row.Cell(8));

                    ShiftMasterCommandFields command = new()
                    {
                        Code = ExcelHelper.GetCellString(row, 1),
                        Name = ExcelHelper.GetCellString(row, 2),
                        Description = ExcelHelper.GetCellString(row, 3),
                        CompanyId = companyId,
                        StartTime = startTime,
                        EndTime = endTime,
                        BreakStartTime = breakStartTime,
                        BreakEndTime = breakEndTime,
                        BreakMinutes = ExcelHelper.ParseInt(row.Cell(9)) ?? 60,
                        WorkingMinutes = ExcelHelper.ParseInt(row.Cell(10)) ?? 480,
                        IsOvernight = ExcelHelper.ParseBool(row.Cell(11)) ?? false,
                        IsActive = ExcelHelper.ParseBool(row.Cell(12)) ?? true
                    };

                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("CA-HC", StringComparison.OrdinalIgnoreCase) ||
                        command.Code.Equals("CA-MAU01", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(command.Code))
                    {
                        throw new InvalidOperationException("Mã ca làm việc là bắt buộc.");
                    }

                    if (string.IsNullOrWhiteSpace(command.Name))
                    {
                        throw new InvalidOperationException("Tên ca làm việc là bắt buộc.");
                    }

                    if (!command.StartTime.HasValue)
                    {
                        throw new InvalidOperationException("Giờ bắt đầu ca là bắt buộc (định dạng HH:mm).");
                    }

                    if (!command.EndTime.HasValue)
                    {
                        throw new InvalidOperationException("Giờ kết thúc ca là bắt buộc (định dạng HH:mm).");
                    }

                    bool exists = await _context.ShiftMasterEntities.AnyAsync(
                        x => !x.IsDeleted && x.Code.ToLower() == command.Code.Trim().ToLower(),
                        cancellationToken);
                    if (exists)
                    {
                        throw new InvalidOperationException($"Mã ca làm việc '{command.Code}' đã tồn tại.");
                    }

                    ShiftMasterEntity entity = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        StartTime = command.StartTime.Value,
                        EndTime = command.EndTime.Value,
                        BreakStartTime = command.BreakStartTime,
                        BreakEndTime = command.BreakEndTime,
                        BreakMinutes = command.BreakMinutes ?? 60,
                        WorkingMinutes = command.WorkingMinutes ?? 480,
                        IsOvernight = command.IsOvernight ?? false,
                        IsActive = command.IsActive ?? true
                    };
                    ShiftMasterMapper.ApplyCommandFields(entity, command);

                    _ = _context.ShiftMasterEntities.Add(entity);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "ShiftMasterEntity",
                        entity.Id,
                        null,
                        ShiftMasterMapper.ToLogObject(entity),
                        "Import Excel - Tạo mới ca làm việc " + entity.Name);

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

        private static TimeSpan? ParseTimeSpan(IXLCell cell)
        {
            if (cell.IsEmpty())
            {
                return null;
            }

            if (cell.TryGetValue(out DateTime dt))
            {
                return dt.TimeOfDay;
            }

            if (cell.TryGetValue(out TimeSpan ts))
            {
                return ts;
            }

            string text = cell.GetString().Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string[] formats = ["hh\\:mm\\:ss", "h\\:mm\\:ss", "hh\\:mm", "h\\:mm", "HH\\:mm\\:ss", "HH\\:mm"];
            if (TimeSpan.TryParseExact(text, formats, CultureInfo.InvariantCulture, out TimeSpan exactTs))
            {
                return exactTs;
            }

            return TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out TimeSpan parsedTs)
                ? parsedTs
                : DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDt) ? parsedDt.TimeOfDay : null;
        }
    }

    internal sealed class ShiftMasterExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class ShiftMasterExcelColumns
    {
        public static readonly ShiftMasterExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã ca", Required = true },
            new() { Title = "Tên ca", Required = true },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Mã công ty (Parent)", Required = false },
            new() { Title = "Giờ bắt đầu (HH:mm)", Required = true },
            new() { Title = "Giờ kết thúc (HH:mm)", Required = true },
            new() { Title = "Bắt đầu nghỉ (HH:mm)", Required = false },
            new() { Title = "Kết thúc nghỉ (HH:mm)", Required = false },
            new() { Title = "Phút nghỉ giữa ca", Required = false },
            new() { Title = "Phút làm việc chuẩn", Required = false },
            new() { Title = "Ca qua đêm?", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true }
        };

        public static IEnumerable<ShiftMasterExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class ShiftMasterExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<ShiftMasterExcelColumnDefinition> columns = ShiftMasterExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                ShiftMasterExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "CA-HC",
                "Ca hành chính",
                "Ca làm việc 8 tiếng văn phòng",
                "CT01",
                "08:00",
                "17:00",
                "12:00",
                "13:00",
                "60",
                "480",
                "Không",
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

        public static void WriteShiftRow(
            IXLWorksheet worksheet,
            int row,
            ShiftMasterEntity shift,
            string? companyCode,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                shift.Code,
                shift.Name,
                shift.Description,
                companyCode,
                shift.StartTime.ToString(@"hh\:mm"),
                shift.EndTime.ToString(@"hh\:mm"),
                shift.BreakStartTime?.ToString(@"hh\:mm"),
                shift.BreakEndTime?.ToString(@"hh\:mm"),
                shift.BreakMinutes.ToString(),
                shift.WorkingMinutes.ToString(),
                shift.IsOvernight ? "Có" : "Không",
                shift.IsActive ? "Có" : "Không"
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(shift.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

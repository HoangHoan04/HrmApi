using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HrmApi.Application.Features.TimeKeepingStandards.Commands
{
    public class ExportTimeKeepingStandardsExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportTimeKeepingStandardsExcelQueryHandler : IRequestHandler<ExportTimeKeepingStandardsExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportTimeKeepingStandardsExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportTimeKeepingStandardsExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<TimeKeepingStandardEntity> query = _context.TimeKeepingStandardEntities.AsNoTracking()
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

            List<TimeKeepingStandardEntity> standards = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            Dictionary<Guid, string> companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachChuanChamCong");
            TimeKeepingStandardExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < standards.Count; i++)
            {
                TimeKeepingStandardEntity standard = standards[i];
                string? companyCode = standard.CompanyId.HasValue && companyDict.TryGetValue(standard.CompanyId.Value, out string? cc) ? cc : null;
                TimeKeepingStandardExcelWriter.WriteStandardRow(worksheet, i + 2, standard, companyCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadTimeKeepingStandardExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadTimeKeepingStandardExcelTemplateQueryHandler : IRequestHandler<DownloadTimeKeepingStandardExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadTimeKeepingStandardExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadTimeKeepingStandardExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("ChuanChamCong");
            TimeKeepingStandardExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            TimeKeepingStandardExcelWriter.WriteTemplateSampleRow(worksheet);
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

    public class ImportTimeKeepingStandardsExcelCommand : IRequest<TimeKeepingStandardImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class TimeKeepingStandardImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportTimeKeepingStandardsExcelCommandHandler : IRequestHandler<ImportTimeKeepingStandardsExcelCommand, TimeKeepingStandardImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportTimeKeepingStandardsExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<TimeKeepingStandardImportResultDto> Handle(ImportTimeKeepingStandardsExcelCommand request, CancellationToken cancellationToken)
        {
            TimeKeepingStandardImportResultDto result = new();

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
                    TimeKeepingStandardCommandFields command = ReadRow(row, companyDict);
                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("TC-VP01", StringComparison.OrdinalIgnoreCase) ||
                        command.Code.Equals("TC-MAU01", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(command.Code))
                    {
                        throw new InvalidOperationException("Mã chuẩn chấm công là bắt buộc.");
                    }

                    if (string.IsNullOrWhiteSpace(command.Name))
                    {
                        throw new InvalidOperationException("Tên chuẩn chấm công là bắt buộc.");
                    }

                    if ((command.AllowedRadiusMeters ?? 200) <= 0)
                    {
                        throw new InvalidOperationException("Bán kính cho phép GPS phải lớn hơn 0.");
                    }

                    bool exists = await _context.TimeKeepingStandardEntities.AnyAsync(
                        x => !x.IsDeleted && x.Code.ToLower() == command.Code.Trim().ToLower(),
                        cancellationToken);
                    if (exists)
                    {
                        throw new InvalidOperationException($"Mã chuẩn chấm công '{command.Code}' đã tồn tại.");
                    }

                    if (command.CompanyId.HasValue && command.CompanyId != Guid.Empty)
                    {
                        bool companyExists = await _context.CompanyEntities.AnyAsync(
                            x => x.Id == command.CompanyId && !x.IsDeleted,
                            cancellationToken);
                        if (!companyExists)
                        {
                            throw new InvalidOperationException("Công ty không tồn tại.");
                        }
                    }

                    TimeKeepingStandardEntity standard = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        AllowedRadiusMeters = command.AllowedRadiusMeters ?? 200,
                        LateGraceMinutes = command.LateGraceMinutes ?? 0,
                        EarlyLeaveGraceMinutes = command.EarlyLeaveGraceMinutes ?? 0,
                        NightStartTime = command.NightStartTime ?? new TimeSpan(22, 0, 0),
                        NightEndTime = command.NightEndTime ?? new TimeSpan(6, 0, 0),
                        IsActive = command.IsActive ?? true
                    };
                    TimeKeepingStandardMapper.ApplyCommandFields(standard, command);

                    _ = _context.TimeKeepingStandardEntities.Add(standard);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "TimeKeepingStandardEntity",
                        standard.Id,
                        null,
                        TimeKeepingStandardMapper.ToLogObject(standard),
                        "Import Excel - Tạo mới chuẩn chấm công " + standard.Name);

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

        private static TimeKeepingStandardCommandFields ReadRow(IXLRangeRow row, Dictionary<string, Guid> companyDict)
        {
            string companyCode = ExcelHelper.GetCellString(row, 4).Trim().ToLower();
            Guid? companyId = !string.IsNullOrWhiteSpace(companyCode) && companyDict.TryGetValue(companyCode, out Guid cid) ? cid : null;

            return new TimeKeepingStandardCommandFields
            {
                Code = ExcelHelper.GetCellString(row, 1),
                Name = ExcelHelper.GetCellString(row, 2),
                Description = ExcelHelper.GetCellString(row, 3),
                CompanyId = companyId,
                AllowedRadiusMeters = ExcelHelper.ParseInt(row.Cell(5)) ?? 200,
                LateGraceMinutes = ExcelHelper.ParseInt(row.Cell(6)) ?? 0,
                EarlyLeaveGraceMinutes = ExcelHelper.ParseInt(row.Cell(7)) ?? 0,
                NightStartTime = ParseTimeSpan(row.Cell(8)) ?? new TimeSpan(22, 0, 0),
                NightEndTime = ParseTimeSpan(row.Cell(9)) ?? new TimeSpan(6, 0, 0),
                IsActive = ExcelHelper.ParseBool(row.Cell(10)) ?? true
            };
        }

        public static TimeSpan? ParseTimeSpan(IXLCell cell)
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

    internal sealed class TimeKeepingStandardExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class TimeKeepingStandardExcelColumns
    {
        public static readonly TimeKeepingStandardExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã chuẩn chấm công", Required = true },
            new() { Title = "Tên chuẩn chấm công", Required = true },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Mã công ty", Required = false },
            new() { Title = "Bán kính GPS cho phép (m)", Required = false },
            new() { Title = "Phút ân hạn đi trễ (phút)", Required = false },
            new() { Title = "Phút ân hạn về sớm (phút)", Required = false },
            new() { Title = "Bắt đầu giờ đêm (HH:mm)", Required = false },
            new() { Title = "Kết thúc giờ đêm (HH:mm)", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true }
        };

        public static IEnumerable<TimeKeepingStandardExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class TimeKeepingStandardExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<TimeKeepingStandardExcelColumnDefinition> columns = TimeKeepingStandardExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                TimeKeepingStandardExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "TC-VP01",
                "Chuẩn chấm công văn phòng",
                "Áp dụng cho khối văn phòng trụ sở chính",
                "CT01",
                "200",
                "10",
                "5",
                "22:00",
                "06:00",
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

        public static void WriteStandardRow(
            IXLWorksheet worksheet,
            int row,
            TimeKeepingStandardEntity standard,
            string? companyCode,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                standard.Code,
                standard.Name,
                standard.Description,
                companyCode,
                standard.AllowedRadiusMeters.ToString(),
                standard.LateGraceMinutes.ToString(),
                standard.EarlyLeaveGraceMinutes.ToString(),
                standard.NightStartTime.ToString(@"hh\:mm"),
                standard.NightEndTime.ToString(@"hh\:mm"),
                standard.IsActive ? "Có" : "Không"
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(standard.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

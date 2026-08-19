using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.EmployeeWorkPatterns
{
    public class ExportEmployeeWorkPatternsExcelQuery : IRequest<byte[]>
    {
        public Guid? EmployeeId { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ExportEmployeeWorkPatternsExcelQueryHandler : IRequestHandler<ExportEmployeeWorkPatternsExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportEmployeeWorkPatternsExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportEmployeeWorkPatternsExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<EmployeeWorkPatternEntity> query = _context.EmployeeWorkPatternEntities.AsNoTracking()
                .Include(x => x.Employee)
                .Include(x => x.ShiftMaster)
                .Include(x => x.Branch)
                .Where(x => !x.IsDeleted);

            if (request.EmployeeId.HasValue)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            }

            if (request.ShiftMasterId.HasValue)
            {
                query = query.Where(x => x.ShiftMasterId == request.ShiftMasterId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            List<EmployeeWorkPatternEntity> patterns = await query
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.EffectiveFrom)
                .ToListAsync(cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachMauPhanCa");
            EmployeeWorkPatternExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < patterns.Count; i++)
            {
                EmployeeWorkPatternExcelWriter.WritePatternRow(worksheet, i + 2, patterns[i], includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadEmployeeWorkPatternExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadEmployeeWorkPatternExcelTemplateQueryHandler : IRequestHandler<DownloadEmployeeWorkPatternExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadEmployeeWorkPatternExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadEmployeeWorkPatternExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("MauPhanCa");
            EmployeeWorkPatternExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            EmployeeWorkPatternExcelWriter.WriteTemplateSampleRow(worksheet);
            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, Name = $"{x.LastName} {x.FirstName}" })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "NhanVien", "Mã nhân viên", "Họ và tên", employees.Select(x => (x.Code, x.Name)));

            var shifts = await _context.ShiftMasterEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "CaLamViec", "Mã ca", "Tên ca", shifts.Select(x => (x.Code, x.Name)));

            var branches = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "ChiNhanh", "Mã chi nhánh", "Tên chi nhánh", branches.Select(x => (x.Code, x.Name)));

            List<(string Code, string Name)> patternTypes =
            [
                ("FIXED_WEEKLY", "Cố định theo tuần"),
                ("ROSTER", "Lịch xoay ca / Roster")
            ];
            ExcelHelper.WriteReferenceSheet(workbook, "LoaiQuyLuat", "Mã loại", "Tên loại", patternTypes);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class ImportEmployeeWorkPatternsExcelCommand : IRequest<EmployeeWorkPatternImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class EmployeeWorkPatternImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportEmployeeWorkPatternsExcelCommandHandler : IRequestHandler<ImportEmployeeWorkPatternsExcelCommand, EmployeeWorkPatternImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportEmployeeWorkPatternsExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<EmployeeWorkPatternImportResultDto> Handle(ImportEmployeeWorkPatternsExcelCommand request, CancellationToken cancellationToken)
        {
            EmployeeWorkPatternImportResultDto result = new();

            using MemoryStream stream = new(request.FileContent);
            using XLWorkbook workbook = new(stream);
            IXLWorksheet worksheet = workbook.Worksheet(1);
            List<IXLRangeRow> rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? [];

            result.TotalRows = rows.Count;

            Dictionary<string, Guid> empDict = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            Dictionary<string, Guid> shiftDict = await _context.ShiftMasterEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            Dictionary<string, Guid> branchDict = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            foreach (IXLRangeRow? row in rows)
            {
                int rowNumber = row.RowNumber();
                try
                {
                    string empCode = ExcelHelper.GetCellString(row, 1).Trim();
                    string shiftCode = ExcelHelper.GetCellString(row, 2).Trim();
                    string rawPatternType = ExcelHelper.GetCellString(row, 3).Trim().ToUpper();

                    if (string.IsNullOrWhiteSpace(empCode) && string.IsNullOrWhiteSpace(shiftCode))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (empCode.Equals("NV001", StringComparison.OrdinalIgnoreCase) &&
                        shiftCode.Equals("CA-HC", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(empCode) || !empDict.TryGetValue(empCode.ToLower(), out Guid employeeId))
                    {
                        throw new InvalidOperationException($"Mã nhân viên '{empCode}' không tồn tại hoặc đã bị ngừng hoạt động.");
                    }

                    if (string.IsNullOrWhiteSpace(shiftCode) || !shiftDict.TryGetValue(shiftCode.ToLower(), out Guid shiftMasterId))
                    {
                        throw new InvalidOperationException($"Mã ca làm việc '{shiftCode}' không tồn tại hoặc đã bị ngừng hoạt động.");
                    }

                    WorkPatternType patternType = rawPatternType switch
                    {
                        "ROSTER" or "2" => WorkPatternType.ROSTER,
                        _ => WorkPatternType.FIXED_WEEKLY
                    };

                    DateOnly? effectiveFrom = ExcelHelper.ParseDateOnly(row.Cell(11));
                    if (!effectiveFrom.HasValue || effectiveFrom.Value == default)
                    {
                        throw new InvalidOperationException("Ngày hiệu lực là bắt buộc (định dạng dd/MM/yyyy).");
                    }

                    DateOnly? effectiveTo = ExcelHelper.ParseDateOnly(row.Cell(12));
                    if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom.Value)
                    {
                        throw new InvalidOperationException("Ngày kết thúc không được trước ngày hiệu lực.");
                    }

                    string branchCode = ExcelHelper.GetCellString(row, 13).Trim();
                    Guid? branchId = null;
                    if (!string.IsNullOrWhiteSpace(branchCode))
                    {
                        if (!branchDict.TryGetValue(branchCode.ToLower(), out Guid matchedBranchId))
                        {
                            throw new InvalidOperationException($"Mã chi nhánh '{branchCode}' không tồn tại.");
                        }
                        branchId = matchedBranchId;
                    }

                    bool workMon = ExcelHelper.ParseBool(row.Cell(4)) ?? true;
                    bool workTue = ExcelHelper.ParseBool(row.Cell(5)) ?? true;
                    bool workWed = ExcelHelper.ParseBool(row.Cell(6)) ?? true;
                    bool workThu = ExcelHelper.ParseBool(row.Cell(7)) ?? true;
                    bool workFri = ExcelHelper.ParseBool(row.Cell(8)) ?? true;
                    bool workSat = ExcelHelper.ParseBool(row.Cell(9)) ?? false;
                    bool workSun = ExcelHelper.ParseBool(row.Cell(10)) ?? false;

                    if (!workMon && !workTue && !workWed && !workThu && !workFri && !workSat && !workSun)
                    {
                        throw new InvalidOperationException("Cần chọn ít nhất 1 ngày làm việc trong tuần.");
                    }

                    EmployeeWorkPatternEntity entity = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        EmployeeId = employeeId,
                        ShiftMasterId = shiftMasterId,
                        PatternType = patternType,
                        WorkOnMonday = workMon,
                        WorkOnTuesday = workTue,
                        WorkOnWednesday = workWed,
                        WorkOnThursday = workThu,
                        WorkOnFriday = workFri,
                        WorkOnSaturday = workSat,
                        WorkOnSunday = workSun,
                        EffectiveFrom = effectiveFrom.Value,
                        EffectiveTo = effectiveTo,
                        BranchId = branchId,
                        Note = ExcelHelper.GetCellString(row, 14),
                        IsActive = ExcelHelper.ParseBool(row.Cell(15)) ?? true
                    };

                    _ = _context.EmployeeWorkPatternEntities.Add(entity);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "EmployeeWorkPatternEntity",
                        entity.Id,
                        null,
                        new { entity.Id, entity.EmployeeId, entity.ShiftMasterId, entity.EffectiveFrom },
                        "Import Excel - Gán mẫu phân ca cho nhân viên " + empCode);

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

    internal sealed class EmployeeWorkPatternExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class EmployeeWorkPatternExcelColumns
    {
        public static readonly EmployeeWorkPatternExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã nhân viên", Required = true },
            new() { Title = "Mã ca làm việc", Required = true },
            new() { Title = "Loại quy luật", Required = false },
            new() { Title = "Làm T2?", Required = false },
            new() { Title = "Làm T3?", Required = false },
            new() { Title = "Làm T4?", Required = false },
            new() { Title = "Làm T5?", Required = false },
            new() { Title = "Làm T6?", Required = false },
            new() { Title = "Làm T7?", Required = false },
            new() { Title = "Làm CN?", Required = false },
            new() { Title = "Hiệu lực từ ngày (dd/MM/yyyy)", Required = true },
            new() { Title = "Hiệu lực đến ngày (dd/MM/yyyy)", Required = false },
            new() { Title = "Mã chi nhánh", Required = false },
            new() { Title = "Ghi chú", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true }
        };

        public static IEnumerable<EmployeeWorkPatternExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class EmployeeWorkPatternExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<EmployeeWorkPatternExcelColumnDefinition> columns = EmployeeWorkPatternExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                EmployeeWorkPatternExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "NV001",
                "CA-HC",
                "FIXED_WEEKLY",
                "Có",
                "Có",
                "Có",
                "Có",
                "Có",
                "Không",
                "Không",
                "01/01/2026",
                "",
                "CN01",
                "Áp dụng cho nhân viên văn phòng",
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

        public static void WritePatternRow(
            IXLWorksheet worksheet,
            int row,
            EmployeeWorkPatternEntity pattern,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                pattern.Employee?.Code ?? pattern.EmployeeId.ToString(),
                pattern.ShiftMaster?.Code ?? pattern.ShiftMasterId.ToString(),
                pattern.PatternType.ToString(),
                pattern.WorkOnMonday ? "Có" : "Không",
                pattern.WorkOnTuesday ? "Có" : "Không",
                pattern.WorkOnWednesday ? "Có" : "Không",
                pattern.WorkOnThursday ? "Có" : "Không",
                pattern.WorkOnFriday ? "Có" : "Không",
                pattern.WorkOnSaturday ? "Có" : "Không",
                pattern.WorkOnSunday ? "Có" : "Không",
                pattern.EffectiveFrom.ToString("dd/MM/yyyy"),
                pattern.EffectiveTo?.ToString("dd/MM/yyyy") ?? string.Empty,
                pattern.Branch?.Code ?? string.Empty,
                pattern.Note,
                pattern.IsActive ? "Có" : "Không"
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(pattern.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

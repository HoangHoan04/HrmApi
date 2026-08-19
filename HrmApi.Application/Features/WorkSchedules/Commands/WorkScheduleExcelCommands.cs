using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Timekeeping;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.WorkSchedules.Commands
{
    public class ExportWorkSchedulesExcelQuery : IRequest<byte[]>
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? ShiftMasterId { get; set; }
        public Guid? BranchId { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportWorkSchedulesExcelQueryHandler : IRequestHandler<ExportWorkSchedulesExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportWorkSchedulesExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportWorkSchedulesExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<WorkScheduledEmployeeEntity> query = _context.WorkScheduledEmployeeEntities.AsNoTracking()
                .Include(x => x.Employee)
                .Include(x => x.ShiftMaster)
                .Include(x => x.Branch);

            if (request.FromDate.HasValue)
            {
                query = query.Where(x => x.WorkDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(x => x.WorkDate <= request.ToDate.Value);
            }

            if (request.EmployeeId.HasValue)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            }

            if (request.ShiftMasterId.HasValue)
            {
                query = query.Where(x => x.ShiftMasterId == request.ShiftMasterId.Value);
            }

            if (request.BranchId.HasValue)
            {
                query = query.Where(x => x.BranchId == request.BranchId.Value);
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            List<WorkScheduledEmployeeEntity> schedules = await query
                .OrderByDescending(x => x.WorkDate)
                .ThenBy(x => x.Employee != null ? x.Employee.Code : string.Empty)
                .ToListAsync(cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachLichLamViec");
            WorkScheduleExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < schedules.Count; i++)
            {
                WorkScheduleExcelWriter.WriteScheduleRow(worksheet, i + 2, schedules[i], includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadWorkScheduleExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadWorkScheduleExcelTemplateQueryHandler : IRequestHandler<DownloadWorkScheduleExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadWorkScheduleExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadWorkScheduleExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("LichLamViec");
            WorkScheduleExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            WorkScheduleExcelWriter.WriteTemplateSampleRow(worksheet);
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

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class ImportWorkSchedulesExcelCommand : IRequest<WorkScheduleImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class WorkScheduleImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportWorkSchedulesExcelCommandHandler : IRequestHandler<ImportWorkSchedulesExcelCommand, WorkScheduleImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportWorkSchedulesExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<WorkScheduleImportResultDto> Handle(ImportWorkSchedulesExcelCommand request, CancellationToken cancellationToken)
        {
            WorkScheduleImportResultDto result = new();

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
                    DateOnly? workDate = ExcelHelper.ParseDateOnly(row.Cell(2));
                    string shiftCode = ExcelHelper.GetCellString(row, 3).Trim();

                    if (string.IsNullOrWhiteSpace(empCode) && !workDate.HasValue)
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

                    if (!workDate.HasValue || workDate.Value == default)
                    {
                        throw new InvalidOperationException("Ngày làm việc là bắt buộc (định dạng dd/MM/yyyy).");
                    }

                    if (string.IsNullOrWhiteSpace(shiftCode) || !shiftDict.TryGetValue(shiftCode.ToLower(), out Guid shiftMasterId))
                    {
                        throw new InvalidOperationException($"Mã ca làm việc '{shiftCode}' không tồn tại hoặc đã bị ngừng hoạt động.");
                    }

                    string branchCode = ExcelHelper.GetCellString(row, 4).Trim();
                    Guid? branchId = null;
                    if (!string.IsNullOrWhiteSpace(branchCode))
                    {
                        if (!branchDict.TryGetValue(branchCode.ToLower(), out Guid matchedBranchId))
                        {
                            throw new InvalidOperationException($"Mã chi nhánh '{branchCode}' không tồn tại.");
                        }
                        branchId = matchedBranchId;
                    }

                    WorkScheduledEmployeeEntity? existing = await _context.WorkScheduledEmployeeEntities
                        .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.WorkDate == workDate.Value && !x.IsDeleted, cancellationToken);

                    if (existing != null)
                    {
                        existing.ShiftMasterId = shiftMasterId;
                        if (branchId.HasValue)
                        {
                            existing.BranchId = branchId;
                        }

                        existing.Note = ExcelHelper.GetCellString(row, 5);
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        WorkScheduledEmployeeEntity entity = new()
                        {
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            EmployeeId = employeeId,
                            WorkDate = workDate.Value,
                            ShiftMasterId = shiftMasterId,
                            BranchId = branchId,
                            Note = ExcelHelper.GetCellString(row, 5)
                        };
                        _ = _context.WorkScheduledEmployeeEntities.Add(entity);
                    }

                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        existing != null ? ActionType.UPDATE : ActionType.CREATE,
                        "WorkScheduledEmployeeEntity",
                        existing?.Id ?? Guid.Empty,
                        null,
                        new { EmployeeId = employeeId, WorkDate = workDate.Value, ShiftMasterId = shiftMasterId },
                        $"Import Excel - Xếp lịch làm việc ngày {workDate.Value:dd/MM/yyyy} cho NV {empCode}");

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

    internal sealed class WorkScheduleExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class WorkScheduleExcelColumns
    {
        public static readonly WorkScheduleExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã nhân viên", Required = true },
            new() { Title = "Ngày làm việc (dd/MM/yyyy)", Required = true },
            new() { Title = "Mã ca làm việc", Required = true },
            new() { Title = "Mã chi nhánh", Required = false },
            new() { Title = "Ghi chú", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true }
        };

        public static IEnumerable<WorkScheduleExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class WorkScheduleExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<WorkScheduleExcelColumnDefinition> columns = WorkScheduleExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                WorkScheduleExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "NV001",
                "20/08/2026",
                "CA-HC",
                "CN01",
                "Phân ca theo kế hoạch tuần"
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

        public static void WriteScheduleRow(
            IXLWorksheet worksheet,
            int row,
            WorkScheduledEmployeeEntity schedule,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                schedule.Employee?.Code ?? schedule.EmployeeId.ToString(),
                schedule.WorkDate.ToString("dd/MM/yyyy"),
                schedule.ShiftMaster?.Code ?? schedule.ShiftMasterId?.ToString() ?? string.Empty,
                schedule.Branch?.Code ?? string.Empty,
                schedule.Note
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(schedule.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

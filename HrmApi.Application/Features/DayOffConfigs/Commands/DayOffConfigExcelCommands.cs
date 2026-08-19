using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Leave;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.DayOffConfigs.Commands
{
    public class ExportDayOffConfigsExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportDayOffConfigsExcelQueryHandler : IRequestHandler<ExportDayOffConfigsExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportDayOffConfigsExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportDayOffConfigsExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<DayOffConfigEntity> query = _context.DayOffConfigEntities.AsNoTracking()
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

            List<DayOffConfigEntity> configs = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            Dictionary<Guid, string> companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachCauHinhNghiPhep");
            DayOffConfigExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < configs.Count; i++)
            {
                DayOffConfigEntity config = configs[i];
                string? companyCode = config.CompanyId.HasValue && companyDict.TryGetValue(config.CompanyId.Value, out string? cc) ? cc : null;
                DayOffConfigExcelWriter.WriteConfigRow(worksheet, i + 2, config, companyCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadDayOffConfigExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadDayOffConfigExcelTemplateQueryHandler : IRequestHandler<DownloadDayOffConfigExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadDayOffConfigExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadDayOffConfigExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("CauHinhNghiPhep");
            DayOffConfigExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            DayOffConfigExcelWriter.WriteTemplateSampleRow(worksheet);
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

    public class ImportDayOffConfigsExcelCommand : IRequest<DayOffConfigImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class DayOffConfigImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportDayOffConfigsExcelCommandHandler : IRequestHandler<ImportDayOffConfigsExcelCommand, DayOffConfigImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportDayOffConfigsExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<DayOffConfigImportResultDto> Handle(ImportDayOffConfigsExcelCommand request, CancellationToken cancellationToken)
        {
            DayOffConfigImportResultDto result = new();

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

                    DayOffConfigCommandFields command = new()
                    {
                        Code = ExcelHelper.GetCellString(row, 1),
                        Name = ExcelHelper.GetCellString(row, 2),
                        Description = ExcelHelper.GetCellString(row, 3),
                        CompanyId = companyId,
                        DefaultDaysPerYear = ExcelHelper.ParseDecimal(row.Cell(5)) ?? 0,
                        IsPaid = ExcelHelper.ParseBool(row.Cell(6)) ?? true,
                        DeductBalance = ExcelHelper.ParseBool(row.Cell(7)) ?? true,
                        RequireAttachment = ExcelHelper.ParseBool(row.Cell(8)) ?? false,
                        MaxDaysPerRequest = ExcelHelper.ParseDecimal(row.Cell(9)),
                        MinNoticeDays = ExcelHelper.ParseInt(row.Cell(10)) ?? 0,
                        IsActive = ExcelHelper.ParseBool(row.Cell(11)) ?? true
                    };

                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("NP-NAM", StringComparison.OrdinalIgnoreCase) ||
                        command.Code.Equals("NP-MAU01", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(command.Code))
                    {
                        throw new InvalidOperationException("Mã cấu hình nghỉ phép là bắt buộc.");
                    }

                    if (string.IsNullOrWhiteSpace(command.Name))
                    {
                        throw new InvalidOperationException("Tên cấu hình nghỉ phép là bắt buộc.");
                    }

                    if (command.DefaultDaysPerYear.HasValue && command.DefaultDaysPerYear.Value < 0)
                    {
                        throw new InvalidOperationException("Số ngày mặc định/năm phải >= 0.");
                    }

                    if (command.MaxDaysPerRequest.HasValue && command.MaxDaysPerRequest.Value <= 0)
                    {
                        throw new InvalidOperationException("Số ngày tối đa mỗi đơn phải > 0.");
                    }

                    if (command.MinNoticeDays.HasValue && command.MinNoticeDays.Value < 0)
                    {
                        throw new InvalidOperationException("Số ngày báo trước tối thiểu phải >= 0.");
                    }

                    bool exists = await _context.DayOffConfigEntities.AnyAsync(
                        x => !x.IsDeleted && x.Code.ToLower() == command.Code.Trim().ToLower(),
                        cancellationToken);
                    if (exists)
                    {
                        throw new InvalidOperationException($"Mã cấu hình nghỉ phép '{command.Code}' đã tồn tại.");
                    }

                    DayOffConfigEntity entity = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        DefaultDaysPerYear = command.DefaultDaysPerYear ?? 0,
                        IsPaid = command.IsPaid ?? true,
                        DeductBalance = command.DeductBalance ?? true,
                        RequireAttachment = command.RequireAttachment ?? false,
                        MaxDaysPerRequest = command.MaxDaysPerRequest,
                        MinNoticeDays = command.MinNoticeDays ?? 0,
                        IsActive = command.IsActive ?? true
                    };
                    DayOffConfigMapper.ApplyCommandFields(entity, command);

                    _ = _context.DayOffConfigEntities.Add(entity);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "DayOffConfigEntity",
                        entity.Id,
                        null,
                        DayOffConfigMapper.ToLogObject(entity),
                        "Import Excel - Tạo mới cấu hình nghỉ phép " + entity.Name);

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

    internal sealed class DayOffConfigExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class DayOffConfigExcelColumns
    {
        public static readonly DayOffConfigExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã cấu hình", Required = true },
            new() { Title = "Tên cấu hình", Required = true },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Mã công ty (Parent)", Required = false },
            new() { Title = "Số ngày phép mặc định/năm", Required = false },
            new() { Title = "Có tính lương?", Required = false },
            new() { Title = "Trừ quỹ phép?", Required = false },
            new() { Title = "Bắt buộc đính kèm?", Required = false },
            new() { Title = "Số ngày tối đa mỗi đơn", Required = false },
            new() { Title = "Số ngày báo trước tối thiểu", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true }
        };

        public static IEnumerable<DayOffConfigExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class DayOffConfigExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<DayOffConfigExcelColumnDefinition> columns = DayOffConfigExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                DayOffConfigExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "NP-NAM",
                "Nghỉ phép năm",
                "Phép thường niên 12 ngày/năm hưởng 100% lương",
                "CT01",
                "12",
                "Có",
                "Có",
                "Không",
                "5",
                "3",
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

        public static void WriteConfigRow(
            IXLWorksheet worksheet,
            int row,
            DayOffConfigEntity config,
            string? companyCode,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                config.Code,
                config.Name,
                config.Description,
                companyCode,
                config.DefaultDaysPerYear.ToString("G29"),
                config.IsPaid ? "Có" : "Không",
                config.DeductBalance ? "Có" : "Không",
                config.RequireAttachment ? "Có" : "Không",
                config.MaxDaysPerRequest?.ToString("G29"),
                config.MinNoticeDays.ToString(),
                config.IsActive ? "Có" : "Không"
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(config.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

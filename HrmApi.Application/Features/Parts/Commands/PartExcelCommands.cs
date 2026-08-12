using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Features.Parts.Commands;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Parts.Commands
{
    public class ExportPartsExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportPartsExcelQueryHandler : IRequestHandler<ExportPartsExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportPartsExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportPartsExcelQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PartEntities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code != null && x.Code.ToLower().Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var name = request.Name.Trim().ToLower();
                query = query.Where(x => x.Name != null && x.Name.ToLower().Contains(name));
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            var parts = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachToNhom");
            PartExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < parts.Count; i++)
            {
                PartExcelWriter.WritePartRow(worksheet, i + 2, parts[i], includeExportOnlyColumns: true);
            }

            PartExcelWriter.ApplyColumnWidths(worksheet);
            PartExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadPartExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadPartExcelTemplateQueryHandler : IRequestHandler<DownloadPartExcelTemplateQuery, byte[]>
    {
        public Task<byte[]> Handle(DownloadPartExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("MauImport");
            PartExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            PartExcelWriter.ApplyColumnWidths(worksheet);
            PartExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }

    public class ImportPartsExcelCommand : IRequest<PartImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class PartImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ImportPartsExcelCommandHandler : IRequestHandler<ImportPartsExcelCommand, PartImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportPartsExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<PartImportResultDto> Handle(ImportPartsExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new PartImportResultDto();

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
                    if (string.IsNullOrWhiteSpace(command.Code)
                        && string.IsNullOrWhiteSpace(command.Name)
                        && !command.PartMasterId.HasValue)
                    {
                        continue;
                    }

                    await CreatePartCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var part = new PartEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    PartMapper.ApplyCommandFields(part, command);

                    _context.PartEntities.Add(part);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "PartEntity",
                        part.Id,
                        null,
                        PartMapper.ToLogObject(part),
                        "Import Excel - Tạo mới tổ/nhóm " + (part.Name ?? part.Code));

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

        private static PartCommandFields ReadRow(IXLRangeRow row)
        {
            return new PartCommandFields
            {
                Code = GetCellString(row, 1),
                Name = GetCellString(row, 2),
                Description = GetCellString(row, 3),
                CompanyId = ParseGuid(row.Cell(4)),
                BranchId = ParseGuid(row.Cell(5)),
                PartMasterId = ParseGuid(row.Cell(6)),
                DepartmentId = ParseGuid(row.Cell(7)),
                ManagerId = ParseGuid(row.Cell(8)),
                Limit = ParseInt(row.Cell(9)),
                IsActive = ParseBool(row.Cell(10)) ?? true,
                DisplayOrder = ParseInt(row.Cell(11)) ?? 0
            };
        }

        private static string? GetCellString(IXLRangeRow row, int column)
        {
            var value = row.Cell(column).GetString().Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static int? ParseInt(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue(out int intValue)) return intValue;
            if (int.TryParse(cell.GetString(), out var parsed)) return parsed;
            return null;
        }

        private static Guid? ParseGuid(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            var text = cell.GetString().Trim();
            return Guid.TryParse(text, out var parsed) ? parsed : null;
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

    internal sealed class PartExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class PartExcelColumns
    {
        public static readonly PartExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã tổ/nhóm", Required = false },
            new() { Title = "Tên tổ/nhóm", Required = false },
            new() { Title = "Ghi chú", Required = false },
            new() { Title = "Id công ty", Required = false },
            new() { Title = "Id chi nhánh", Required = false },
            new() { Title = "Id mẫu tổ/nhóm", Required = true },
            new() { Title = "Id phòng ban", Required = false },
            new() { Title = "Id tổ trưởng", Required = false },
            new() { Title = "Định biên", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Thứ tự hiển thị", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<PartExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns) =>
            Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
    }

    internal static class PartExcelWriter
    {
        private static readonly XLColor RequiredHeaderColor = XLColor.FromHtml("#FFC000");
        private static readonly XLColor OptionalHeaderColor = XLColor.FromHtml("#92D050");

        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = PartExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (var col = 0; col < columns.Count; col++)
            {
                var definition = columns[col];
                var cell = worksheet.Cell(1, col + 1);
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

        public static void WritePartRow(
            IXLWorksheet worksheet,
            int row,
            PartEntity part,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                part.Code,
                part.Name,
                part.Description,
                part.CompanyId?.ToString(),
                part.BranchId?.ToString(),
                part.PartMasterId?.ToString(),
                part.DepartmentId?.ToString(),
                part.ManagerId?.ToString(),
                part.Limit?.ToString(),
                part.IsActive.ToString(),
                part.DisplayOrder.ToString(),
            };

            if (includeExportOnlyColumns)
            {
                values.Add(part.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
            }

            for (var col = 0; col < values.Count; col++)
            {
                var cell = worksheet.Cell(row, col + 1);
                cell.Value = values[col] ?? string.Empty;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }

        public static void ApplyColumnWidths(IXLWorksheet worksheet)
        {
            var usedColumns = worksheet.ColumnsUsed();
            foreach (var column in usedColumns)
            {
                column.AdjustToContents(8, 60);
                column.Width = Math.Max(column.Width + 2, 12);
            }
        }

        public static void FreezeHeaderRow(IXLWorksheet worksheet)
        {
            worksheet.SheetView.FreezeRows(1);
        }
    }
}

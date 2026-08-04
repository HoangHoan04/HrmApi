using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Features.PartMasters.Commands;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PartMasters.Commands
{
    public class ExportPartMastersExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportPartMastersExcelQueryHandler : IRequestHandler<ExportPartMastersExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportPartMastersExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportPartMastersExcelQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PartMasterEntities.AsNoTracking();

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

            var partMasters = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachMauToNhom");
            PartMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < partMasters.Count; i++)
            {
                PartMasterExcelWriter.WritePartMasterRow(worksheet, i + 2, partMasters[i], includeExportOnlyColumns: true);
            }

            PartMasterExcelWriter.ApplyColumnWidths(worksheet);
            PartMasterExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadPartMasterExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadPartMasterExcelTemplateQueryHandler : IRequestHandler<DownloadPartMasterExcelTemplateQuery, byte[]>
    {
        public Task<byte[]> Handle(DownloadPartMasterExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("MauImport");
            PartMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            PartMasterExcelWriter.ApplyColumnWidths(worksheet);
            PartMasterExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }

    public class ImportPartMastersExcelCommand : IRequest<PartMasterImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class PartMasterImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ImportPartMastersExcelCommandHandler : IRequestHandler<ImportPartMastersExcelCommand, PartMasterImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportPartMastersExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<PartMasterImportResultDto> Handle(ImportPartMastersExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new PartMasterImportResultDto();

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

                    await CreatePartMasterCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var partMaster = new PartMasterEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    PartMasterMapper.ApplyCommandFields(partMaster, command);

                    _context.PartMasterEntities.Add(partMaster);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "PartMasterEntity",
                        partMaster.Id,
                        null,
                        PartMasterMapper.ToLogObject(partMaster),
                        "Import Excel - Tạo mới mẫu tổ/nhóm " + partMaster.Name);

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

        private static PartMasterCommandFields ReadRow(IXLRangeRow row)
        {
            return new PartMasterCommandFields
            {
                Code = GetCellString(row, 1),
                Name = GetCellString(row, 2),
                Description = GetCellString(row, 3),
                CompanyId = ParseGuid(row.Cell(4)),
                BranchId = ParseGuid(row.Cell(5)),
                Type = GetCellString(row, 6),
                IsActive = ParseBool(row.Cell(7)) ?? true,
                DisplayOrder = ParseInt(row.Cell(8)) ?? 0
            };
        }

        private static string GetCellString(IXLRangeRow row, int column) =>
            row.Cell(column).GetString().Trim();

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

    internal sealed class PartMasterExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class PartMasterExcelColumns
    {
        public static readonly PartMasterExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã mẫu tổ/nhóm", Required = true },
            new() { Title = "Tên mẫu tổ/nhóm", Required = true },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Id công ty", Required = false },
            new() { Title = "Id chi nhánh", Required = false },
            new() { Title = "Loại tổ/nhóm", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Thứ tự hiển thị", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<PartMasterExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns) =>
            Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
    }

    internal static class PartMasterExcelWriter
    {
        private static readonly XLColor RequiredHeaderColor = XLColor.FromHtml("#FFC000");
        private static readonly XLColor OptionalHeaderColor = XLColor.FromHtml("#92D050");

        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = PartMasterExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

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

        public static void WritePartMasterRow(
            IXLWorksheet worksheet,
            int row,
            PartMasterEntity partMaster,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                partMaster.Code,
                partMaster.Name,
                partMaster.Description,
                partMaster.CompanyId?.ToString(),
                partMaster.BranchId?.ToString(),
                partMaster.Type,
                partMaster.IsActive.ToString(),
                partMaster.DisplayOrder.ToString(),
            };

            if (includeExportOnlyColumns)
            {
                values.Add(partMaster.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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
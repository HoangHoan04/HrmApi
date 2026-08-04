using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Features.Positions.Commands;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Positions.Commands
{
    public class ExportPositionsExcelQuery : IRequest<byte[]>
    {
        public bool? IsDeleted { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? DepartmentId { get; set; }
    }

    public class ExportPositionsExcelQueryHandler : IRequestHandler<ExportPositionsExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportPositionsExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportPositionsExcelQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PositionEntities.AsNoTracking();

            if (request.IsDeleted.HasValue)
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
                query = query.Where(x => x.CompanyId == request.CompanyId);

            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
                query = query.Where(x => x.DepartmentId == request.DepartmentId);

            var items = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachChucDanh");
            PositionExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < items.Count; i++)
                PositionExcelWriter.WriteRow(worksheet, i + 2, items[i], includeExportOnlyColumns: true);

            PositionExcelWriter.ApplyColumnWidths(worksheet);
            PositionExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadPositionExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadPositionExcelTemplateQueryHandler : IRequestHandler<DownloadPositionExcelTemplateQuery, byte[]>
    {
        public Task<byte[]> Handle(DownloadPositionExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("MauImport");
            PositionExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            PositionExcelWriter.ApplyColumnWidths(worksheet);
            PositionExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }

    public class ImportPositionsExcelCommand : IRequest<PositionImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class PositionImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ImportPositionsExcelCommandHandler : IRequestHandler<ImportPositionsExcelCommand, PositionImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportPositionsExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<PositionImportResultDto> Handle(ImportPositionsExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new PositionImportResultDto();

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
                    if (!command.PositionMasterId.HasValue)
                        continue;

                    await CreatePositionCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var entity = new PositionEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    PositionMapper.ApplyCommandFields(entity, command);

                    _context.PositionEntities.Add(entity);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "PositionEntity",
                        entity.Id,
                        null,
                        PositionMapper.ToLogObject(entity),
                        "Import Excel - Tạo mới chức danh thành công");

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

        private static PositionCommandFields ReadRow(IXLRangeRow row) => new()
        {
            PositionMasterId = ParseGuid(row.Cell(1)),
            CompanyId = ParseGuid(row.Cell(2)),
            BranchId = ParseGuid(row.Cell(3)),
            DepartmentId = ParseGuid(row.Cell(4)),
            PartId = ParseGuid(row.Cell(5)),
            QuantityStandard = ParseInt(row.Cell(6)),
            IsActive = ParseBool(row.Cell(7)) ?? true,
            DisplayOrder = ParseInt(row.Cell(8)) ?? 0
        };

        private static int? ParseInt(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue(out int intValue)) return intValue;
            return int.TryParse(cell.GetString(), out var parsed) ? parsed : null;
        }

        private static Guid? ParseGuid(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            return Guid.TryParse(cell.GetString().Trim(), out var parsed) ? parsed : null;
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

    internal static class PositionExcelWriter
    {
        private static readonly XLColor RequiredHeaderColor = XLColor.FromHtml("#FFC000");
        private static readonly XLColor OptionalHeaderColor = XLColor.FromHtml("#92D050");

        private static readonly string[] Headers =
        [
            "Id mẫu chức danh*",
            "Id công ty",
            "Id chi nhánh",
            "Id phòng ban",
            "Id tổ/nhóm",
            "Định biên",
            "Kích hoạt",
            "Thứ tự hiển thị",
            "Trạng thái hệ thống"
        ];

        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var count = includeExportOnlyColumns ? Headers.Length : Headers.Length - 1;
            for (var col = 0; col < count; col++)
            {
                var cell = worksheet.Cell(1, col + 1);
                cell.Value = Headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = col < 1 ? RequiredHeaderColor : OptionalHeaderColor;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            worksheet.Row(1).Height = 28;
        }

        public static void WriteRow(IXLWorksheet worksheet, int row, PositionEntity entity, bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                entity.PositionMasterId?.ToString(),
                entity.CompanyId?.ToString(),
                entity.BranchId?.ToString(),
                entity.DepartmentId?.ToString(),
                entity.PartId?.ToString(),
                entity.QuantityStandard?.ToString(),
                entity.IsActive.ToString(),
                entity.DisplayOrder.ToString()
            };

            if (includeExportOnlyColumns)
                values.Add(entity.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");

            for (var col = 0; col < values.Count; col++)
                worksheet.Cell(row, col + 1).Value = values[col] ?? string.Empty;
        }

        public static void ApplyColumnWidths(IXLWorksheet worksheet)
        {
            foreach (var column in worksheet.ColumnsUsed())
            {
                column.AdjustToContents(8, 60);
                column.Width = Math.Max(column.Width + 2, 12);
            }
        }

        public static void FreezeHeaderRow(IXLWorksheet worksheet) => worksheet.SheetView.FreezeRows(1);
    }
}

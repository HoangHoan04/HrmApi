using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Features.PositionMasters.Commands;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.PositionMasters.Commands
{
    public class ExportPositionMastersExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportPositionMastersExcelQueryHandler : IRequestHandler<ExportPositionMastersExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportPositionMastersExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportPositionMastersExcelQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PositionMasterEntities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Code))
                query = query.Where(x => x.Code.ToLower().Contains(request.Code.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(x => x.Name.ToLower().Contains(request.Name.Trim().ToLower()));

            if (request.IsDeleted.HasValue)
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

            var items = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachMauChucDanh");
            PositionMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < items.Count; i++)
                PositionMasterExcelWriter.WriteRow(worksheet, i + 2, items[i], includeExportOnlyColumns: true);

            PositionMasterExcelWriter.ApplyColumnWidths(worksheet);
            PositionMasterExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadPositionMasterExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadPositionMasterExcelTemplateQueryHandler : IRequestHandler<DownloadPositionMasterExcelTemplateQuery, byte[]>
    {
        public Task<byte[]> Handle(DownloadPositionMasterExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("MauImport");
            PositionMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            PositionMasterExcelWriter.ApplyColumnWidths(worksheet);
            PositionMasterExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }

    public class ImportPositionMastersExcelCommand : IRequest<PositionMasterImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class PositionMasterImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ImportPositionMastersExcelCommandHandler : IRequestHandler<ImportPositionMastersExcelCommand, PositionMasterImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportPositionMastersExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<PositionMasterImportResultDto> Handle(ImportPositionMastersExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new PositionMasterImportResultDto();

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
                        continue;

                    await CreatePositionMasterCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var entity = new PositionMasterEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    PositionMasterMapper.ApplyCommandFields(entity, command);

                    _context.PositionMasterEntities.Add(entity);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "PositionMasterEntity",
                        entity.Id,
                        null,
                        PositionMasterMapper.ToLogObject(entity),
                        "Import Excel - Tạo mới mẫu chức danh " + entity.Name);

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

        private static PositionMasterCommandFields ReadRow(IXLRangeRow row) => new()
        {
            Code = row.Cell(1).GetString().Trim(),
            Name = row.Cell(2).GetString().Trim(),
            Description = row.Cell(3).GetString().Trim(),
            CompanyId = ParseGuid(row.Cell(4)),
            BranchId = ParseGuid(row.Cell(5)),
            WorkingHour = ParseInt(row.Cell(6)),
            IsTimeKeeping = ParseBool(row.Cell(7)) ?? false,
            QuantityStandard = ParseInt(row.Cell(8)),
            IsActive = ParseBool(row.Cell(9)) ?? true,
            DisplayOrder = ParseInt(row.Cell(10)) ?? 0
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

    internal static class PositionMasterExcelWriter
    {
        private static readonly XLColor RequiredHeaderColor = XLColor.FromHtml("#FFC000");
        private static readonly XLColor OptionalHeaderColor = XLColor.FromHtml("#92D050");

        private static readonly string[] Headers =
        [
            "Mã mẫu chức danh*",
            "Tên mẫu chức danh*",
            "Mô tả",
            "Id công ty",
            "Id chi nhánh",
            "Giờ làm chuẩn",
            "Chấm công",
            "Định biên chuẩn",
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
                cell.Style.Fill.BackgroundColor = col < 2 ? RequiredHeaderColor : OptionalHeaderColor;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            worksheet.Row(1).Height = 28;
        }

        public static void WriteRow(IXLWorksheet worksheet, int row, PositionMasterEntity entity, bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                entity.Code,
                entity.Name,
                entity.Description,
                entity.CompanyId?.ToString(),
                entity.BranchId?.ToString(),
                entity.WorkingHour?.ToString(),
                entity.IsTimeKeeping.ToString(),
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

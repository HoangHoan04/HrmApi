using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
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

            var positionMasters = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            var companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var branchDict = await _context.BranchEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachMauChucDanh");
            PositionMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < positionMasters.Count; i++)
            {
                var pm = positionMasters[i];
                string? companyCode = pm.CompanyId.HasValue && companyDict.TryGetValue(pm.CompanyId.Value, out var cc) ? cc : null;
                string? branchCode = pm.BranchId.HasValue && branchDict.TryGetValue(pm.BranchId.Value, out var bc) ? bc : null;

                PositionMasterExcelWriter.WriteRow(worksheet, i + 2, pm, companyCode, branchCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

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
        private readonly IApplicationDbContext _context;

        public DownloadPositionMasterExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadPositionMasterExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("MauChucDanh");
            PositionMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            PositionMasterExcelWriter.WriteTemplateSampleRow(worksheet);
            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            var companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "CongTy", "Mã công ty", "Tên công ty", companies.Select(x => (x.Code, x.Name)));

            var branches = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "ChiNhanh", "Mã chi nhánh", "Tên chi nhánh", branches.Select(x => (x.Code, x.Name)));

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
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

            var companyDict = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var branchDict = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber();
                try
                {
                    var command = ReadRow(row, companyDict, branchDict);
                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("MCD001", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

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

        private static PositionMasterCommandFields ReadRow(
            IXLRangeRow row,
            Dictionary<string, Guid> companyDict,
            Dictionary<string, Guid> branchDict)
        {
            var companyCode = ExcelHelper.GetCellString(row, 4).Trim().ToLower();
            var branchCode = ExcelHelper.GetCellString(row, 5).Trim().ToLower();

            Guid? companyId = !string.IsNullOrWhiteSpace(companyCode) && companyDict.TryGetValue(companyCode, out var cid) ? cid : null;
            Guid? branchId = !string.IsNullOrWhiteSpace(branchCode) && branchDict.TryGetValue(branchCode, out var bid) ? bid : null;

            return new PositionMasterCommandFields
            {
                Code = ExcelHelper.GetCellString(row, 1),
                Name = ExcelHelper.GetCellString(row, 2),
                Description = ExcelHelper.GetCellString(row, 3),
                CompanyId = companyId,
                BranchId = branchId,
                WorkingHour = ExcelHelper.ParseInt(row.Cell(6)),
                IsTimeKeeping = ExcelHelper.ParseBool(row.Cell(7)) ?? true,
                QuantityStandard = ExcelHelper.ParseInt(row.Cell(8)),
                IsActive = ExcelHelper.ParseBool(row.Cell(9)) ?? true,
                DisplayOrder = ExcelHelper.ParseInt(row.Cell(10)) ?? 0
            };
        }
    }

    internal sealed class PositionMasterExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class PositionMasterExcelColumns
    {
        public static readonly PositionMasterExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã mẫu chức danh", Required = true },
            new() { Title = "Tên mẫu chức danh", Required = true },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Mã công ty", Required = false },
            new() { Title = "Mã chi nhánh", Required = false },
            new() { Title = "Giờ làm chuẩn", Required = false },
            new() { Title = "Chấm công", Required = false },
            new() { Title = "Định biên chuẩn", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Thứ tự hiển thị", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<PositionMasterExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns) =>
            Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
    }

    internal static class PositionMasterExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = PositionMasterExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (var col = 0; col < columns.Count; col++)
            {
                var definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            var sampleValues = new List<string>
            {
                "MCD001",
                "Kỹ sư Phần mềm",
                "Mẫu chức danh kỹ sư lập trình phần mềm",
                "CT01",
                "CN01",
                "8",
                "Có",
                "10",
                "Có",
                "1"
            };

            for (var col = 0; col < sampleValues.Count; col++)
            {
                var cell = worksheet.Cell(2, col + 1);
                cell.Value = sampleValues[col];
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.FontColor = XLColor.DarkGray;
            }
        }

        public static void WriteRow(
            IXLWorksheet worksheet,
            int row,
            PositionMasterEntity entity,
            string? companyCode,
            string? branchCode,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                entity.Code,
                entity.Name,
                entity.Description,
                companyCode,
                branchCode,
                entity.WorkingHour?.ToString(),
                entity.IsTimeKeeping ? "Có" : "Không",
                entity.QuantityStandard?.ToString(),
                entity.IsActive ? "Có" : "Không",
                entity.DisplayOrder.ToString()
            };

            if (includeExportOnlyColumns)
            {
                values.Add(entity.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
            }

            for (var col = 0; col < values.Count; col++)
            {
                var cell = worksheet.Cell(row, col + 1);
                cell.Value = values[col] ?? string.Empty;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }
    }
}

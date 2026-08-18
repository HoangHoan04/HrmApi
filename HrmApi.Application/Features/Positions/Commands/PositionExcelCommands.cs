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

namespace HrmApi.Application.Features.Positions.Commands
{
    public class ExportPositionsExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
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
            var query = _context.PositionEntities.AsNoTracking().Include(x => x.PositionMaster).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var code = request.Code.Trim().ToLower();
                query = query.Where(x => x.PositionMaster != null && x.PositionMaster.Code.ToLower().Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var name = request.Name.Trim().ToLower();
                query = query.Where(x => x.PositionMaster != null && x.PositionMaster.Name.ToLower().Contains(name));
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            var positions = await query.OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
            var posMasterDict = await _context.PositionMasterEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var branchDict = await _context.BranchEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var deptDict = await _context.DepartmentEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var partDict = await _context.PartEntities.AsNoTracking()
                .Where(x => x.Code != null)
                .ToDictionaryAsync(x => x.Id, x => x.Code!, cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachChucDanh");
            PositionExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                string? posMasterCode = null;
                if (pos.PositionMasterId.HasValue) posMasterDict.TryGetValue(pos.PositionMasterId.Value, out posMasterCode);

                string? companyCode = null;
                if (pos.CompanyId.HasValue) companyDict.TryGetValue(pos.CompanyId.Value, out companyCode);

                string? branchCode = null;
                if (pos.BranchId.HasValue) branchDict.TryGetValue(pos.BranchId.Value, out branchCode);

                string? deptCode = null;
                if (pos.DepartmentId.HasValue) deptDict.TryGetValue(pos.DepartmentId.Value, out deptCode);

                string? partCode = null;
                if (pos.PartId.HasValue) partDict.TryGetValue(pos.PartId.Value, out partCode);

                PositionExcelWriter.WriteRow(worksheet, i + 2, pos, posMasterCode, companyCode, branchCode, deptCode, partCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

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
        private readonly IApplicationDbContext _context;

        public DownloadPositionExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadPositionExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("ChucDanh");
            PositionExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            PositionExcelWriter.WriteTemplateSampleRow(worksheet);
            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            var posMasters = await _context.PositionMasterEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "MauChucDanh", "Mã mẫu chức danh", "Tên mẫu chức danh", posMasters.Select(x => (x.Code, x.Name)));

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

            var departments = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "PhongBan", "Mã phòng ban", "Tên phòng ban", departments.Select(x => (x.Code, x.Name)));

            var parts = await _context.PartEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.Code))
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "ToNhom", "Mã tổ/nhóm", "Tên tổ/nhóm", parts.Select(x => (x.Code ?? string.Empty, x.Name ?? string.Empty)));

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
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

            var posMasterDict = await _context.PositionMasterEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var companyDict = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var branchDict = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var deptDict = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var partDict = await _context.PartEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.Code))
                .ToDictionaryAsync(x => x.Code!.Trim().ToLower(), x => x.Id, cancellationToken);

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber();
                try
                {
                    var command = ReadRow(row, posMasterDict, companyDict, branchDict, deptDict, partDict);
                    if (command.PositionMasterId == null || command.PositionMasterId == Guid.Empty)
                    {
                        result.TotalRows--;
                        continue;
                    }

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
                        "Import Excel - Tạo mới chức danh");

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

        private static PositionCommandFields ReadRow(
            IXLRangeRow row,
            Dictionary<string, Guid> posMasterDict,
            Dictionary<string, Guid> companyDict,
            Dictionary<string, Guid> branchDict,
            Dictionary<string, Guid> deptDict,
            Dictionary<string, Guid> partDict)
        {
            var posMasterCode = ExcelHelper.GetCellString(row, 1).Trim().ToLower();
            var companyCode = ExcelHelper.GetCellString(row, 2).Trim().ToLower();
            var branchCode = ExcelHelper.GetCellString(row, 3).Trim().ToLower();
            var deptCode = ExcelHelper.GetCellString(row, 4).Trim().ToLower();
            var partCode = ExcelHelper.GetCellString(row, 5).Trim().ToLower();

            Guid? posMasterId = !string.IsNullOrWhiteSpace(posMasterCode) && posMasterDict.TryGetValue(posMasterCode, out var pmid) ? pmid : null;
            Guid? companyId = !string.IsNullOrWhiteSpace(companyCode) && companyDict.TryGetValue(companyCode, out var cid) ? cid : null;
            Guid? branchId = !string.IsNullOrWhiteSpace(branchCode) && branchDict.TryGetValue(branchCode, out var bid) ? bid : null;
            Guid? deptId = !string.IsNullOrWhiteSpace(deptCode) && deptDict.TryGetValue(deptCode, out var did) ? did : null;
            Guid? partId = !string.IsNullOrWhiteSpace(partCode) && partDict.TryGetValue(partCode, out var pid) ? pid : null;

            return new PositionCommandFields
            {
                PositionMasterId = posMasterId,
                CompanyId = companyId,
                BranchId = branchId,
                DepartmentId = deptId,
                PartId = partId,
                QuantityStandard = ExcelHelper.ParseInt(row.Cell(6)),
                IsActive = ExcelHelper.ParseBool(row.Cell(7)) ?? true,
                DisplayOrder = ExcelHelper.ParseInt(row.Cell(8)) ?? 0
            };
        }
    }

    internal sealed class PositionExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class PositionExcelColumns
    {
        public static readonly PositionExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã mẫu chức danh", Required = true },
            new() { Title = "Mã công ty", Required = true },
            new() { Title = "Mã chi nhánh", Required = false },
            new() { Title = "Mã phòng ban", Required = true },
            new() { Title = "Mã tổ/nhóm", Required = false },
            new() { Title = "Định biên", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Thứ tự hiển thị", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<PositionExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns) =>
            Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
    }

    internal static class PositionExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = PositionExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

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
                "CT01",
                "CN01",
                "PB01",
                "BP01",
                "5",
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
            PositionEntity entity,
            string? posMasterCode,
            string? companyCode,
            string? branchCode,
            string? deptCode,
            string? partCode,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                posMasterCode,
                companyCode,
                branchCode,
                deptCode,
                partCode,
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

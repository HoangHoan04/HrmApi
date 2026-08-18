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
            var companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var branchDict = await _context.BranchEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var deptDict = await _context.DepartmentEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var partMasterDict = await _context.PartMasterEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachToNhom");
            PartExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                string? companyCode = part.CompanyId.HasValue && companyDict.TryGetValue(part.CompanyId.Value, out var cc) ? cc : null;
                string? branchCode = part.BranchId.HasValue && branchDict.TryGetValue(part.BranchId.Value, out var bc) ? bc : null;
                string? deptCode = part.DepartmentId.HasValue && deptDict.TryGetValue(part.DepartmentId.Value, out var dc) ? dc : null;
                string? partMasterCode = part.PartMasterId.HasValue && partMasterDict.TryGetValue(part.PartMasterId.Value, out var pmc) ? pmc : null;

                PartExcelWriter.WritePartRow(worksheet, i + 2, part, companyCode, branchCode, deptCode, partMasterCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

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
        private readonly IApplicationDbContext _context;

        public DownloadPartExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadPartExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("ToNhom");
            PartExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            PartExcelWriter.WriteTemplateSampleRow(worksheet);
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

            var departments = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "PhongBan", "Mã phòng ban", "Tên phòng ban", departments.Select(x => (x.Code, x.Name)));

            var partMasters = await _context.PartMasterEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "MauToNhom", "Mã mẫu tổ/nhóm", "Tên mẫu tổ/nhóm", partMasters.Select(x => (x.Code, x.Name)));

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
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

            var companyDict = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var branchDict = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var deptDict = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var partMasterDict = await _context.PartMasterEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber();
                try
                {
                    var command = ReadRow(row, companyDict, branchDict, deptDict, partMasterDict);
                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name) && command.PartMasterId == Guid.Empty)
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code != null && command.Code.Equals("BP001", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
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
                        "Import Excel - Tạo mới tổ/nhóm " + part.Name);

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

        private static PartCommandFields ReadRow(
            IXLRangeRow row,
            Dictionary<string, Guid> companyDict,
            Dictionary<string, Guid> branchDict,
            Dictionary<string, Guid> deptDict,
            Dictionary<string, Guid> partMasterDict)
        {
            var companyCode = ExcelHelper.GetCellString(row, 4).Trim().ToLower();
            var branchCode = ExcelHelper.GetCellString(row, 5).Trim().ToLower();
            var deptCode = ExcelHelper.GetCellString(row, 6).Trim().ToLower();
            var partMasterCode = ExcelHelper.GetCellString(row, 7).Trim().ToLower();

            Guid? companyId = !string.IsNullOrWhiteSpace(companyCode) && companyDict.TryGetValue(companyCode, out var cid) ? cid : null;
            Guid? branchId = !string.IsNullOrWhiteSpace(branchCode) && branchDict.TryGetValue(branchCode, out var bid) ? bid : null;
            Guid? deptId = !string.IsNullOrWhiteSpace(deptCode) && deptDict.TryGetValue(deptCode, out var did) ? did : null;
            Guid partMasterId = !string.IsNullOrWhiteSpace(partMasterCode) && partMasterDict.TryGetValue(partMasterCode, out var pmid) ? pmid : Guid.Empty;

            return new PartCommandFields
            {
                Code = ExcelHelper.GetCellString(row, 1),
                Name = ExcelHelper.GetCellString(row, 2),
                Description = ExcelHelper.GetCellString(row, 3),
                CompanyId = companyId,
                BranchId = branchId,
                DepartmentId = deptId,
                PartMasterId = partMasterId,
                Limit = ExcelHelper.ParseInt(row.Cell(8)),
                IsActive = ExcelHelper.ParseBool(row.Cell(9)) ?? true,
                DisplayOrder = ExcelHelper.ParseInt(row.Cell(10)) ?? 0
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
            new() { Title = "Mã công ty", Required = true },
            new() { Title = "Mã chi nhánh", Required = false },
            new() { Title = "Mã phòng ban", Required = true },
            new() { Title = "Mã mẫu tổ/nhóm", Required = true },
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
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = PartExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

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
                "BP001",
                "Tổ Web Frontend",
                "Nhóm phụ trách giao diện web",
                "CT01",
                "CN01",
                "PB01",
                "MTN001",
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

        public static void WritePartRow(
            IXLWorksheet worksheet,
            int row,
            PartEntity part,
            string? companyCode,
            string? branchCode,
            string? deptCode,
            string? partMasterCode,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                part.Code,
                part.Name,
                part.Description,
                companyCode,
                branchCode,
                deptCode,
                partMasterCode,
                part.Limit?.ToString(),
                part.IsActive ? "Có" : "Không",
                part.DisplayOrder.ToString()
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
    }
}

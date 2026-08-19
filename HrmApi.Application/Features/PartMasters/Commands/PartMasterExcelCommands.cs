using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
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
            IQueryable<PartMasterEntity> query = _context.PartMasterEntities.AsNoTracking();

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

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            List<PartMasterEntity> partMasters = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            Dictionary<Guid, string> companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            Dictionary<Guid, string> branchDict = await _context.BranchEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachMauToNhom");
            PartMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < partMasters.Count; i++)
            {
                PartMasterEntity pm = partMasters[i];
                string? companyCode = pm.CompanyId.HasValue && companyDict.TryGetValue(pm.CompanyId.Value, out string? cc) ? cc : null;
                string? branchCode = pm.BranchId.HasValue && branchDict.TryGetValue(pm.BranchId.Value, out string? bc) ? bc : null;

                PartMasterExcelWriter.WritePartMasterRow(worksheet, i + 2, pm, companyCode, branchCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

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
        private readonly IApplicationDbContext _context;

        public DownloadPartMasterExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadPartMasterExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("MauToNhom");
            PartMasterExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            PartMasterExcelWriter.WriteTemplateSampleRow(worksheet);
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

    public class ImportPartMastersExcelCommand : IRequest<PartMasterImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class PartMasterImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
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
            IXLWorksheet worksheet = workbook.Worksheet(1);
            List<IXLRangeRow> rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? [];

            result.TotalRows = rows.Count;

            Dictionary<string, Guid> companyDict = await _context.CompanyEntities.AsNoTracking()
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
                    PartMasterCommandFields command = ReadRow(row, companyDict, branchDict);
                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("MTN001", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    await CreatePartMasterCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var partMaster = new PartMasterEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    PartMasterMapper.ApplyCommandFields(partMaster, command);

                    _ = _context.PartMasterEntities.Add(partMaster);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "PartMasterEntity",
                        partMaster.Id,
                        null,
                        PartMasterMapper.ToLogObject(partMaster),
                        "Import Excel - Tạo mới mẫu bộ phận " + partMaster.Name);

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

        private static PartMasterCommandFields ReadRow(
            IXLRangeRow row,
            Dictionary<string, Guid> companyDict,
            Dictionary<string, Guid> branchDict)
        {
            string companyCode = ExcelHelper.GetCellString(row, 4).Trim().ToLower();
            string branchCode = ExcelHelper.GetCellString(row, 5).Trim().ToLower();

            Guid? companyId = !string.IsNullOrWhiteSpace(companyCode) && companyDict.TryGetValue(companyCode, out Guid cid) ? cid : null;
            Guid? branchId = !string.IsNullOrWhiteSpace(branchCode) && branchDict.TryGetValue(branchCode, out Guid bid) ? bid : null;

            return new PartMasterCommandFields
            {
                Code = ExcelHelper.GetCellString(row, 1),
                Name = ExcelHelper.GetCellString(row, 2),
                Description = ExcelHelper.GetCellString(row, 3),
                CompanyId = companyId,
                BranchId = branchId,
                Type = ExcelHelper.GetCellString(row, 6),
                IsActive = ExcelHelper.ParseBool(row.Cell(7)) ?? true,
                DisplayOrder = ExcelHelper.ParseInt(row.Cell(8)) ?? 0
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
            new() { Title = "Mã mẫu bộ phận", Required = true },
            new() { Title = "Tên mẫu bộ phận", Required = true },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Mã công ty", Required = false },
            new() { Title = "Mã chi nhánh", Required = false },
            new() { Title = "Loại bộ phận", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Thứ tự hiển thị", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<PartMasterExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class PartMasterExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = PartMasterExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                PartMasterExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            var sampleValues = new List<string>
            {
                "MTN001",
                "Tổ Frontend",
                "Mẫu tổ lập trình Frontend",
                "CT01",
                "CN01",
                "Kỹ thuật",
                "Có",
                "1"
            };

            for (int col = 0; col < sampleValues.Count; col++)
            {
                IXLCell cell = worksheet.Cell(2, col + 1);
                cell.Value = sampleValues[col];
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.FontColor = XLColor.DarkGray;
            }
        }

        public static void WritePartMasterRow(
            IXLWorksheet worksheet,
            int row,
            PartMasterEntity partMaster,
            string? companyCode,
            string? branchCode,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                partMaster.Code,
                partMaster.Name,
                partMaster.Description,
                companyCode,
                branchCode,
                partMaster.Type,
                partMaster.IsActive ? "Có" : "Không",
                partMaster.DisplayOrder.ToString()
            };

            if (includeExportOnlyColumns)
            {
                values.Add(partMaster.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

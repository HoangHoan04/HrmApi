using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Asset;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Asset.Commands
{
    public class ExportAssetTypesExcelQuery : IRequest<byte[]>
    {
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
    }

    public class ExportAssetTypesExcelQueryHandler : IRequestHandler<ExportAssetTypesExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportAssetTypesExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportAssetTypesExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AssetTypeEntity> query = _context.AssetTypeEntities.AsNoTracking()
                .Include(x => x.Company)
                .Where(x => !x.IsDeleted);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s) || x.Name.ToLower().Contains(s));
            }

            List<AssetTypeEntity> types = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            Dictionary<Guid, string> companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachLoaiTaiSan");
            AssetTypeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < types.Count; i++)
            {
                AssetTypeEntity type = types[i];
                string? companyCode = type.CompanyId.HasValue && companyDict.TryGetValue(type.CompanyId.Value, out string? cc) ? cc : null;
                AssetTypeExcelWriter.WriteTypeRow(worksheet, i + 2, type, companyCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadAssetTypeExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadAssetTypeExcelTemplateQueryHandler : IRequestHandler<DownloadAssetTypeExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadAssetTypeExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadAssetTypeExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("LoaiTaiSan");
            AssetTypeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            AssetTypeExcelWriter.WriteTemplateSampleRow(worksheet);
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

    public class ImportAssetTypesExcelCommand : IRequest<AssetTypeImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class AssetTypeImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportAssetTypesExcelCommandHandler : IRequestHandler<ImportAssetTypesExcelCommand, AssetTypeImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportAssetTypesExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<AssetTypeImportResultDto> Handle(ImportAssetTypesExcelCommand request, CancellationToken cancellationToken)
        {
            AssetTypeImportResultDto result = new();

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
                    string code = ExcelHelper.GetCellString(row, 1).Trim();
                    string name = ExcelHelper.GetCellString(row, 2).Trim();
                    string rawCompanyCode = ExcelHelper.GetCellString(row, 3).Trim();

                    if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (code.Equals("LTS-LAPTOP", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(code))
                    {
                        throw new InvalidOperationException("Mã loại tài sản là bắt buộc.");
                    }

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        throw new InvalidOperationException("Tên loại tài sản là bắt buộc.");
                    }

                    Guid? companyId = null;
                    if (!string.IsNullOrWhiteSpace(rawCompanyCode))
                    {
                        if (!companyDict.TryGetValue(rawCompanyCode.ToLower(), out Guid matchedCompanyId))
                        {
                            throw new InvalidOperationException($"Mã công ty '{rawCompanyCode}' không tồn tại hoặc đã bị ngừng hoạt động.");
                        }

                        companyId = matchedCompanyId;
                    }

                    bool exists = await _context.AssetTypeEntities.AnyAsync(
                        x => !x.IsDeleted && x.Code.ToLower() == code.ToLower() && x.CompanyId == companyId,
                        cancellationToken);
                    if (exists)
                    {
                        throw new InvalidOperationException($"Mã loại tài sản '{code}' đã tồn tại trong công ty.");
                    }

                    AssetTypeEntity entity = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        Code = code.ToUpperInvariant(),
                        Name = name,
                        CompanyId = companyId,
                        Description = ExcelHelper.GetCellString(row, 4),
                        IsSerialRequired = ExcelHelper.ParseBool(row.Cell(5)) ?? true,
                        MaxPerEmployee = ExcelHelper.ParseInt(row.Cell(6)),
                        IsActive = ExcelHelper.ParseBool(row.Cell(7)) ?? true
                    };

                    _ = _context.AssetTypeEntities.Add(entity);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "AssetTypeEntity",
                        entity.Id,
                        null,
                        new { entity.Id, entity.Code, entity.Name },
                        "Import Excel - Tạo mới loại tài sản " + entity.Name);

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

    internal sealed class AssetTypeExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class AssetTypeExcelColumns
    {
        public static readonly AssetTypeExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã loại tài sản", Required = true },
            new() { Title = "Tên loại tài sản", Required = true },
            new() { Title = "Mã công ty (Parent)", Required = false },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Bắt buộc Serial?", Required = false },
            new() { Title = "Tối đa / nhân viên", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true }
        };

        public static IEnumerable<AssetTypeExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class AssetTypeExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<AssetTypeExcelColumnDefinition> columns = AssetTypeExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                AssetTypeExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "LTS-LAPTOP",
                "Laptop & Máy tính xách tay",
                "CT01",
                "Thiết bị làm việc cá nhân của nhân viên",
                "Có",
                "1",
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

        public static void WriteTypeRow(
            IXLWorksheet worksheet,
            int row,
            AssetTypeEntity type,
            string? companyCode,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                type.Code,
                type.Name,
                companyCode,
                type.Description,
                type.IsSerialRequired ? "Có" : "Không",
                type.MaxPerEmployee?.ToString() ?? string.Empty,
                type.IsActive ? "Có" : "Không"
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(type.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

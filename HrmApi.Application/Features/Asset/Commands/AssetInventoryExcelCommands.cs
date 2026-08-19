using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Asset;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Asset.Commands
{
    public class ExportAssetsExcelQuery : IRequest<byte[]>
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? AssetTypeId { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
    }

    public class ExportAssetsExcelQueryHandler : IRequestHandler<ExportAssetsExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportAssetsExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportAssetsExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AssetEntity> query = _context.AssetEntities.AsNoTracking()
                .Include(x => x.AssetType)
                .Include(x => x.Company)
                .Include(x => x.Branch)
                .Where(x => !x.IsDeleted);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.BranchId.HasValue && request.BranchId != Guid.Empty)
            {
                query = query.Where(x => x.BranchId == request.BranchId);
            }

            if (request.AssetTypeId.HasValue && request.AssetTypeId != Guid.Empty)
            {
                query = query.Where(x => x.AssetTypeId == request.AssetTypeId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s)
                    || x.Name.ToLower().Contains(s)
                    || (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(s)));
            }

            List<AssetEntity> assets = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            List<Guid> assetIds = assets.Select(x => x.Id).ToList();

            Dictionary<Guid, string> activeAssignments = await _context.AssetAssignmentEntities.AsNoTracking()
                .Include(x => x.Employee)
                .Where(x => !x.IsDeleted && assetIds.Contains(x.AssetId) && !x.ReturnedAt.HasValue)
                .ToDictionaryAsync(x => x.AssetId, x => x.Employee != null ? $"{x.Employee.Code} - {x.Employee.FullName ?? (x.Employee.LastName + " " + x.Employee.FirstName).Trim()}" : x.EmployeeId.ToString(), cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachTaiSan");
            AssetExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < assets.Count; i++)
            {
                AssetEntity asset = assets[i];
                string? currentHolder = activeAssignments.GetValueOrDefault(asset.Id);
                AssetExcelWriter.WriteAssetRow(worksheet, i + 2, asset, currentHolder, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadAssetExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadAssetExcelTemplateQueryHandler : IRequestHandler<DownloadAssetExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadAssetExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadAssetExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("TaiSan");
            AssetExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            AssetExcelWriter.WriteTemplateSampleRow(worksheet);
            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            var types = await _context.AssetTypeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "LoaiTaiSan", "Mã loại", "Tên loại", types.Select(x => (x.Code, x.Name)));

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

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class ImportAssetsExcelCommand : IRequest<AssetImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class AssetImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportAssetsExcelCommandHandler : IRequestHandler<ImportAssetsExcelCommand, AssetImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportAssetsExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<AssetImportResultDto> Handle(ImportAssetsExcelCommand request, CancellationToken cancellationToken)
        {
            AssetImportResultDto result = new();

            using MemoryStream stream = new(request.FileContent);
            using XLWorkbook workbook = new(stream);
            IXLWorksheet worksheet = workbook.Worksheet(1);
            List<IXLRangeRow> rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? [];

            result.TotalRows = rows.Count;

            Dictionary<string, Guid> typeDict = await _context.AssetTypeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

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
                    string code = ExcelHelper.GetCellString(row, 1).Trim();
                    string name = ExcelHelper.GetCellString(row, 2).Trim();
                    string typeCode = ExcelHelper.GetCellString(row, 3).Trim();
                    string companyCode = ExcelHelper.GetCellString(row, 4).Trim();

                    if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (code.Equals("TS-LAPTOP01", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(code))
                    {
                        throw new InvalidOperationException("Mã tài sản là bắt buộc.");
                    }

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        throw new InvalidOperationException("Tên tài sản là bắt buộc.");
                    }

                    if (string.IsNullOrWhiteSpace(typeCode) || !typeDict.TryGetValue(typeCode.ToLower(), out Guid typeId))
                    {
                        throw new InvalidOperationException($"Mã loại tài sản '{typeCode}' không tồn tại.");
                    }

                    if (string.IsNullOrWhiteSpace(companyCode) || !companyDict.TryGetValue(companyCode.ToLower(), out Guid companyId))
                    {
                        throw new InvalidOperationException($"Mã công ty '{companyCode}' không tồn tại.");
                    }

                    string branchCode = ExcelHelper.GetCellString(row, 5).Trim();
                    Guid? branchId = null;
                    if (!string.IsNullOrWhiteSpace(branchCode))
                    {
                        if (!branchDict.TryGetValue(branchCode.ToLower(), out Guid matchedBranchId))
                        {
                            throw new InvalidOperationException($"Mã chi nhánh '{branchCode}' không tồn tại.");
                        }

                        branchId = matchedBranchId;
                    }

                    string serialNumber = ExcelHelper.GetCellString(row, 6).Trim();
                    bool codeExists = await _context.AssetEntities.AnyAsync(
                        x => !x.IsDeleted && x.Code.ToLower() == code.ToLower() && x.CompanyId == companyId,
                        cancellationToken);
                    if (codeExists)
                    {
                        throw new InvalidOperationException($"Mã tài sản '{code}' đã tồn tại trong công ty.");
                    }

                    if (!string.IsNullOrWhiteSpace(serialNumber))
                    {
                        bool serialExists = await _context.AssetEntities.AnyAsync(
                            x => !x.IsDeleted && x.SerialNumber != null && x.SerialNumber.ToLower() == serialNumber.ToLower() && x.CompanyId == companyId,
                            cancellationToken);
                        if (serialExists)
                        {
                            throw new InvalidOperationException($"Số Serial '{serialNumber}' đã tồn tại trong công ty.");
                        }
                    }

                    DateOnly? purchaseDate = ExcelHelper.ParseDateOnly(row.Cell(7));
                    decimal? purchaseCost = ExcelHelper.ParseDecimal(row.Cell(8));
                    DateOnly? warrantyExpiryDate = ExcelHelper.ParseDateOnly(row.Cell(9));

                    string rawStatus = ExcelHelper.GetCellString(row, 13).Trim().ToUpperInvariant();
                    string status = rawStatus switch
                    {
                        "ASSIGNED" => AssetStatus.Assigned,
                        "MAINTENANCE" => AssetStatus.Maintenance,
                        "RETIRED" => AssetStatus.Retired,
                        "LOST" => AssetStatus.Lost,
                        "DISPOSED" => AssetStatus.Disposed,
                        _ => AssetStatus.Available
                    };

                    AssetEntity entity = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        Code = code.ToUpperInvariant(),
                        Name = name,
                        AssetTypeId = typeId,
                        CompanyId = companyId,
                        BranchId = branchId,
                        SerialNumber = string.IsNullOrWhiteSpace(serialNumber) ? null : serialNumber,
                        PurchaseDate = purchaseDate,
                        PurchaseCost = purchaseCost,
                        WarrantyExpiryDate = warrantyExpiryDate,
                        Vendor = ExcelHelper.GetCellString(row, 10),
                        Model = ExcelHelper.GetCellString(row, 11),
                        Location = ExcelHelper.GetCellString(row, 12),
                        Status = status,
                        Note = ExcelHelper.GetCellString(row, 14)
                    };

                    _ = _context.AssetEntities.Add(entity);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "AssetEntity",
                        entity.Id,
                        null,
                        new { entity.Id, entity.Code, entity.Name },
                        "Import Excel - Tạo mới tài sản " + entity.Name);

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

    internal sealed class AssetExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class AssetExcelColumns
    {
        public static readonly AssetExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã tài sản", Required = true },
            new() { Title = "Tên tài sản", Required = true },
            new() { Title = "Mã loại tài sản", Required = true },
            new() { Title = "Mã công ty (Parent)", Required = true },
            new() { Title = "Mã chi nhánh", Required = false },
            new() { Title = "Số Serial", Required = false },
            new() { Title = "Ngày mua (dd/MM/yyyy)", Required = false },
            new() { Title = "Giá mua", Required = false },
            new() { Title = "Hạn bảo hành (dd/MM/yyyy)", Required = false },
            new() { Title = "Nhà cung cấp", Required = false },
            new() { Title = "Model / Thông số", Required = false },
            new() { Title = "Vị trí / Kho", Required = false },
            new() { Title = "Trạng thái", Required = false },
            new() { Title = "Ghi chú", Required = false },
            new() { Title = "Người đang giữ", Required = false, ExportOnly = true },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true }
        };

        public static IEnumerable<AssetExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class AssetExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<AssetExcelColumnDefinition> columns = AssetExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                AssetExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "TS-LAPTOP01",
                "Laptop Dell Latitude 5420",
                "LTS-LAPTOP",
                "CT01",
                "CN01",
                "DLLAT5420X99",
                "15/01/2026",
                "22000000",
                "15/01/2028",
                "Dell Official Store",
                "Core i5 11th, RAM 16GB, SSD 512GB",
                "Phòng IT - Tầng 3",
                "AVAILABLE",
                "Thiết bị văn phòng tiêu chuẩn"
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

        public static void WriteAssetRow(
            IXLWorksheet worksheet,
            int row,
            AssetEntity asset,
            string? currentHolder,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                asset.Code,
                asset.Name,
                asset.AssetType?.Code ?? asset.AssetTypeId.ToString(),
                asset.Company?.Code ?? asset.CompanyId.ToString(),
                asset.Branch?.Code ?? string.Empty,
                asset.SerialNumber,
                asset.PurchaseDate?.ToString("dd/MM/yyyy") ?? string.Empty,
                asset.PurchaseCost?.ToString("G") ?? string.Empty,
                asset.WarrantyExpiryDate?.ToString("dd/MM/yyyy") ?? string.Empty,
                asset.Vendor,
                asset.Model,
                asset.Location,
                asset.Status,
                asset.Note
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(currentHolder ?? "Chưa cấp phát");
                values.Add(asset.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

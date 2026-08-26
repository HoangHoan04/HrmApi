
using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Branches.Commands
{
    public class ExportBranchesExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportBranchesExcelQueryHandler : IRequestHandler<ExportBranchesExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;
        public ExportBranchesExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportBranchesExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<BranchEntity> query = _context.BranchEntities.AsNoTracking();

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

            List<BranchEntity> branches = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            Dictionary<Guid, string> companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachChiNhanh");
            BranchExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < branches.Count; i++)
            {
                BranchEntity branch = branches[i];
                string? companyCode = branch.CompanyId.HasValue && companyDict.TryGetValue(branch.CompanyId.Value, out string? cc) ? cc : null;
                BranchExcelWriter.WriteBranchRow(worksheet, i + 2, branch, companyCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadBranchExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadBranchExcelTemplateQueryHandler : IRequestHandler<DownloadBranchExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadBranchExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadBranchExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("ChiNhanh");
            BranchExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            BranchExcelWriter.WriteTemplateSampleRow(worksheet);
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

    public class ImportBranchesExcelCommand : IRequest<BranchImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class BranchImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportBranchesExcelCommandHandler : IRequestHandler<ImportBranchesExcelCommand, BranchImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportBranchesExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<BranchImportResultDto> Handle(ImportBranchesExcelCommand request, CancellationToken cancellationToken)
        {
            BranchImportResultDto result = new();

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
                    BranchCommandFields command = ReadRow(row, companyDict);
                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("CN001", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    await CreateBranchCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    BranchEntity branch = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    BranchMapper.ApplyCommandFields(branch, command);

                    _ = _context.BranchEntities.Add(branch);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "BranchEntity",
                        branch.Id,
                        null,
                        BranchMapper.ToLogObject(branch),
                        "Import Excel - Tạo mới chi nhánh " + branch.Name);

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

        private static BranchCommandFields ReadRow(IXLRangeRow row, Dictionary<string, Guid> companyDict)
        {
            string companyCode = ExcelHelper.GetCellString(row, 31).Trim().ToLower();
            Guid? companyId = !string.IsNullOrWhiteSpace(companyCode) && companyDict.TryGetValue(companyCode, out Guid cid) ? cid : null;

            return new BranchCommandFields
            {
                Code = ExcelHelper.GetCellString(row, 1),
                Name = ExcelHelper.GetCellString(row, 2),
                ShortName = ExcelHelper.GetCellString(row, 3),
                Description = ExcelHelper.GetCellString(row, 4),
                Type = ExcelHelper.GetCellString(row, 5),
                IsHeadQuarter = ExcelHelper.ParseBool(row.Cell(6)) ?? false,
                Address = ExcelHelper.GetCellString(row, 7),
                Country = ExcelHelper.GetCellString(row, 8),
                City = ExcelHelper.GetCellString(row, 9),
                Ward = ExcelHelper.GetCellString(row, 10),
                Latitude = double.TryParse(ExcelHelper.GetCellString(row, 11), out double lat) ? lat : null,
                Longitude = double.TryParse(ExcelHelper.GetCellString(row, 12), out double lng) ? lng : null,
                PhoneNumber = ExcelHelper.GetCellString(row, 13),
                Email = ExcelHelper.GetCellString(row, 14),
                Fax = ExcelHelper.GetCellString(row, 15),
                IpAddress = ExcelHelper.GetCellString(row, 16),
                ManagerName = ExcelHelper.GetCellString(row, 17),
                ManagerPhone = ExcelHelper.GetCellString(row, 18),
                TaxCode = ExcelHelper.GetCellString(row, 19),
                BusinessRegistrationCode = ExcelHelper.GetCellString(row, 20),
                OpeningDate = ExcelHelper.ParseDate(row.Cell(21)),
                ClosingDate = ExcelHelper.ParseDate(row.Cell(22)),
                OperatingStatus = ExcelHelper.GetCellString(row, 23),
                IsActive = ExcelHelper.ParseBool(row.Cell(24)) ?? true,
                IsUsingHrm = ExcelHelper.ParseBool(row.Cell(25)) ?? true,
                DisplayOrder = ExcelHelper.ParseInt(row.Cell(26)) ?? 0,
                GroupSalary = ExcelHelper.GetCellString(row, 27),
                MaxEmployeeCapacity = ExcelHelper.ParseInt(row.Cell(28)),
                TimeZone = ExcelHelper.GetCellString(row, 29),
                CompanyId = companyId
            };
        }
    }

    internal sealed class BranchExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class BranchExcelColumns
    {
        public static readonly BranchExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã chi nhánh", Required = true },
            new() { Title = "Tên chi nhánh", Required = true },
            new() { Title = "Tên viết tắt", Required = false },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Loại chi nhánh", Required = false },
            new() { Title = "Trụ sở chính?", Required = false },
            new() { Title = "Địa chỉ", Required = false },
            new() { Title = "Quốc gia", Required = false },
            new() { Title = "Thành phố/Tỉnh", Required = false },
            new() { Title = "Phường/Xã", Required = false },
            new() { Title = "Vĩ độ", Required = false },
            new() { Title = "Kinh độ", Required = false },
            new() { Title = "Số điện thoại", Required = false },
            new() { Title = "Email", Required = false },
            new() { Title = "Fax", Required = false },
            new() { Title = "Địa chỉ IP", Required = false },
            new() { Title = "Tên người quản lý", Required = false },
            new() { Title = "SĐT người quản lý", Required = false },
            new() { Title = "Mã số thuế", Required = false },
            new() { Title = "Mã ĐKKD", Required = false },
            new() { Title = "Ngày mở cửa", Required = false },
            new() { Title = "Ngày đóng cửa", Required = false },
            new() { Title = "Trạng thái hoạt động", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Sử dụng HRM", Required = false },
            new() { Title = "Thứ tự hiển thị", Required = false },
            new() { Title = "Nhóm tính lương", Required = false },
            new() { Title = "Sức chứa tối đa", Required = false },
            new() { Title = "Múi giờ", Required = false },
            new() { Title = "Mã công ty", Required = true },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<BranchExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class BranchExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<BranchExcelColumnDefinition> columns = BranchExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                BranchExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "CN001",
                "Chi nhánh Hà Nội",
                "CNHN",
                "Chi nhánh chính khu vực phía Bắc",
                "Văn phòng",
                "Có",
                "Số 1 Thái Hà, Đống Đa, Hà Nội",
                "Việt Nam",
                "Hà Nội",
                "Trung Liệt",
                "21.0123",
                "105.8234",
                "0241234567",
                "chinhanhhn@company.com",
                "",
                "192.168.1.1",
                "Nguyễn Văn B",
                "0987654321",
                "0100000001-001",
                "0100000001",
                "01/01/2020",
                "",
                "Hoạt động",
                "Có",
                "Có",
                "1",
                "Nhóm 1",
                "100",
                "SE Asia Standard Time",
                "CT01"
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

        public static void WriteBranchRow(
            IXLWorksheet worksheet,
            int row,
            BranchEntity branch,
            string? companyCode,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                branch.Code,
                branch.Name,
                branch.ShortName,
                branch.Description,
                branch.Type,
                branch.IsHeadQuarter ? "Có" : "Không",
                branch.Address,
                branch.Country,
                branch.City,
                branch.Ward,
                branch.Latitude?.ToString(),
                branch.Longitude?.ToString(),
                branch.PhoneNumber,
                branch.Email,
                branch.Fax,
                branch.IpAddress,
                branch.ManagerName,
                branch.ManagerPhone,
                branch.TaxCode,
                branch.BusinessRegistrationCode,
                branch.OpeningDate?.ToString("yyyy-MM-dd"),
                branch.ClosingDate?.ToString("yyyy-MM-dd"),
                branch.OperatingStatus,
                branch.IsActive ? "Có" : "Không",
                branch.IsUsingHrm ? "Có" : "Không",
                branch.DisplayOrder.ToString(),
                branch.GroupSalary,
                branch.MaxEmployeeCapacity?.ToString(),
                branch.TimeZone,
                companyCode
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(branch.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

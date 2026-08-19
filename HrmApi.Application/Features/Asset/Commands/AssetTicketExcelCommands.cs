using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Asset;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Asset.Commands
{
    public class ExportAssetTicketsExcelQuery : IRequest<byte[]>
    {
        public Guid? CompanyId { get; set; }
        public Guid? AssetId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? TicketType { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
    }

    public class ExportAssetTicketsExcelQueryHandler : IRequestHandler<ExportAssetTicketsExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportAssetTicketsExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportAssetTicketsExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AssetTicketEntity> query = _context.AssetTicketEntities.AsNoTracking()
                .Include(x => x.Asset)
                .Include(x => x.Employee)
                .Include(x => x.ToEmployee)
                .Include(x => x.Company)
                .Where(x => !x.IsDeleted);

            if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId);
            }

            if (request.AssetId.HasValue && request.AssetId != Guid.Empty)
            {
                query = query.Where(x => x.AssetId == request.AssetId);
            }

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId);
            }

            if (!string.IsNullOrWhiteSpace(request.TicketType))
            {
                query = query.Where(x => x.TicketType == request.TicketType.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(s)
                    || (x.Note != null && x.Note.ToLower().Contains(s)));
            }

            List<AssetTicketEntity> tickets = await query.OrderByDescending(x => x.TicketAt).ToListAsync(cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachPhieuTaiSan");
            AssetTicketExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < tickets.Count; i++)
            {
                AssetTicketEntity ticket = tickets[i];
                AssetTicketExcelWriter.WriteTicketRow(worksheet, i + 2, ticket, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadAssetTicketExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadAssetTicketExcelTemplateQueryHandler : IRequestHandler<DownloadAssetTicketExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadAssetTicketExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadAssetTicketExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("PhieuTaiSan");
            AssetTicketExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            AssetTicketExcelWriter.WriteTemplateSampleRow(worksheet);
            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            var assets = await _context.AssetEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name, x.Status })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "TaiSan", "Mã tài sản", "Tên tài sản (Trạng thái)", assets.Select(x => (x.Code, $"{x.Name} [{x.Status}]")));

            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, Name = $"{x.LastName} {x.FirstName}" })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "NhanVien", "Mã nhân viên", "Họ và tên", employees.Select(x => (x.Code, x.Name)));

            var companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "CongTy", "Mã công ty", "Tên công ty", companies.Select(x => (x.Code, x.Name)));

            List<(string Code, string Name)> ticketTypes =
            [
                ("ISSUE", "Cấp phát / Bàn giao"),
                ("RETURN", "Thu hồi tài sản"),
                ("REPAIR", "Báo hỏng / Đi bảo trì"),
                ("TRANSFER", "Điều chuyển nhân viên khác")
            ];
            ExcelHelper.WriteReferenceSheet(workbook, "LoaiPhieu", "Mã loại phiếu", "Ý nghĩa", ticketTypes);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class ImportAssetTicketsExcelCommand : IRequest<AssetTicketImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class AssetTicketImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportAssetTicketsExcelCommandHandler : IRequestHandler<ImportAssetTicketsExcelCommand, AssetTicketImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportAssetTicketsExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<AssetTicketImportResultDto> Handle(ImportAssetTicketsExcelCommand request, CancellationToken cancellationToken)
        {
            AssetTicketImportResultDto result = new();

            using MemoryStream stream = new(request.FileContent);
            using XLWorkbook workbook = new(stream);
            IXLWorksheet worksheet = workbook.Worksheet(1);
            List<IXLRangeRow> rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? [];

            result.TotalRows = rows.Count;

            Dictionary<string, Guid> assetDict = await _context.AssetEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            Dictionary<string, Guid> empDict = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            Dictionary<string, Guid> companyDict = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            foreach (IXLRangeRow? row in rows)
            {
                int rowNumber = row.RowNumber();
                try
                {
                    string code = ExcelHelper.GetCellString(row, 1).Trim();
                    string assetCode = ExcelHelper.GetCellString(row, 2).Trim();
                    string empCode = ExcelHelper.GetCellString(row, 3).Trim();
                    string companyCode = ExcelHelper.GetCellString(row, 4).Trim();

                    if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(assetCode))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (code.Equals("PH-BG001", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(code))
                    {
                        throw new InvalidOperationException("Mã phiếu là bắt buộc.");
                    }

                    if (string.IsNullOrWhiteSpace(assetCode) || !assetDict.TryGetValue(assetCode.ToLower(), out Guid assetId))
                    {
                        throw new InvalidOperationException($"Mã tài sản '{assetCode}' không tồn tại.");
                    }

                    if (string.IsNullOrWhiteSpace(empCode) || !empDict.TryGetValue(empCode.ToLower(), out Guid employeeId))
                    {
                        throw new InvalidOperationException($"Mã nhân viên '{empCode}' không tồn tại.");
                    }

                    if (string.IsNullOrWhiteSpace(companyCode) || !companyDict.TryGetValue(companyCode.ToLower(), out Guid companyId))
                    {
                        throw new InvalidOperationException($"Mã công ty '{companyCode}' không tồn tại.");
                    }

                    bool exists = await _context.AssetTicketEntities.AnyAsync(
                        x => !x.IsDeleted && x.Code.ToLower() == code.ToLower(),
                        cancellationToken);
                    if (exists)
                    {
                        throw new InvalidOperationException($"Mã phiếu '{code}' đã tồn tại.");
                    }

                    string rawType = ExcelHelper.GetCellString(row, 5).Trim().ToUpperInvariant();
                    string ticketType = rawType switch
                    {
                        "RETURN" => AssetTicketType.Return,
                        "REPAIR" => AssetTicketType.Repair,
                        "TRANSFER" => AssetTicketType.Transfer,
                        _ => AssetTicketType.Issue
                    };

                    string toEmpCode = ExcelHelper.GetCellString(row, 6).Trim();
                    Guid? toEmployeeId = null;
                    if (!string.IsNullOrWhiteSpace(toEmpCode))
                    {
                        if (!empDict.TryGetValue(toEmpCode.ToLower(), out Guid matchedToEmpId))
                        {
                            throw new InvalidOperationException($"Mã nhân viên tiếp nhận '{toEmpCode}' không tồn tại.");
                        }

                        toEmployeeId = matchedToEmpId;
                    }

                    DateTime? ticketAt = row.Cell(7).IsEmpty() ? DateTime.UtcNow : row.Cell(7).GetDateTime();
                    DateOnly? returnExpectedDate = ExcelHelper.ParseDateOnly(row.Cell(8));

                    AssetTicketEntity entity = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        Code = code.ToUpperInvariant(),
                        AssetId = assetId,
                        EmployeeId = employeeId,
                        ToEmployeeId = toEmployeeId,
                        CompanyId = companyId,
                        TicketType = ticketType,
                        Status = AssetTicketStatus.Draft,
                        TicketAt = ticketAt ?? DateTime.UtcNow,
                        ReturnExpectedDate = returnExpectedDate,
                        Condition = ExcelHelper.GetCellString(row, 9),
                        Note = ExcelHelper.GetCellString(row, 10)
                    };

                    _ = _context.AssetTicketEntities.Add(entity);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "AssetTicketEntity",
                        entity.Id,
                        null,
                        new { entity.Id, entity.Code, entity.TicketType },
                        "Import Excel - Tạo mới phiếu tài sản " + entity.Code);

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

    internal sealed class AssetTicketExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class AssetTicketExcelColumns
    {
        public static readonly AssetTicketExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã phiếu", Required = true },
            new() { Title = "Mã tài sản", Required = true },
            new() { Title = "Mã nhân viên", Required = true },
            new() { Title = "Mã công ty (Parent)", Required = true },
            new() { Title = "Loại phiếu (ISSUE/RETURN/REPAIR/TRANSFER)", Required = true },
            new() { Title = "Mã NV tiếp nhận (chỉ khi TRANSFER)", Required = false },
            new() { Title = "Ngày lập phiếu (dd/MM/yyyy)", Required = false },
            new() { Title = "Hạn trả dự kiến (dd/MM/yyyy)", Required = false },
            new() { Title = "Tình trạng khi giao/nhận", Required = false },
            new() { Title = "Ghi chú", Required = false },
            new() { Title = "Trạng thái phiếu", Required = false, ExportOnly = true },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true }
        };

        public static IEnumerable<AssetTicketExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class AssetTicketExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<AssetTicketExcelColumnDefinition> columns = AssetTicketExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                AssetTicketExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "PH-BG001",
                "TS-LAPTOP01",
                "NV001",
                "CT01",
                "ISSUE",
                "",
                "20/08/2026",
                "20/08/2028",
                "Máy mới 100%, đầy đủ sạc và balo",
                "Cấp phát cho nhân viên mới onboarding"
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

        public static void WriteTicketRow(
            IXLWorksheet worksheet,
            int row,
            AssetTicketEntity ticket,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                ticket.Code,
                ticket.Asset?.Code ?? ticket.AssetId.ToString(),
                ticket.Employee?.Code ?? ticket.EmployeeId.ToString(),
                ticket.Company?.Code ?? ticket.CompanyId.ToString(),
                ticket.TicketType,
                ticket.ToEmployee?.Code ?? ticket.ToEmployeeId?.ToString() ?? string.Empty,
                ticket.TicketAt.ToString("dd/MM/yyyy"),
                ticket.ReturnExpectedDate?.ToString("dd/MM/yyyy") ?? string.Empty,
                ticket.Condition,
                ticket.Note
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(ticket.Status);
                values.Add(ticket.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

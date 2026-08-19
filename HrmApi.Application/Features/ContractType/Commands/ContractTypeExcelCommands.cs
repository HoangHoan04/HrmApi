using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.ContractType.Commands
{
    public class ExportContractTypesExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportContractTypesExcelQueryHandler : IRequestHandler<ExportContractTypesExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportContractTypesExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportContractTypesExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ContractTypeEntity> query = _context.ContractTypeEntities.AsNoTracking();

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

            List<ContractTypeEntity> contractTypes = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            Dictionary<Guid, string> companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachLoaiHopDong");
            ContractTypeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < contractTypes.Count; i++)
            {
                ContractTypeEntity contractType = contractTypes[i];
                string? companyCode = contractType.CompanyId.HasValue && companyDict.TryGetValue(contractType.CompanyId.Value, out string? cc) ? cc : null;
                ContractTypeExcelWriter.WriteContractTypeRow(worksheet, i + 2, contractType, companyCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadContractTypesExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadContractTypesExcelTemplateQueryHandler : IRequestHandler<DownloadContractTypesExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadContractTypesExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadContractTypesExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("LoaiHopDong");
            ContractTypeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            ContractTypeExcelWriter.WriteTemplateSampleRow(worksheet);
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

    public class ImportContractTypesExcelCommand : IRequest<ContractTypeImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class ContractTypeImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportContractTypesExcelCommandHandler : IRequestHandler<ImportContractTypesExcelCommand, ContractTypeImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportContractTypesExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<ContractTypeImportResultDto> Handle(ImportContractTypesExcelCommand request, CancellationToken cancellationToken)
        {
            ContractTypeImportResultDto result = new();

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
                    ContractTypeCommandFields command = ReadRow(row, companyDict);
                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("HD-THUVIEC", StringComparison.OrdinalIgnoreCase) || command.Code.Equals("LHD001", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    await CreateContractTypeCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    ContractTypeEntity contractType = new()
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    ContractTypeMapper.ApplyCommandFields(contractType, command);

                    _ = _context.ContractTypeEntities.Add(contractType);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "ContractTypeEntity",
                        contractType.Id,
                        null,
                        ContractTypeMapper.ToLogObject(contractType),
                        "Import Excel - Tạo mới loại hợp đồng " + contractType.Name);

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

        private static ContractTypeCommandFields ReadRow(IXLRangeRow row, Dictionary<string, Guid> companyDict)
        {
            string companyCode = ExcelHelper.GetCellString(row, 4).Trim().ToLower();
            Guid? companyId = !string.IsNullOrWhiteSpace(companyCode) && companyDict.TryGetValue(companyCode, out Guid cid) ? cid : null;

            return new ContractTypeCommandFields
            {
                Code = ExcelHelper.GetCellString(row, 1),
                Name = ExcelHelper.GetCellString(row, 2),
                Description = ExcelHelper.GetCellString(row, 3),
                CompanyId = companyId,
                IsProbation = ExcelHelper.ParseBool(row.Cell(5)) ?? false,
                IsUnlimited = ExcelHelper.ParseBool(row.Cell(6)) ?? false,
                DefaultDurationMonths = ExcelHelper.ParseInt(row.Cell(7)),
                MaxRenewalTimes = ExcelHelper.ParseInt(row.Cell(8)),
                NotifyBeforeExpiryDays = ExcelHelper.ParseInt(row.Cell(9)),
                IsActive = ExcelHelper.ParseBool(row.Cell(10)) ?? true,
                DisplayOrder = ExcelHelper.ParseInt(row.Cell(11)) ?? 0
            };
        }
    }

    internal sealed class ContractTypeExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class ContractTypeExcelColumns
    {
        public static readonly ContractTypeExcelColumnDefinition[] Definitions =
        {
        new() { Title = "Mã loại hợp đồng", Required = true },
        new() { Title = "Tên loại hợp đồng", Required = true },
        new() { Title = "Mô tả", Required = false },
        new() { Title = "Mã công ty", Required = false },
        new() { Title = "Thử việc?", Required = false },
        new() { Title = "Không xác định thời hạn?", Required = false },
        new() { Title = "Thời hạn mặc định (tháng)", Required = false },
        new() { Title = "Số lần gia hạn tối đa", Required = false },
        new() { Title = "Số ngày thông báo trước hết hạn", Required = false },
        new() { Title = "Kích hoạt", Required = false },
        new() { Title = "Thứ tự hiển thị", Required = false },
        new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
    };

        public static IEnumerable<ContractTypeExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class ContractTypeExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            List<ContractTypeExcelColumnDefinition> columns = ContractTypeExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                ContractTypeExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "HD-THUVIEC",
                "Hợp đồng thử việc",
                "Áp dụng cho nhân viên trong giai đoạn thử việc 2 tháng",
                "CT01",
                "Có",
                "Không",
                "2",
                "1",
                "15",
                "Có",
                "1"
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

        public static void WriteContractTypeRow(
            IXLWorksheet worksheet,
            int row,
            ContractTypeEntity contractType,
            string? companyCode,
            bool includeExportOnlyColumns)
        {
            List<string?> values =
            [
                contractType.Code,
            contractType.Name,
            contractType.Description,
            companyCode,
            contractType.IsProbation ? "Có" : "Không",
            contractType.IsUnlimited ? "Có" : "Không",
            contractType.DefaultDurationMonths?.ToString(),
            contractType.MaxRenewalTimes?.ToString(),
            contractType.NotifyBeforeExpiryDays?.ToString(),
            contractType.IsActive ? "Có" : "Không",
            contractType.DisplayOrder.ToString()
            ];

            if (includeExportOnlyColumns)
            {
                values.Add(contractType.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

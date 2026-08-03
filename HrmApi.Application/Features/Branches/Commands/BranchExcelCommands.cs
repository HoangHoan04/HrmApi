using System;
using System.Collections.Generic;
using System.Text;
using ClosedXML.Excel;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Features.Companies.Commands;
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
            var query = _context.BranchEntities.AsNoTracking();

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

            var branches = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Danh sach chi nhanh");
            BranchExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < branches.Count; i++)
            {
                BranchExcelWriter.WriteBranchRow(worksheet, i + 2, branches[i], includeExportOnlyColumns: true);
            }

            BranchExcelWriter.ApplyColumnWidths(worksheet);
            BranchExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadBranchExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadBranchExcelTemplateQueryHandler : IRequestHandler<DownloadBranchExcelTemplateQuery, byte[]>
    {
        public Task<byte[]> Handle(DownloadBranchExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("MauImport");
            BranchExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            BranchExcelWriter.ApplyColumnWidths(worksheet);
            BranchExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
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
        public List<string> Errors { get; set; } = new();
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
            var result = new BranchImportResultDto();

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
                    {
                        continue;
                    }

                    await CreateBranchCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var branch = new BranchEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    BranchMapper.ApplyCommandFields(branch, command);

                    _context.BranchEntities.Add(branch);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "BranchEntity",
                        branch.Id,
                        null,
                        BranchMapper.ToLogObject(branch),
                        "Import Excel - Tạo mới chi nhánh" + branch.Name);

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

        private static BranchCommandFields ReadRow(IXLRangeRow row)
        {
            return new BranchCommandFields
            {
                Code = GetCellString(row, 1),
                Name = GetCellString(row, 2),
                ShortName = GetCellString(row, 3),
                Description = GetCellString(row, 4),
                Type = GetCellString(row, 5),
                IsHeadQuarter = bool.Parse(GetCellString(row, 6) ?? "false"),
                Address = GetCellString(row, 7),
                Country = GetCellString(row, 8),
                City = GetCellString(row, 9),
                District = GetCellString(row, 10),
                Ward = GetCellString(row, 11),
                Latitude = double.Parse(GetCellString(row, 12) ?? "0"),
                Longitude = double.Parse(GetCellString(row, 13) ?? "0"),
                PhoneNumber = GetCellString(row, 14),
                Email = GetCellString(row, 15),
                Fax = GetCellString(row, 16),
                IpAddress = GetCellString(row, 17),
                ManagerName = GetCellString(row, 18),
                ManagerPhone = GetCellString(row, 19),
                TaxCode = GetCellString(row, 20),
                BusinessRegistrationCode = GetCellString(row, 21),
                OpeningDate = DateTime.Parse(GetCellString(row, 22) ?? DateTime.UtcNow.ToString()),
                ClosingDate = DateTime.Parse(GetCellString(row, 23) ?? DateTime.UtcNow.ToString()),
                OperatingStatus = GetCellString(row, 24),
                IsActive = bool.Parse(GetCellString(row, 25) ?? "true"),
                IsUsingHrm = bool.Parse(GetCellString(row, 26) ?? "true"),
                DisplayOrder = int.Parse(GetCellString(row, 27) ?? "0"),
                GroupSalary = GetCellString(row, 28),
                MaxEmployeeCapacity = int.Parse(GetCellString(row, 29) ?? "0"),
                TimeZone = GetCellString(row, 30)
            };
        }
        private static string GetCellString(IXLRangeRow row, int column) =>
         row.Cell(column).GetString().Trim();

        private static DateTime? ParseDate(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue(out DateTime dateValue)) return dateValue;
            if (DateTime.TryParse(cell.GetString(), out var parsed)) return parsed;
            return null;
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
            new() { Title = "Quận/Huyện", Required = false },
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
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<BranchExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns) =>
            Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
    }

    internal static class BranchExcelWriter
    {
        private static readonly XLColor RequiredHeaderColor = XLColor.FromHtml("#FFC000");
        private static readonly XLColor OptionalHeaderColor = XLColor.FromHtml("#92D050");

        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = BranchExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (var col = 0; col < columns.Count; col++)
            {
                var definition = columns[col];
                var cell = worksheet.Cell(1, col + 1);
                cell.Value = definition.Required ? $"{definition.Title}*" : definition.Title;

                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.Black;
                cell.Style.Fill.BackgroundColor = definition.Required ? RequiredHeaderColor : OptionalHeaderColor;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteBranchRow(
            IXLWorksheet worksheet,
            int row,
            BranchEntity branch,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                branch.Code,
                branch.Name,
                branch.ShortName,
                branch.Description,
                branch.Type,
                branch.IsHeadQuarter ? "Có" : "Không",
                branch.Address,
                branch.Country,
                branch.City,
                branch.District,
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
                branch.TimeZone
            };

            if (includeExportOnlyColumns)
            {
                values.Add(branch.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
            }

            for (var col = 0; col < values.Count; col++)
            {
                var cell = worksheet.Cell(row, col + 1);
                cell.Value = values[col] ?? string.Empty;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }

        public static void ApplyColumnWidths(IXLWorksheet worksheet)
        {
            var usedColumns = worksheet.ColumnsUsed();
            foreach (var column in usedColumns)
            {
                column.AdjustToContents(8, 60);
                column.Width = Math.Max(column.Width + 2, 12);
            }
        }

        public static void FreezeHeaderRow(IXLWorksheet worksheet)
        {
            worksheet.SheetView.FreezeRows(1);
        }
    }
}

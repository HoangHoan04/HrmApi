using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Organization;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Departments.Commands
{

    public class ExportDepartmentsExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class ExportDepartmentsExcelQueryHandler : IRequestHandler<ExportDepartmentsExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportDepartmentsExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportDepartmentsExcelQuery request, CancellationToken cancellationToken)
        {
            var query = _context.DepartmentEntities.AsNoTracking();

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
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

            if (request.BranchId.HasValue)
                query = query.Where(x => x.BranchId == request.BranchId.Value);

            if (request.CompanyId.HasValue)
                query = query.Where(x => x.CompanyId == request.CompanyId.Value);

            var departments = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachPhongBan");
            DepartmentExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < departments.Count; i++)
            {
                DepartmentExcelWriter.WriteDepartmentRow(worksheet, i + 2, departments[i], includeExportOnlyColumns: true);
            }

            DepartmentExcelWriter.ApplyColumnWidths(worksheet);
            DepartmentExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadDepartmentExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadDepartmentExcelTemplateQueryHandler : IRequestHandler<DownloadDepartmentExcelTemplateQuery, byte[]>
    {
        public Task<byte[]> Handle(DownloadDepartmentExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("MauImport");
            DepartmentExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            DepartmentExcelWriter.ApplyColumnWidths(worksheet);
            DepartmentExcelWriter.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }

    public class ImportDepartmentsExcelCommand : IRequest<DepartmentImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class DepartmentImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ImportDepartmentsExcelCommandHandler : IRequestHandler<ImportDepartmentsExcelCommand, DepartmentImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public ImportDepartmentsExcelCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<DepartmentImportResultDto> Handle(ImportDepartmentsExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new DepartmentImportResultDto();

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
                        continue;

                    await CreateDepartmentCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var department = new DepartmentEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    DepartmentMapper.ApplyCommandFields(department, command);

                    _context.DepartmentEntities.Add(department);
                    await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "DepartmentEntity",
                        department.Id,
                        null,
                        DepartmentMapper.ToLogObject(department),
                        $"Import Excel - Tạo mới phòng ban {department.Name}");

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

        private static DepartmentCommandFields ReadRow(IXLRangeRow row)
        {
            return new DepartmentCommandFields
            {
                Code = GetCellString(row, 1),
                Name = GetCellString(row, 2),
                ShortName = GetCellString(row, 3),
                Description = GetCellString(row, 4),
                Type = GetCellString(row, 5),
                CompanyId = ParseGuid(row.Cell(6)),
                BranchId = ParseGuid(row.Cell(7)),
                ParentDepartmentId = ParseGuid(row.Cell(8)),
                Level = ParseInt(row.Cell(9)) ?? 1,
                Limit = ParseInt(row.Cell(10)) ?? 0,
                CurrentHeadCount = ParseInt(row.Cell(11)),
                ManagerId = ParseGuid(row.Cell(12)),
                DeputyManagerId = ParseGuid(row.Cell(13)),
                Email = GetCellString(row, 14),
                PhoneExtension = GetCellString(row, 15),
                CostCenterCode = GetCellString(row, 16),
                IsActive = ParseBool(row.Cell(17)) ?? true,
                DisplayOrder = ParseInt(row.Cell(18)) ?? 0,
                EstablishedDate = ParseDate(row.Cell(19)),
                DissolvedDate = ParseDate(row.Cell(20)),
                IsNotifyMarketing = ParseBool(row.Cell(21)) ?? false
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

        private static int? ParseInt(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue(out int intValue)) return intValue;
            if (int.TryParse(cell.GetString(), out var parsed)) return parsed;
            return null;
        }

        private static Guid? ParseGuid(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            var str = cell.GetString().Trim();
            if (Guid.TryParse(str, out var guid)) return guid;
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

    internal sealed class DepartmentExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class DepartmentExcelColumns
    {
        public static readonly DepartmentExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã phòng ban", Required = true },
            new() { Title = "Tên phòng ban", Required = true },
            new() { Title = "Tên viết tắt", Required = false },
            new() { Title = "Mô tả", Required = false },
            new() { Title = "Loại phòng ban", Required = false },
            new() { Title = "CompanyId", Required = false },
            new() { Title = "BranchId", Required = false },
            new() { Title = "ParentDepartmentId", Required = false },
            new() { Title = "Cấp bậc", Required = false },
            new() { Title = "Định biên", Required = false },
            new() { Title = "Số lượng hiện tại", Required = false },
            new() { Title = "ManagerId", Required = false },
            new() { Title = "DeputyManagerId", Required = false },
            new() { Title = "Email", Required = false },
            new() { Title = "SĐT nội bộ", Required = false },
            new() { Title = "Mã Cost Center", Required = false },
            new() { Title = "Kích hoạt", Required = false },
            new() { Title = "Thứ tự hiển thị", Required = false },
            new() { Title = "Ngày thành lập", Required = false },
            new() { Title = "Ngày giải thể", Required = false },
            new() { Title = "Nhận thông báo marketing", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<DepartmentExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns) =>
            Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
    }

    internal static class DepartmentExcelWriter
    {
        private static readonly XLColor RequiredHeaderColor = XLColor.FromHtml("#FFC000");
        private static readonly XLColor OptionalHeaderColor = XLColor.FromHtml("#92D050");

        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = DepartmentExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

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

        public static void WriteDepartmentRow(
            IXLWorksheet worksheet,
            int row,
            DepartmentEntity department,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                department.Code,
                department.Name,
                department.ShortName,
                department.Description,
                department.Type,
                department.CompanyId?.ToString(),
                department.BranchId?.ToString(),
                department.ParentDepartmentId?.ToString(),
                department.Level.ToString(),
                department.Limit.ToString(),
                department.CurrentHeadCount?.ToString(),
                department.ManagerId?.ToString(),
                department.DeputyManagerId?.ToString(),
                department.Email,
                department.PhoneExtension,
                department.CostCenterCode,
                department.IsActive.ToString(),
                department.DisplayOrder.ToString(),
                department.EstablishedDate?.ToString("yyyy-MM-dd"),
                department.DissolvedDate?.ToString("yyyy-MM-dd"),
                department.IsNotifyMarketing.ToString(),
            };

            if (includeExportOnlyColumns)
            {
                values.Add(department.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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
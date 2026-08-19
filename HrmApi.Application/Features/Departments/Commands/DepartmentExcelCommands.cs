using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
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
            IQueryable<DepartmentEntity> query = _context.DepartmentEntities.AsNoTracking();

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

            if (request.BranchId.HasValue)
            {
                query = query.Where(x => x.BranchId == request.BranchId.Value);
            }

            if (request.CompanyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == request.CompanyId.Value);
            }

            List<DepartmentEntity> departments = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);
            Dictionary<Guid, string> companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            Dictionary<Guid, string> branchDict = await _context.BranchEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            Dictionary<Guid, string> deptDict = await _context.DepartmentEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachPhongBan");
            DepartmentExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < departments.Count; i++)
            {
                DepartmentEntity dept = departments[i];
                string? companyCode = dept.CompanyId.HasValue && companyDict.TryGetValue(dept.CompanyId.Value, out string? cc) ? cc : null;
                string? branchCode = dept.BranchId.HasValue && branchDict.TryGetValue(dept.BranchId.Value, out string? bc) ? bc : null;
                string? parentDeptCode = dept.ParentDepartmentId.HasValue && deptDict.TryGetValue(dept.ParentDepartmentId.Value, out string? pdc) ? pdc : null;

                DepartmentExcelWriter.WriteDepartmentRow(worksheet, i + 2, dept, companyCode, branchCode, parentDeptCode, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

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
        private readonly IApplicationDbContext _context;

        public DownloadDepartmentExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadDepartmentExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("PhongBan");
            DepartmentExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            DepartmentExcelWriter.WriteTemplateSampleRow(worksheet);
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
            ExcelHelper.WriteReferenceSheet(workbook, "PhongBanThamChieu", "Mã phòng ban", "Tên phòng ban", departments.Select(x => (x.Code, x.Name)));

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
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
        public List<string> Errors { get; set; } = [];
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
            IXLWorksheet worksheet = workbook.Worksheet(1);
            List<IXLRangeRow> rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? [];

            result.TotalRows = rows.Count;

            Dictionary<string, Guid> companyDict = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            Dictionary<string, Guid> branchDict = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            Dictionary<string, Guid> deptDict = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            foreach (IXLRangeRow? row in rows)
            {
                int rowNumber = row.RowNumber();
                try
                {
                    DepartmentCommandFields command = ReadRow(row, companyDict, branchDict, deptDict);
                    if (string.IsNullOrWhiteSpace(command.Code) && string.IsNullOrWhiteSpace(command.Name))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("PB001", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    await CreateDepartmentCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var department = new DepartmentEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    DepartmentMapper.ApplyCommandFields(department, command);

                    _ = _context.DepartmentEntities.Add(department);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "DepartmentEntity",
                        department.Id,
                        null,
                        DepartmentMapper.ToLogObject(department),
                        "Import Excel - Tạo mới phòng ban " + department.Name);

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

        private static DepartmentCommandFields ReadRow(
            IXLRangeRow row,
            Dictionary<string, Guid> companyDict,
            Dictionary<string, Guid> branchDict,
            Dictionary<string, Guid> deptDict)
        {
            string companyCode = ExcelHelper.GetCellString(row, 6).Trim().ToLower();
            string branchCode = ExcelHelper.GetCellString(row, 7).Trim().ToLower();
            string parentDeptCode = ExcelHelper.GetCellString(row, 8).Trim().ToLower();

            Guid? companyId = !string.IsNullOrWhiteSpace(companyCode) && companyDict.TryGetValue(companyCode, out Guid cid) ? cid : null;
            Guid? branchId = !string.IsNullOrWhiteSpace(branchCode) && branchDict.TryGetValue(branchCode, out Guid bid) ? bid : null;
            Guid? parentDeptId = !string.IsNullOrWhiteSpace(parentDeptCode) && deptDict.TryGetValue(parentDeptCode, out Guid pdid) ? pdid : null;

            return new DepartmentCommandFields
            {
                Code = ExcelHelper.GetCellString(row, 1),
                Name = ExcelHelper.GetCellString(row, 2),
                ShortName = ExcelHelper.GetCellString(row, 3),
                Description = ExcelHelper.GetCellString(row, 4),
                Type = ExcelHelper.GetCellString(row, 5),
                CompanyId = companyId,
                BranchId = branchId,
                ParentDepartmentId = parentDeptId,
                Level = ExcelHelper.ParseInt(row.Cell(9)) ?? 1,
                Limit = ExcelHelper.ParseInt(row.Cell(10)),
                Email = ExcelHelper.GetCellString(row, 11),
                PhoneExtension = ExcelHelper.GetCellString(row, 12),
                CostCenterCode = ExcelHelper.GetCellString(row, 13),
                IsActive = ExcelHelper.ParseBool(row.Cell(14)) ?? true,
                DisplayOrder = ExcelHelper.ParseInt(row.Cell(15)) ?? 0,
                EstablishedDate = ExcelHelper.ParseDate(row.Cell(16)),
                DissolvedDate = ExcelHelper.ParseDate(row.Cell(17)),
                IsNotifyMarketing = ExcelHelper.ParseBool(row.Cell(18)) ?? false,
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
            new() { Title = "Mã công ty", Required = true },
            new() { Title = "Mã chi nhánh", Required = false },
            new() { Title = "Mã phòng ban cha", Required = false },
            new() { Title = "Cấp bậc", Required = false },
            new() { Title = "Định biên", Required = false },
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

        public static IEnumerable<DepartmentExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class DepartmentExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = DepartmentExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (int col = 0; col < columns.Count; col++)
            {
                DepartmentExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            var sampleValues = new List<string>
            {
                "PB001",
                "Phòng Kế toán",
                "KT",
                "Phòng quản lý tài chính kế toán",
                "Phòng ban",
                "CT01",
                "CN01",
                "",
                "1",
                "10",
                "ketoan@company.com",
                "101",
                "CC001",
                "Có",
                "1",
                "01/01/2020",
                "",
                "Không"
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

        public static void WriteDepartmentRow(
            IXLWorksheet worksheet,
            int row,
            DepartmentEntity dept,
            string? companyCode,
            string? branchCode,
            string? parentDeptCode,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                dept.Code,
                dept.Name,
                dept.ShortName,
                dept.Description,
                dept.Type,
                companyCode,
                branchCode,
                parentDeptCode,
                dept.Level.ToString(),
                dept.Limit.ToString(),
                dept.Email,
                dept.PhoneExtension,
                dept.CostCenterCode,
                dept.IsActive ? "Có" : "Không",
                dept.DisplayOrder.ToString(),
                dept.EstablishedDate?.ToString("yyyy-MM-dd"),
                dept.DissolvedDate?.ToString("yyyy-MM-dd"),
                dept.IsNotifyMarketing ? "Có" : "Không"
            };

            if (includeExportOnlyColumns)
            {
                values.Add(dept.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
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

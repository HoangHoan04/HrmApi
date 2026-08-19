using ClosedXML.Excel;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.EmployeeMovement;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.TransferEmployees.Commands
{
    public class ExportTransferEmployeesExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? TransferType { get; set; }
        public string? Status { get; set; }
        public DateTime? EffectiveDateFrom { get; set; }
        public DateTime? EffectiveDateTo { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportTransferEmployeesExcelQueryHandler : IRequestHandler<ExportTransferEmployeesExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportTransferEmployeesExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportTransferEmployeesExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<TransferEmployeeEntity> query = _context.TransferEmployeeEntities.AsNoTracking()
                .Include(x => x.Employee)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.OldCompany)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.NewCompany)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.OldBranch)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.NewBranch)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.OldDepartment)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.NewDepartment)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.OldPart)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.NewPart)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.OldPosition)
                        .ThenInclude(p => p!.PositionMaster)
                .Include(x => x.TransferDetails)
                    .ThenInclude(d => d.NewPosition)
                        .ThenInclude(p => p!.PositionMaster);

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                string code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }

            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.TransferType))
            {
                string transferType = request.TransferType.Trim().ToLower();
                query = query.Where(x => x.TransferType.ToLower() == transferType);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                string status = request.Status.Trim().ToLower();
                query = query.Where(x => x.Status != null && x.Status.ToLower() == status);
            }

            if (request.EffectiveDateFrom.HasValue)
            {
                query = query.Where(x => x.EffectiveDate >= request.EffectiveDateFrom.Value);
            }

            if (request.EffectiveDateTo.HasValue)
            {
                query = query.Where(x => x.EffectiveDate <= request.EffectiveDateTo.Value);
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            List<TransferEmployeeEntity> transfers = await query
                .OrderByDescending(x => x.EffectiveDate)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachDieuChuyen");
            TransferEmployeeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (int i = 0; i < transfers.Count; i++)
            {
                TransferEmployeeEntity transfer = transfers[i];
                TransferEmployeeExcelWriter.WriteTransferEmployeeRow(worksheet, i + 2, transfer, includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadTransferEmployeeExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadTransferEmployeeExcelTemplateQueryHandler : IRequestHandler<DownloadTransferEmployeeExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadTransferEmployeeExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadTransferEmployeeExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DieuChuyen");
            TransferEmployeeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            TransferEmployeeExcelWriter.WriteTemplateSampleRow(worksheet);
            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            // 1. Sheet NhanVien
            var employees = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new
                {
                    x.Code,
                    FullName = (x.LastName + " " + x.FirstName).Trim(),
                    DepartmentName = x.Department != null ? x.Department.Name : string.Empty,
                    PositionName = x.Position != null && x.Position.PositionMaster != null ? x.Position.PositionMaster.Name : string.Empty
                })
                .ToListAsync(cancellationToken);

            IXLWorksheet empWs = workbook.Worksheets.Add("NhanVien");
            ExcelHelper.WriteStyledHeaderCell(empWs, 1, "Mã nhân viên", false);
            ExcelHelper.WriteStyledHeaderCell(empWs, 2, "Họ và tên", false);
            ExcelHelper.WriteStyledHeaderCell(empWs, 3, "Phòng ban hiện tại", false);
            ExcelHelper.WriteStyledHeaderCell(empWs, 4, "Chức vụ hiện tại", false);
            empWs.Row(1).Height = 28;
            for (int r = 0; r < employees.Count; r++)
            {
                var emp = employees[r];
                empWs.Cell(r + 2, 1).Value = emp.Code;
                empWs.Cell(r + 2, 2).Value = emp.FullName;
                empWs.Cell(r + 2, 3).Value = emp.DepartmentName;
                empWs.Cell(r + 2, 4).Value = emp.PositionName;
                for (int c = 1; c <= 4; c++)
                {
                    empWs.Cell(r + 2, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    empWs.Cell(r + 2, c).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
            }
            ExcelHelper.ApplyColumnWidths(empWs);
            ExcelHelper.FreezeHeaderRow(empWs);

            // 2. Sheet CongTy
            var companies = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "CongTy", "Mã công ty", "Tên công ty", companies.Select(x => (x.Code, x.Name)));

            // 3. Sheet ChiNhanh
            var branches = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "ChiNhanh", "Mã chi nhánh", "Tên chi nhánh", branches.Select(x => (x.Code, x.Name)));

            // 4. Sheet PhongBan
            var departments = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "PhongBan", "Mã phòng ban", "Tên phòng ban", departments.Select(x => (x.Code, x.Name)));

            // 5. Sheet BoPhan
            var parts = await _context.PartEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.Code))
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "BoPhan", "Mã bộ phận", "Tên bộ phận", parts.Select(x => (x.Code ?? string.Empty, x.Name ?? string.Empty)));

            // 6. Sheet ChucVu
            var positions = await _context.PositionMasterEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.Code))
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "ChucVu", "Mã chức vụ", "Tên chức vụ", positions.Select(x => (x.Code, x.Name)));

            // 7. Sheet LoaiDieuChuyen
            List<(string Code, string Name)> transferTypes =
            [
                (TransferType.InternalTransfer, "Thuyên chuyển nội bộ"),
                (TransferType.Secondment, "Biệt phái"),
                (TransferType.Rotation, "Luân chuyển"),
                (TransferType.CompanyTransfer, "Chuyển công ty"),
                (TransferType.BranchTransfer, "Chuyển chi nhánh"),
                (TransferType.Promotion, "Bổ nhiệm / Thăng chức"),
                (TransferType.Demotion, "Giáng chức"),
                (TransferType.Dismissal, "Miễn nhiệm")
            ];
            ExcelHelper.WriteReferenceSheet(workbook, "LoaiDieuChuyen", "Mã loại điều chuyển", "Tên loại điều chuyển", transferTypes);

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class ImportTransferEmployeesExcelCommand : IRequest<TransferEmployeeImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class TransferEmployeeImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportTransferEmployeesExcelCommandHandler : IRequestHandler<ImportTransferEmployeesExcelCommand, TransferEmployeeImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly IWorkflowEngine _workflow;

        public ImportTransferEmployeesExcelCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            IWorkflowEngine workflow)
        {
            _context = context;
            _actionLog = actionLog;
            _workflow = workflow;
        }

        public async Task<TransferEmployeeImportResultDto> Handle(ImportTransferEmployeesExcelCommand request, CancellationToken cancellationToken)
        {
            TransferEmployeeImportResultDto result = new();

            using MemoryStream stream = new(request.FileContent);
            using XLWorkbook workbook = new(stream);
            IXLWorksheet worksheet = workbook.Worksheet(1);
            List<IXLRangeRow> rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? [];

            result.TotalRows = rows.Count;

            Dictionary<string, EmployeeEntity> employeeDict = await _context.EmployeeEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x, cancellationToken);

            Dictionary<string, Guid> companyDict = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            Dictionary<string, Guid> branchDict = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            Dictionary<string, Guid> deptDict = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            Dictionary<string, Guid> partDict = await _context.PartEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.Code))
                .ToDictionaryAsync(x => x.Code!.Trim().ToLower(), x => x.Id, cancellationToken);

            Dictionary<string, Guid> posMasterDict = await _context.PositionEntities.AsNoTracking()
                .Include(x => x.PositionMaster)
                .Where(x => !x.IsDeleted && x.PositionMaster != null && !string.IsNullOrWhiteSpace(x.PositionMaster.Code))
                .ToDictionaryAsync(x => x.PositionMaster!.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            foreach (IXLRangeRow? row in rows)
            {
                int rowNumber = row.RowNumber();
                try
                {
                    TransferEmployeeExcelRowData rowData = ReadRow(row, companyDict, branchDict, deptDict, partDict, posMasterDict);

                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(rowData.Code) && string.IsNullOrWhiteSpace(rowData.EmployeeCode))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    // Skip sample template row
                    if (rowData.Code.Equals("DC-2026-001", StringComparison.OrdinalIgnoreCase) ||
                        rowData.Code.Equals("DC-MAU001", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(rowData.Code))
                    {
                        throw new InvalidOperationException("Mã điều chuyển là bắt buộc.");
                    }

                    if (string.IsNullOrWhiteSpace(rowData.EmployeeCode))
                    {
                        throw new InvalidOperationException("Mã nhân viên là bắt buộc.");
                    }

                    if (!employeeDict.TryGetValue(rowData.EmployeeCode.Trim().ToLower(), out EmployeeEntity? employee))
                    {
                        throw new InvalidOperationException($"Nhân viên với mã '{rowData.EmployeeCode}' không tồn tại trong hệ thống.");
                    }

                    if (string.IsNullOrWhiteSpace(rowData.TransferType))
                    {
                        throw new InvalidOperationException("Loại điều chuyển là bắt buộc.");
                    }

                    if (!rowData.EffectiveDate.HasValue)
                    {
                        throw new InvalidOperationException("Ngày hiệu lực là bắt buộc và phải đúng định dạng ngày tháng.");
                    }

                    bool codeExists = await _context.TransferEmployeeEntities.AnyAsync(
                        x => !x.IsDeleted && x.Code.ToLower() == rowData.Code.Trim().ToLower(),
                        cancellationToken);
                    if (codeExists)
                    {
                        throw new InvalidOperationException($"Mã điều chuyển '{rowData.Code}' đã tồn tại trong hệ thống.");
                    }

                    // Build Header Entity
                    TransferEmployeeEntity entity = new()
                    {
                        EmployeeId = employee.Id,
                        Code = rowData.Code.Trim(),
                        TransferType = rowData.TransferType.Trim(),
                        RequestDate = rowData.RequestDate ?? DateTime.UtcNow,
                        EffectiveDate = rowData.EffectiveDate.Value,
                        ExpectedEndDate = rowData.ExpectedEndDate,
                        Reason = string.IsNullOrWhiteSpace(rowData.Reason) ? null : rowData.Reason.Trim(),
                        DecisionNumber = string.IsNullOrWhiteSpace(rowData.DecisionNumber) ? null : rowData.DecisionNumber.Trim(),
                        DecisionDate = rowData.DecisionDate,
                        Note = string.IsNullOrWhiteSpace(rowData.Note) ? null : rowData.Note.Trim(),
                        Status = TransferStatus.Pending,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Build Detail Entity
                    Guid? oldCompany = employee.CompanyId;
                    Guid? oldBranch = employee.BranchId;
                    Guid? oldDepartment = employee.DepartmentId;
                    Guid? oldPart = employee.PartId;
                    Guid? oldPosition = employee.PositionId;

                    Guid? newCompany = rowData.NewCompanyId ?? oldCompany;
                    Guid? newBranch = rowData.NewBranchId ?? oldBranch;
                    Guid? newDepartment = rowData.NewDepartmentId ?? oldDepartment;
                    Guid? newPart = rowData.NewPartId ?? oldPart;
                    Guid? newPosition = rowData.NewPositionId ?? oldPosition;

                    TransferEmployeePositionEntity detail = new()
                    {
                        TransferEmployee = entity,
                        EmployeeId = employee.Id,
                        EffectiveDate = rowData.EffectiveDate.Value,
                        OldCompanyId = oldCompany,
                        NewCompanyId = newCompany,
                        OldBranchId = oldBranch,
                        NewBranchId = newBranch,
                        OldDepartmentId = oldDepartment,
                        NewDepartmentId = newDepartment,
                        OldPartId = oldPart,
                        NewPartId = newPart,
                        OldPositionId = oldPosition,
                        NewPositionId = newPosition,
                        ChangeType = string.IsNullOrWhiteSpace(rowData.ChangeType) ? null : rowData.ChangeType.Trim(),
                        Note = string.IsNullOrWhiteSpace(rowData.Note) ? null : rowData.Note.Trim(),
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    entity.TransferDetails.Add(detail);

                    _ = _context.TransferEmployeeEntities.Add(entity);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "TransferEmployeeEntity",
                        entity.Id,
                        null,
                        TransferEmployeeMapper.ToLogObject(entity),
                        "Import Excel - Tạo mới đơn điều chuyển " + entity.Code);

                    _ = await _workflow.StartAsync(
                        Domain.Enums.WorkflowEntityType.Transfer, entity.Id, employee.CompanyId, cancellationToken);

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

        private static TransferEmployeeExcelRowData ReadRow(
            IXLRangeRow row,
            Dictionary<string, Guid> companyDict,
            Dictionary<string, Guid> branchDict,
            Dictionary<string, Guid> deptDict,
            Dictionary<string, Guid> partDict,
            Dictionary<string, Guid> posDict)
        {
            string code = ExcelHelper.GetCellString(row, 1);
            string empCode = ExcelHelper.GetCellString(row, 2);
            string rawTransferType = ExcelHelper.GetCellString(row, 3);
            DateTime? reqDate = ExcelHelper.ParseDate(row.Cell(4));
            DateTime? effDate = ExcelHelper.ParseDate(row.Cell(5));
            DateTime? expEndDate = ExcelHelper.ParseDate(row.Cell(6));
            string reason = ExcelHelper.GetCellString(row, 7);
            string decisionNumber = ExcelHelper.GetCellString(row, 8);
            DateTime? decisionDate = ExcelHelper.ParseDate(row.Cell(9));

            string newCompanyCode = ExcelHelper.GetCellString(row, 10).Trim().ToLower();
            string newBranchCode = ExcelHelper.GetCellString(row, 11).Trim().ToLower();
            string newDeptCode = ExcelHelper.GetCellString(row, 12).Trim().ToLower();
            string newPartCode = ExcelHelper.GetCellString(row, 13).Trim().ToLower();
            string newPosCode = ExcelHelper.GetCellString(row, 14).Trim().ToLower();

            string changeType = ExcelHelper.GetCellString(row, 15);
            string note = ExcelHelper.GetCellString(row, 16);

            Guid? newCompanyId = !string.IsNullOrWhiteSpace(newCompanyCode) && companyDict.TryGetValue(newCompanyCode, out Guid cid) ? cid : null;
            Guid? newBranchId = !string.IsNullOrWhiteSpace(newBranchCode) && branchDict.TryGetValue(newBranchCode, out Guid bid) ? bid : null;
            Guid? newDeptId = !string.IsNullOrWhiteSpace(newDeptCode) && deptDict.TryGetValue(newDeptCode, out Guid did) ? did : null;
            Guid? newPartId = !string.IsNullOrWhiteSpace(newPartCode) && partDict.TryGetValue(newPartCode, out Guid pid) ? pid : null;
            Guid? newPosId = !string.IsNullOrWhiteSpace(newPosCode) && posDict.TryGetValue(newPosCode, out Guid psid) ? psid : null;

            string normalizedTransferType = NormalizeTransferType(rawTransferType);

            return new TransferEmployeeExcelRowData
            {
                Code = code,
                EmployeeCode = empCode,
                TransferType = normalizedTransferType,
                RequestDate = reqDate,
                EffectiveDate = effDate,
                ExpectedEndDate = expEndDate,
                Reason = reason,
                DecisionNumber = decisionNumber,
                DecisionDate = decisionDate,
                NewCompanyId = newCompanyId,
                NewBranchId = newBranchId,
                NewDepartmentId = newDeptId,
                NewPartId = newPartId,
                NewPositionId = newPosId,
                ChangeType = changeType,
                Note = note
            };
        }

        private static string NormalizeTransferType(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            string lower = raw.Trim().ToLower();
            return lower switch
            {
                "thuyên chuyển nội bộ" or "thuyen chuyen noi bo" or "internal_transfer" or "internal transfer" => TransferType.InternalTransfer,
                "biệt phái" or "biet phai" or "secondment" => TransferType.Secondment,
                "luân chuyển" or "luan chuyen" or "rotation" => TransferType.Rotation,
                "chuyển công ty" or "chuyen cong ty" or "company_transfer" or "company transfer" => TransferType.CompanyTransfer,
                "chuyển chi nhánh" or "chuyen chi nhanh" or "branch_transfer" or "branch transfer" => TransferType.BranchTransfer,
                "bổ nhiệm" or "thăng chức" or "bo nhiem" or "thang chuc" or "promotion" => TransferType.Promotion,
                "giáng chức" or "giang chuc" or "demotion" => TransferType.Demotion,
                "miễn nhiệm" or "mien nhiem" or "dismissal" => TransferType.Dismissal,
                _ => raw.Trim().ToUpper()
            };
        }
    }

    internal class TransferEmployeeExcelRowData
    {
        public string Code { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;
        public DateTime? RequestDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public string? Reason { get; set; }
        public string? DecisionNumber { get; set; }
        public DateTime? DecisionDate { get; set; }
        public Guid? NewCompanyId { get; set; }
        public Guid? NewBranchId { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public Guid? NewPartId { get; set; }
        public Guid? NewPositionId { get; set; }
        public string? ChangeType { get; set; }
        public string? Note { get; set; }
    }

    internal sealed class TransferEmployeeExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class TransferEmployeeExcelColumns
    {
        public static readonly TransferEmployeeExcelColumnDefinition[] TemplateDefinitions =
        {
            new() { Title = "Mã điều chuyển", Required = true },
            new() { Title = "Mã nhân viên", Required = true },
            new() { Title = "Loại điều chuyển", Required = true },
            new() { Title = "Ngày đề nghị (dd/MM/yyyy)", Required = false },
            new() { Title = "Ngày hiệu lực (dd/MM/yyyy)", Required = true },
            new() { Title = "Ngày dự kiến kết thúc (dd/MM/yyyy)", Required = false },
            new() { Title = "Lý do điều chuyển", Required = false },
            new() { Title = "Số quyết định", Required = false },
            new() { Title = "Ngày quyết định (dd/MM/yyyy)", Required = false },
            new() { Title = "Mã công ty mới", Required = false },
            new() { Title = "Mã chi nhánh mới", Required = false },
            new() { Title = "Mã phòng ban mới", Required = false },
            new() { Title = "Mã bộ phận mới", Required = false },
            new() { Title = "Mã chức vụ mới", Required = false },
            new() { Title = "Loại thay đổi", Required = false },
            new() { Title = "Ghi chú", Required = false }
        };

        public static readonly TransferEmployeeExcelColumnDefinition[] ExportDefinitions =
        {
            new() { Title = "Mã điều chuyển", Required = true },
            new() { Title = "Mã nhân viên", Required = true },
            new() { Title = "Họ và tên", Required = false },
            new() { Title = "Loại điều chuyển", Required = true },
            new() { Title = "Ngày đề nghị", Required = false },
            new() { Title = "Ngày hiệu lực", Required = true },
            new() { Title = "Ngày dự kiến kết thúc", Required = false },
            new() { Title = "Lý do điều chuyển", Required = false },
            new() { Title = "Số quyết định", Required = false },
            new() { Title = "Ngày quyết định", Required = false },

            // Cơ cấu cũ
            new() { Title = "Công ty cũ", Required = false },
            new() { Title = "Chi nhánh cũ", Required = false },
            new() { Title = "Phòng ban cũ", Required = false },
            new() { Title = "Bộ phận cũ", Required = false },
            new() { Title = "Chức vụ cũ", Required = false },

            // Cơ cấu mới
            new() { Title = "Công ty mới", Required = false },
            new() { Title = "Chi nhánh mới", Required = false },
            new() { Title = "Phòng ban mới", Required = false },
            new() { Title = "Bộ phận mới", Required = false },
            new() { Title = "Chức vụ mới", Required = false },

            new() { Title = "Loại thay đổi", Required = false },
            new() { Title = "Trạng thái", Required = false },
            new() { Title = "Người duyệt", Required = false },
            new() { Title = "Ngày duyệt", Required = false },
            new() { Title = "Ghi chú", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false }
        };
    }

    internal static class TransferEmployeeExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            TransferEmployeeExcelColumnDefinition[] columns = includeExportOnlyColumns
                ? TransferEmployeeExcelColumns.ExportDefinitions
                : TransferEmployeeExcelColumns.TemplateDefinitions;

            for (int col = 0; col < columns.Length; col++)
            {
                TransferEmployeeExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            List<string> sampleValues =
            [
                "DC-2026-001",
                "NV001",
                "Thuyên chuyển nội bộ",
                DateTime.Now.ToString("dd/MM/yyyy"),
                DateTime.Now.AddDays(7).ToString("dd/MM/yyyy"),
                "",
                "Điều động nhân sự hỗ trợ dự án mới",
                "QD-01/2026/QĐ-HR",
                DateTime.Now.ToString("dd/MM/yyyy"),
                "CT01",
                "CN01",
                "PB01",
                "BP01",
                "CV01",
                "Chuyển phòng ban",
                "Điều chuyển đợt 1 năm 2026"
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

        public static void WriteTransferEmployeeRow(
            IXLWorksheet worksheet,
            int row,
            TransferEmployeeEntity transfer,
            bool includeExportOnlyColumns)
        {
            TransferEmployeePositionEntity? detail = transfer.TransferDetails.FirstOrDefault();

            string employeeName = transfer.Employee != null
                ? (transfer.Employee.LastName + " " + transfer.Employee.FirstName).Trim()
                : string.Empty;

            string transferTypeName = transfer.TransferType switch
            {
                TransferType.InternalTransfer => "Thuyên chuyển nội bộ",
                TransferType.Secondment => "Biệt phái",
                TransferType.Rotation => "Luân chuyển",
                TransferType.CompanyTransfer => "Chuyển công ty",
                TransferType.BranchTransfer => "Chuyển chi nhánh",
                TransferType.Promotion => "Bổ nhiệm / Thăng chức",
                TransferType.Demotion => "Giáng chức",
                TransferType.Dismissal => "Miễn nhiệm",
                _ => transfer.TransferType
            };

            string statusName = transfer.Status switch
            {
                TransferStatus.Pending => "Chờ duyệt",
                TransferStatus.Approved => "Đã duyệt",
                TransferStatus.Rejected => "Từ chối",
                TransferStatus.Cancelled => "Đã hủy",
                TransferStatus.Applied => "Đã áp dụng",
                _ => transfer.Status ?? string.Empty
            };

            string oldPosName = detail?.OldPosition?.PositionMaster != null
                ? detail.OldPosition.PositionMaster.Name
                : string.Empty;

            string newPosName = detail?.NewPosition?.PositionMaster != null
                ? detail.NewPosition.PositionMaster.Name
                : string.Empty;

            List<string?> values =
            [
                transfer.Code,
                transfer.Employee?.Code ?? string.Empty,
                employeeName,
                transferTypeName,
                transfer.RequestDate?.ToString("dd/MM/yyyy"),
                transfer.EffectiveDate.ToString("dd/MM/yyyy"),
                transfer.ExpectedEndDate?.ToString("dd/MM/yyyy"),
                transfer.Reason,
                transfer.DecisionNumber,
                transfer.DecisionDate?.ToString("dd/MM/yyyy"),

                // Cơ cấu cũ
                detail?.OldCompany?.Name ?? string.Empty,
                detail?.OldBranch?.Name ?? string.Empty,
                detail?.OldDepartment?.Name ?? string.Empty,
                detail?.OldPart?.Name ?? string.Empty,
                oldPosName,

                // Cơ cấu mới
                detail?.NewCompany?.Name ?? string.Empty,
                detail?.NewBranch?.Name ?? string.Empty,
                detail?.NewDepartment?.Name ?? string.Empty,
                detail?.NewPart?.Name ?? string.Empty,
                newPosName,

                detail?.ChangeType ?? string.Empty,
                statusName,
                transfer.ApprovedBy,
                transfer.ApprovedDate?.ToString("dd/MM/yyyy HH:mm"),
                transfer.Note,
                transfer.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động"
            ];

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

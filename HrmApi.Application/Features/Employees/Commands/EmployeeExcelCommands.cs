using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Employees.Commands
{
    public class ExportEmployeesExcelQuery : IRequest<byte[]>
    {
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Status { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class ExportEmployeesExcelQueryHandler : IRequestHandler<ExportEmployeesExcelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public ExportEmployeesExcelQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(ExportEmployeesExcelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<EmployeeEntity> query = _context.EmployeeEntities.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var code = request.Code.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                var fullName = request.FullName.Trim().ToLower();
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.ToLower().Contains(fullName))
                    || x.FirstName.ToLower().Contains(fullName)
                    || x.LastName.ToLower().Contains(fullName));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var status = request.Status.Trim().ToLower();
                query = query.Where(x => x.Status != null && x.Status.ToLower() == status);
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
            }

            List<EmployeeEntity> employees = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);

            var companyDict = await _context.CompanyEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var branchDict = await _context.BranchEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var deptDict = await _context.DepartmentEntities.AsNoTracking()
                .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
            var partDict = await _context.PartEntities.AsNoTracking()
                .Where(x => x.Code != null)
                .ToDictionaryAsync(x => x.Id, x => x.Code!, cancellationToken);
            var posDict = await _context.PositionEntities.AsNoTracking()
                .Include(x => x.PositionMaster)
                .Where(x => x.PositionMaster != null)
                .ToDictionaryAsync(x => x.Id, x => x.PositionMaster!.Code, cancellationToken);

            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("DanhSachNhanVien");
            EmployeeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: true);

            for (var i = 0; i < employees.Count; i++)
            {
                var emp = employees[i];
                string? companyCode = emp.CompanyId.HasValue && companyDict.TryGetValue(emp.CompanyId.Value, out var cc) ? cc : null;
                string? branchCode = emp.BranchId.HasValue && branchDict.TryGetValue(emp.BranchId.Value, out var bc) ? bc : null;
                string? deptCode = emp.DepartmentId.HasValue && deptDict.TryGetValue(emp.DepartmentId.Value, out var dc) ? dc : null;
                string? partCode = emp.PartId.HasValue && partDict.TryGetValue(emp.PartId.Value, out var pc) ? pc : null;
                string? posCode = emp.PositionId.HasValue && posDict.TryGetValue(emp.PositionId.Value, out var psc) ? psc : null;

                EmployeeExcelWriter.WriteEmployeeRow(
                    worksheet,
                    i + 2,
                    emp,
                    companyCode,
                    branchCode,
                    deptCode,
                    partCode,
                    posCode,
                    includeExportOnlyColumns: true);
            }

            ExcelHelper.ApplyColumnWidths(worksheet);
            ExcelHelper.FreezeHeaderRow(worksheet);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class DownloadEmployeeExcelTemplateQuery : IRequest<byte[]>
    {
    }

    public class DownloadEmployeeExcelTemplateQueryHandler : IRequestHandler<DownloadEmployeeExcelTemplateQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;

        public DownloadEmployeeExcelTemplateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(DownloadEmployeeExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("ThongTinNhanVien");
            EmployeeExcelWriter.WriteHeaders(worksheet, includeExportOnlyColumns: false);
            EmployeeExcelWriter.WriteTemplateSampleRow(worksheet);
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
            ExcelHelper.WriteReferenceSheet(workbook, "PhongBan", "Mã phòng ban", "Tên phòng ban", departments.Select(x => (x.Code, x.Name)));

            var parts = await _context.PartEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.Code))
                .OrderBy(x => x.Code)
                .Select(x => new { x.Code, x.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "BoPhan", "Mã bộ phận", "Tên bộ phận", parts.Select(x => (x.Code ?? string.Empty, x.Name ?? string.Empty)));

            var positions = await _context.PositionEntities.AsNoTracking()
                .Include(x => x.PositionMaster)
                .Where(x => !x.IsDeleted && x.PositionMaster != null && !x.PositionMaster.IsDeleted)
                .OrderBy(x => x.PositionMaster!.Code)
                .Select(x => new { Code = x.PositionMaster!.Code, Name = x.PositionMaster!.Name })
                .ToListAsync(cancellationToken);
            ExcelHelper.WriteReferenceSheet(workbook, "ChucVu", "Mã chức vụ", "Tên chức vụ", positions.Select(x => (x.Code, x.Name)));

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class ImportEmployeesExcelCommand : IRequest<EmployeeImportResultDto>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class EmployeeImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportEmployeesExcelCommandHandler : IRequestHandler<ImportEmployeesExcelCommand, EmployeeImportResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly IPasswordHasherService _passwordHasher;

        public ImportEmployeesExcelCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            IPasswordHasherService passwordHasher)
        {
            _context = context;
            _actionLog = actionLog;
            _passwordHasher = passwordHasher;
        }

        public async Task<EmployeeImportResultDto> Handle(ImportEmployeesExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new EmployeeImportResultDto();

            using var stream = new MemoryStream(request.FileContent);
            using var workbook = new XLWorkbook(stream);
            IXLWorksheet worksheet = workbook.Worksheet(1);
            List<IXLRangeRow> rows = worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? [];

            result.TotalRows = rows.Count;

            var companyDict = await _context.CompanyEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var branchDict = await _context.BranchEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var deptDict = await _context.DepartmentEntities.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Code.Trim().ToLower(), x => x.Id, cancellationToken);
            var partDict = await _context.PartEntities.AsNoTracking()
                .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.Code))
                .ToDictionaryAsync(x => x.Code!.Trim().ToLower(), x => x.Id, cancellationToken);
            var posDict = await _context.PositionEntities.AsNoTracking()
                .Include(x => x.PositionMaster)
                .Where(x => !x.IsDeleted && x.PositionMaster != null && !string.IsNullOrWhiteSpace(x.PositionMaster.Code))
                .ToDictionaryAsync(x => x.PositionMaster!.Code.Trim().ToLower(), x => x.Id, cancellationToken);

            foreach (IXLRangeRow? row in rows)
            {
                var rowNumber = row.RowNumber();
                try
                {
                    EmployeeCommandFields command = ReadRow(row, companyDict, branchDict, deptDict, partDict, posDict);
                    if (string.IsNullOrWhiteSpace(command.Code)
                        && string.IsNullOrWhiteSpace(command.FirstName)
                        && string.IsNullOrWhiteSpace(command.LastName)
                        && string.IsNullOrWhiteSpace(command.FullName))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    if (command.Code.Equals("NV0001", StringComparison.OrdinalIgnoreCase)
                        || command.Code.Equals("NV_MAU", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalRows--;
                        continue;
                    }

                    await CreateEmployeeCommandHandler.ValidateAsync(command, null, cancellationToken, _context);

                    var employee = new EmployeeEntity
                    {
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    EmployeeMapper.ApplyCommandFields(employee, command);

                    _ = _context.EmployeeEntities.Add(employee);
                    _ = await _context.SaveChangesAsync(cancellationToken);

                    const string defaultPassword = "123@123@";
                    string usernameNormalized = employee.Code.Trim().ToLower();
                    bool userExists = await _context.UserEntities
                        .AnyAsync(x => x.Username.ToLower() == usernameNormalized, cancellationToken);

                    if (!userExists)
                    {
                        UserEntity user = new()
                        {
                            EmployeeId = employee.Id,
                            Username = employee.Code.Trim(),
                            Email = employee.Email,
                            PhoneNumber = employee.Phone,
                            Type = "EMPLOYEE",
                            IsActive = true,
                            IsLocked = false,
                            MustChangePassword = true,
                            CreatedAt = DateTime.UtcNow,
                            CompanyId = employee.CompanyId,
                            BranchId = employee.BranchId,
                        };
                        user.PasswordHash = _passwordHasher.HashPassword(user, defaultPassword);

                        _ = _context.UserEntities.Add(user);
                        _ = await _context.SaveChangesAsync(cancellationToken);

                        bool hasEmployeeRole = await _context.RoleEntities.AsNoTracking()
                            .AnyAsync(x => x.Code == RoleCodes.Employee && !x.IsDeleted && x.IsActive, cancellationToken);
                        if (hasEmployeeRole)
                        {
                            Guid employeeRoleId = await _context.RoleEntities.AsNoTracking()
                                .Where(x => x.Code == RoleCodes.Employee && !x.IsDeleted && x.IsActive)
                                .Select(x => x.Id)
                                .FirstAsync(cancellationToken);

                            _ = _context.UserRoleEntities.Add(new UserRoleEntity
                            {
                                UserId = user.Id,
                                RoleId = employeeRoleId,
                                EffectiveFrom = DateTime.UtcNow,
                            });
                            _ = await _context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    await _actionLog.LogActionAsync(
                        ActionType.CREATE,
                        "EmployeeEntity",
                        employee.Id,
                        null,
                        EmployeeMapper.ToLogObject(employee),
                        "Import Excel - Tạo mới nhân viên " + employee.FullName);

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

        private static EmployeeCommandFields ReadRow(
            IXLRangeRow row,
            Dictionary<string, Guid> companyDict,
            Dictionary<string, Guid> branchDict,
            Dictionary<string, Guid> deptDict,
            Dictionary<string, Guid> partDict,
            Dictionary<string, Guid> posDict)
        {
            var code = ExcelHelper.GetCellString(row, 1);
            var lastName = ExcelHelper.GetCellString(row, 2);
            var firstName = ExcelHelper.GetCellString(row, 3);
            var fullName = ExcelHelper.GetCellString(row, 4);

            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(fullName))
            {
                var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1)
                {
                    lastName = parts[0];
                    firstName = parts[0];
                }
                else if (parts.Length > 1)
                {
                    lastName = parts[0];
                    firstName = string.Join(" ", parts.Skip(1));
                }
            }

            var dayOfBirth = ExcelHelper.ParseDate(row.Cell(9));
            var issuanceDate = ExcelHelper.ParseDate(row.Cell(15));
            var joinDate = ExcelHelper.ParseDate(row.Cell(31));
            var resignationDate = ExcelHelper.ParseDate(row.Cell(32));

            var companyCode = ExcelHelper.GetCellString(row, 34).Trim().ToLower();
            var branchCode = ExcelHelper.GetCellString(row, 35).Trim().ToLower();
            var deptCode = ExcelHelper.GetCellString(row, 36).Trim().ToLower();
            var partCode = ExcelHelper.GetCellString(row, 37).Trim().ToLower();
            var posCode = ExcelHelper.GetCellString(row, 38).Trim().ToLower();

            Guid? companyId = !string.IsNullOrWhiteSpace(companyCode) && companyDict.TryGetValue(companyCode, out var cid) ? cid : null;
            Guid? branchId = !string.IsNullOrWhiteSpace(branchCode) && branchDict.TryGetValue(branchCode, out var bid) ? bid : null;
            Guid? deptId = !string.IsNullOrWhiteSpace(deptCode) && deptDict.TryGetValue(deptCode, out var did) ? did : null;
            Guid? partId = !string.IsNullOrWhiteSpace(partCode) && partDict.TryGetValue(partCode, out var pid) ? pid : null;
            Guid? posId = !string.IsNullOrWhiteSpace(posCode) && posDict.TryGetValue(posCode, out var psid) ? psid : null;

            return new EmployeeCommandFields
            {
                Code = code,
                LastName = lastName,
                FirstName = firstName,
                FullName = fullName,
                Phone = ExcelHelper.GetCellString(row, 5),
                SecondaryPhone = ExcelHelper.GetCellString(row, 6),
                Email = ExcelHelper.GetCellString(row, 7),
                CompanyEmail = ExcelHelper.GetCellString(row, 8),
                DayOfBirth = dayOfBirth ?? DateTime.SpecifyKind(default, DateTimeKind.Utc),
                Nationality = ExcelHelper.GetCellString(row, 10),
                Ethnicity = ExcelHelper.GetCellString(row, 11),
                Religion = ExcelHelper.GetCellString(row, 12),
                IdentityCard = ExcelHelper.GetCellString(row, 13),
                PlaceOfIsssuance = ExcelHelper.GetCellString(row, 14),
                IssuanceDate = issuanceDate ?? DateTime.SpecifyKind(default, DateTimeKind.Utc),
                PermanentAddress = ExcelHelper.GetCellString(row, 16),
                NowAddress = ExcelHelper.GetCellString(row, 17),
                CurrentCity = ExcelHelper.GetCellString(row, 18),
                CurrentWard = ExcelHelper.GetCellString(row, 19),
                BankAccountNumber = ExcelHelper.GetCellString(row, 20),
                Bankname = ExcelHelper.GetCellString(row, 21),
                BankBranchName = ExcelHelper.GetCellString(row, 22),
                BankAccountHolder = ExcelHelper.GetCellString(row, 23),
                TaxCode = ExcelHelper.GetCellString(row, 24),
                SocialInsuranceNumber = ExcelHelper.GetCellString(row, 25),
                HealthInsuranceNumber = ExcelHelper.GetCellString(row, 26),
                Level = ExcelHelper.GetCellString(row, 27),
                WorkingMode = ExcelHelper.GetCellString(row, 28),
                ContractType = ExcelHelper.GetCellString(row, 29),
                Status = MapWorkStatus(ExcelHelper.GetCellString(row, 30)),
                JoinDate = joinDate ?? DateTime.SpecifyKind(default, DateTimeKind.Utc),
                ResignationDate = resignationDate,
                ResignationReason = ExcelHelper.GetCellString(row, 33),
                CompanyId = companyId,
                BranchId = branchId,
                DepartmentId = deptId,
                PartId = partId,
                PositionId = posId
            };
        }

        private static string MapWorkStatus(string? statusStr)
        {
            if (string.IsNullOrWhiteSpace(statusStr))
            {
                return EmployeeWorkStatus.Working;
            }

            var normalized = statusStr.Trim().ToUpperInvariant();
            if (normalized == EmployeeWorkStatus.Working || normalized.Contains("ĐANG LÀM VIỆC") || normalized.Contains("DANG LAM VIEC"))
            {
                return EmployeeWorkStatus.Working;
            }
            if (normalized == EmployeeWorkStatus.Probation || normalized.Contains("THỬ VIỆC") || normalized.Contains("THU VIEC"))
            {
                return EmployeeWorkStatus.Probation;
            }
            if (normalized == EmployeeWorkStatus.Official || normalized.Contains("CHÍNH THỨC") || normalized.Contains("CHINH THUC"))
            {
                return EmployeeWorkStatus.Official;
            }
            if (normalized == EmployeeWorkStatus.OnLeave || normalized.Contains("NGHỈ PHÉP") || normalized.Contains("NGHI PHEP"))
            {
                return EmployeeWorkStatus.OnLeave;
            }
            if (normalized == EmployeeWorkStatus.Suspended || normalized.Contains("ĐÌNH CHỈ") || normalized.Contains("DINH CHI"))
            {
                return EmployeeWorkStatus.Suspended;
            }
            if (normalized == EmployeeWorkStatus.Resigned || normalized.Contains("NGHỈ VIỆC") || normalized.Contains("NGHI VIEC"))
            {
                return EmployeeWorkStatus.Resigned;
            }
            if (normalized == EmployeeWorkStatus.Retired || normalized.Contains("NGHỈ HƯU") || normalized.Contains("NGHI HUU"))
            {
                return EmployeeWorkStatus.Retired;
            }

            return statusStr.Trim();
        }
    }

    internal sealed class EmployeeExcelColumnDefinition
    {
        public required string Title { get; init; }
        public bool Required { get; init; }
        public bool ExportOnly { get; init; }
    }

    internal static class EmployeeExcelColumns
    {
        public static readonly EmployeeExcelColumnDefinition[] Definitions =
        {
            new() { Title = "Mã nhân viên", Required = true },
            new() { Title = "Họ", Required = true },
            new() { Title = "Tên", Required = true },
            new() { Title = "Họ tên đầy đủ", Required = false },
            new() { Title = "Số điện thoại", Required = true },
            new() { Title = "SĐT phụ", Required = false },
            new() { Title = "Email", Required = true },
            new() { Title = "Email công ty", Required = false },
            new() { Title = "Ngày sinh", Required = true },
            new() { Title = "Quốc tịch", Required = false },
            new() { Title = "Dân tộc", Required = false },
            new() { Title = "Tôn giáo", Required = false },
            new() { Title = "Số CCCD", Required = true },
            new() { Title = "Nơi cấp CCCD", Required = true },
            new() { Title = "Ngày cấp CCCD", Required = true },
            new() { Title = "Địa chỉ thường trú", Required = false },
            new() { Title = "Địa chỉ hiện tại", Required = false },
            new() { Title = "Tỉnh/TP hiện tại", Required = false },
            new() { Title = "Phường/Xã hiện tại", Required = false },
            new() { Title = "Số TK ngân hàng", Required = false },
            new() { Title = "Tên ngân hàng", Required = false },
            new() { Title = "Chi nhánh NH", Required = false },
            new() { Title = "Chủ tài khoản", Required = false },
            new() { Title = "Mã số thuế", Required = false },
            new() { Title = "Số BHXH", Required = false },
            new() { Title = "Số BHYT", Required = false },
            new() { Title = "Cấp bậc", Required = false },
            new() { Title = "Hình thức làm việc", Required = false },
            new() { Title = "Loại hợp đồng", Required = false },
            new() { Title = "Trạng thái NV", Required = false },
            new() { Title = "Ngày vào làm", Required = true },
            new() { Title = "Ngày nghỉ việc", Required = false },
            new() { Title = "Lý do nghỉ việc", Required = false },
            new() { Title = "Mã công ty", Required = false },
            new() { Title = "Mã chi nhánh", Required = false },
            new() { Title = "Mã phòng ban", Required = false },
            new() { Title = "Mã bộ phận", Required = false },
            new() { Title = "Mã chức vụ", Required = false },
            new() { Title = "Trạng thái hệ thống", Required = false, ExportOnly = true },
        };

        public static IEnumerable<EmployeeExcelColumnDefinition> GetColumns(bool includeExportOnlyColumns)
        {
            return Definitions.Where(x => includeExportOnlyColumns || !x.ExportOnly);
        }
    }

    internal static class EmployeeExcelWriter
    {
        public static void WriteHeaders(IXLWorksheet worksheet, bool includeExportOnlyColumns)
        {
            var columns = EmployeeExcelColumns.GetColumns(includeExportOnlyColumns).ToList();

            for (var col = 0; col < columns.Count; col++)
            {
                EmployeeExcelColumnDefinition definition = columns[col];
                ExcelHelper.WriteStyledHeaderCell(worksheet, col + 1, definition.Title, definition.Required);
            }

            worksheet.Row(1).Height = 28;
        }

        public static void WriteTemplateSampleRow(IXLWorksheet worksheet)
        {
            var sampleValues = new List<string>
            {
                "NV0001",
                "Nguyễn",
                "Văn An",
                "Nguyễn Văn An",
                "0901234567",
                "0907654321",
                "an.nguyen@example.com",
                "an.nguyen@company.com",
                "15/05/1995",
                "Việt Nam",
                "Kinh",
                "Không",
                "001200000001",
                "Cục Cảnh sát QLHC về TTXH",
                "01/01/2021",
                "Số 1 Đại Cồ Việt, Hai Bà Trưng, Hà Nội",
                "Số 10 Phạm Hùng, Nam Từ Liêm, Hà Nội",
                "Hà Nội",
                "Mỹ Đình 1",
                "19030000000001",
                "Techcombank",
                "Hà Nội",
                "NGUYEN VAN AN",
                "8000000001",
                "0123456789",
                "DN4010000000001",
                "Nhân viên chính thức",
                "FULL_TIME",
                "CHINH_THUC",
                "WORKING",
                "01/06/2023",
                "",
                "",
                "CT01",
                "CN01",
                "PB01",
                "BP01",
                "CV01"
            };

            for (var col = 0; col < sampleValues.Count; col++)
            {
                IXLCell cell = worksheet.Cell(2, col + 1);
                cell.Value = sampleValues[col];
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.FontColor = XLColor.DarkGray;
            }
        }

        public static void WriteEmployeeRow(
            IXLWorksheet worksheet,
            int row,
            EmployeeEntity employee,
            string? companyCode,
            string? branchCode,
            string? deptCode,
            string? partCode,
            string? posCode,
            bool includeExportOnlyColumns)
        {
            var values = new List<string?>
            {
                employee.Code,
                employee.LastName,
                employee.FirstName,
                employee.FullName,
                employee.Phone,
                employee.SecondaryPhone,
                employee.Email,
                employee.CompanyEmail,
                employee.DayOfBirth.ToString("dd/MM/yyyy"),
                employee.Nationality,
                employee.Ethnicity,
                employee.Religion,
                employee.IdentityCard,
                employee.PlaceOfIsssuance,
                employee.IssuanceDate.ToString("dd/MM/yyyy"),
                employee.PermanentAddress,
                employee.NowAddress,
                employee.CurrentCity,
                employee.CurrentWard,
                employee.BankAccountNumber,
                employee.BankName,
                employee.BankBranchName,
                employee.BankAccountHolder,
                employee.TaxCode,
                employee.SocialInsuranceNumber,
                employee.HealthInsuranceNumber,
                employee.Level,
                employee.WorkingMode,
                employee.ContractType,
                employee.Status,
                employee.JoinDate.ToString("dd/MM/yyyy"),
                employee.ResignationDate?.ToString("dd/MM/yyyy"),
                employee.ResignationReason,
                companyCode,
                branchCode,
                deptCode,
                partCode,
                posCode
            };

            if (includeExportOnlyColumns)
            {
                values.Add(employee.IsDeleted ? "Ngưng hoạt động" : "Đang hoạt động");
            }

            for (var col = 0; col < values.Count; col++)
            {
                IXLCell cell = worksheet.Cell(row, col + 1);
                cell.Value = values[col] ?? string.Empty;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }
    }
}

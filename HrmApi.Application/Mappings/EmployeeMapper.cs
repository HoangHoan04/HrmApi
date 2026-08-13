using HrmApi.Application.DTOs.Employee;
using HrmApi.Domain.Entities.Employee;

namespace HrmApi.Application.Mappings
{
    internal static class EmployeeMapper
    {
        public static EmployeeDto ToDto(EmployeeEntity entity, bool includeChildren = false)
        {
            EmployeeDto dto = new()
            {
                Id = entity.Id,
                Code = entity.Code,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                FullName = entity.FullName,
                Gender = entity.Gender,
                AvatarUrl = entity.AvatarUrl,
                Phone = entity.Phone,
                SecondaryPhone = entity.SecondaryPhone,
                Email = entity.Email,
                CompanyEmail = entity.CompanyEmail,
                DayOfBirth = entity.DayOfBirth,
                Nationality = entity.Nationality,
                Ethnicity = entity.Ethnicity,
                Religion = entity.Religion,
                IdentityCard = entity.IdentityCard,
                PlaceOfIsssuance = entity.PlaceOfIsssuance,
                IssuanceDate = entity.IssuanceDate,
                PermanentAddress = entity.PermanentAddress,
                NowAddress = entity.NowAddress,
                CurrentCity = entity.CurrentCity,
                CurrentWard = entity.CurrentWard,
                BankAccountNumber = entity.BankAccountNumber,
                Bankname = entity.BankName,
                BankBranchName = entity.BankBranchName,
                BankAccountHolder = entity.BankAccountHolder,
                TaxCode = entity.TaxCode,
                SocialInsuranceNumber = entity.SocialInsuranceNumber,
                HealthInsuranceNumber = entity.HealthInsuranceNumber,
                Level = entity.Level,
                WorkingMode = entity.WorkingMode,
                ContractType = entity.ContractType,
                Status = entity.Status,
                JoinDate = entity.JoinDate,
                ResignationDate = entity.ResignationDate,
                ResignationReason = entity.ResignationReason,
                CompanyId = entity.CompanyId,
                BranchId = entity.BranchId,
                DepartmentId = entity.DepartmentId,
                PartId = entity.PartId,
                PositionId = entity.PositionId,
                DirectManagerId = entity.DirectManagerId,
                DirectManagerName = entity.DirectManager?.FullName
                    ?? (entity.DirectManager != null
                        ? $"{entity.DirectManager.LastName} {entity.DirectManager.FirstName}".Trim()
                        : null),
                DirectManagerCode = entity.DirectManager?.Code,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };

            if (includeChildren)
            {
                dto.Dependents = entity.Dependents
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.FullName)
                    .Select(ToDependentDto)
                    .ToList();

                dto.Educations = entity.Educations
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.EndDate)
                    .Select(ToEducationDto)
                    .ToList();

                dto.Certificates = entity.Certificates
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.IssueDate)
                    .Select(ToCertificateDto)
                    .ToList();

                dto.Files = entity.Files
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(ToFileDto)
                    .ToList();

                dto.SalaryHistories = entity.SalaryHistories
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.EffectiveDate)
                    .Select(ToSalaryHistoryDto)
                    .ToList();
            }

            return dto;
        }

        public static void ApplyCommandFields(EmployeeEntity entity, EmployeeCommandFields fields)
        {
            entity.Code = fields.Code.Trim();
            entity.FirstName = fields.FirstName.Trim();
            entity.LastName = fields.LastName.Trim();
            entity.FullName = BuildFullName(fields.FirstName, fields.LastName, fields.FullName);
            entity.Gender = TrimOrNull(fields.Gender);
            entity.AvatarUrl = TrimOrNull(fields.AvatarUrl);
            entity.Phone = fields.Phone.Trim();
            entity.SecondaryPhone = TrimOrNull(fields.SecondaryPhone);
            entity.Email = fields.Email.Trim();
            entity.CompanyEmail = TrimOrNull(fields.CompanyEmail);
            entity.DayOfBirth = fields.DayOfBirth;
            entity.Nationality = TrimOrNull(fields.Nationality);
            entity.Ethnicity = TrimOrNull(fields.Ethnicity);
            entity.Religion = TrimOrNull(fields.Religion);
            entity.IdentityCard = fields.IdentityCard.Trim();
            entity.PlaceOfIsssuance = fields.PlaceOfIsssuance.Trim();
            entity.IssuanceDate = fields.IssuanceDate;
            entity.PermanentAddress = TrimOrNull(fields.PermanentAddress);
            entity.NowAddress = TrimOrNull(fields.NowAddress);
            entity.CurrentCity = TrimOrNull(fields.CurrentCity);
            entity.CurrentWard = TrimOrNull(fields.CurrentWard);
            entity.BankAccountNumber = TrimOrNull(fields.BankAccountNumber);
            entity.BankName = TrimOrNull(fields.Bankname);
            entity.BankBranchName = TrimOrNull(fields.BankBranchName);
            entity.BankAccountHolder = TrimOrNull(fields.BankAccountHolder);
            entity.TaxCode = TrimOrNull(fields.TaxCode);
            entity.SocialInsuranceNumber = TrimOrNull(fields.SocialInsuranceNumber);
            entity.HealthInsuranceNumber = TrimOrNull(fields.HealthInsuranceNumber);
            entity.Level = TrimOrNull(fields.Level);
            entity.WorkingMode = TrimOrNull(fields.WorkingMode);
            entity.ContractType = TrimOrNull(fields.ContractType);
            entity.Status = TrimOrNull(fields.Status);
            entity.JoinDate = fields.JoinDate;
            entity.ResignationDate = fields.ResignationDate;
            entity.ResignationReason = TrimOrNull(fields.ResignationReason);
            entity.CompanyId = fields.CompanyId;
            entity.BranchId = fields.BranchId;
            entity.DepartmentId = fields.DepartmentId;
            entity.PartId = fields.PartId;
            entity.PositionId = fields.PositionId;
            entity.DirectManagerId = fields.DirectManagerId.HasValue && fields.DirectManagerId != Guid.Empty
                ? fields.DirectManagerId
                : null;
        }

        public static object ToLogObject(EmployeeEntity entity)
        {
            return new
            {
                entity.Code,
                entity.FirstName,
                entity.LastName,
                entity.FullName,
                entity.Gender,
                entity.AvatarUrl,
                entity.Phone,
                entity.SecondaryPhone,
                entity.Email,
                entity.CompanyEmail,
                entity.DayOfBirth,
                entity.Nationality,
                entity.Ethnicity,
                entity.Religion,
                entity.IdentityCard,
                entity.PlaceOfIsssuance,
                entity.IssuanceDate,
                entity.PermanentAddress,
                entity.NowAddress,
                entity.CurrentCity,
                entity.CurrentWard,
                entity.BankAccountNumber,
                entity.BankName,
                entity.BankBranchName,
                entity.BankAccountHolder,
                entity.TaxCode,
                entity.SocialInsuranceNumber,
                entity.HealthInsuranceNumber,
                entity.Level,
                entity.WorkingMode,
                entity.ContractType,
                entity.Status,
                entity.JoinDate,
                entity.ResignationDate,
                entity.ResignationReason,
                entity.DirectManagerId,
                entity.CompanyId,
                entity.BranchId,
                entity.DepartmentId,
                entity.PartId,
                entity.PositionId
            };
        }

        public static EmployeeDependentDto ToDependentDto(EmployeeDependentEntity entity)
        {
            return new()
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                FullName = entity.FullName,
                Relationship = entity.Relationship,
                DayOfBirth = entity.DayOfBirth,
                Gender = entity.Gender,
                IdentityNumber = entity.IdentityNumber,
                TaxCode = entity.TaxCode,
                DependentFromDate = entity.DependentFromDate,
                DependentToDate = entity.DependentToDate,
                Status = entity.Status,
                Note = entity.Note,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static EmployeeEducationDto ToEducationDto(EmployeeEducationEntity entity)
        {
            return new()
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                SchoolName = entity.SchoolName,
                Degree = entity.Degree,
                Major = entity.Major,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Gpa = entity.Gpa,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static EmployeeCertificateDto ToCertificateDto(EmployeeCertificateEntity entity)
        {
            return new()
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                Name = entity.Name,
                IssuingOrganization = entity.IssuingOrganization,
                IssueDate = entity.IssueDate,
                ExpiryDate = entity.ExpiryDate,
                CredentialId = entity.CredentialId,
                ImageUrl = entity.ImageUrl,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static EmployeeFileDto ToFileDto(EmployeeFileEntity entity)
        {
            var today = DateTime.UtcNow.Date;
            return new()
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                FileCategory = entity.FileCategory,
                FileName = entity.FileName,
                FileUrl = entity.FileUrl,
                ContentType = entity.ContentType,
                FileSize = entity.FileSize,
                Description = entity.Description,
                ExpiryDate = entity.ExpiryDate,
                VersionNo = entity.VersionNo <= 0 ? 1 : entity.VersionNo,
                ReplacesFileId = entity.ReplacesFileId,
                IsCurrent = entity.IsCurrent,
                IsExpired = entity.ExpiryDate.HasValue && entity.ExpiryDate.Value.Date < today,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static EmployeeSalaryHistoryDto ToSalaryHistoryDto(EmployeeSalaryHistoryEntity entity)
        {
            return new()
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EffectiveDate = entity.EffectiveDate,
                OldBasicSalary = entity.OldBasicSalary,
                NewBasicSalary = entity.NewBasicSalary,
                Allowance = entity.Allowance,
                ChangeType = entity.ChangeType,
                Reason = entity.Reason,
                DecisionNumber = entity.DecisionNumber,
                ApprovedBy = entity.ApprovedBy,
                Note = entity.Note,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static string BuildFullName(string firstName, string lastName, string? fullName)
        {
            return !string.IsNullOrWhiteSpace(fullName) ? fullName.Trim() : $"{firstName.Trim()} {lastName.Trim()}".Trim();
        }

        private static string? TrimOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public class EmployeeCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? SecondaryPhone { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? CompanyEmail { get; set; }
        public DateTime DayOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? Ethnicity { get; set; }
        public string? Religion { get; set; }
        public string IdentityCard { get; set; } = string.Empty;
        public string PlaceOfIsssuance { get; set; } = string.Empty;
        public DateTime IssuanceDate { get; set; }
        public string? PermanentAddress { get; set; }
        public string? NowAddress { get; set; }
        public string? CurrentCity { get; set; }
        public string? CurrentWard { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? Bankname { get; set; }
        public string? BankBranchName { get; set; }
        public string? BankAccountHolder { get; set; }
        public string? TaxCode { get; set; }
        public string? SocialInsuranceNumber { get; set; }
        public string? HealthInsuranceNumber { get; set; }
        public string? Level { get; set; }
        public string? WorkingMode { get; set; }
        public string? ContractType { get; set; }
        public string? Status { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? ResignationDate { get; set; }
        public string? ResignationReason { get; set; }

        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? DirectManagerId { get; set; }
    }
}

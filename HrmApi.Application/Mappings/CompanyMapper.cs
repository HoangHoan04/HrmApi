using HrmApi.Application.DTOs.Company;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Application.Mappings
{
    internal static class CompanyMapper
    {
        public static CompanyDto ToDto(CompanyEntity entity, string? parentName = null)
        {
            return new CompanyDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                Address = entity.Address,
                TaxCode = entity.TaxCode,
                Hotline = entity.Hotline,
                PrefixMaleCode = entity.PrefixMaleCode,
                PrefixFemaleCode = entity.PrefixFemaleCode,
                PrefixFullTimeCode = entity.PrefixFullTimeCode,
                PrefixPartTimeCode = entity.PrefixPartTimeCode,
                ParentId = entity.ParentId,
                ParentName = parentName,
                DayComputeSalary = entity.DayComputeSalary,
                IsComputePrevMonth = entity.IsComputePrevMonth,
                Email = entity.Email,
                Website = entity.Website,
                Fax = entity.Fax,
                Country = entity.Country,
                City = entity.City,
                District = entity.District,
                Ward = entity.Ward,
                BusinessRegistrationCode = entity.BusinessRegistrationCode,
                FoundedDate = entity.FoundedDate,
                OperatingStatus = entity.OperatingStatus,
                LegalRepresentative = entity.LegalRepresentative,
                LegalRepresentativePosition = entity.LegalRepresentativePosition,
                CompanyType = entity.CompanyType,
                Industry = entity.Industry,
                BankAccountNumber = entity.BankAccountNumber,
                BankName = entity.BankName,
                BankBranch = entity.BankBranch,
                TimeZone = entity.TimeZone,
                DefaultLanguage = entity.DefaultLanguage,
                LogoUrl = entity.LogoUrl,
                IsActive = entity.IsActive,
                SocialInsuranceCode = entity.SocialInsuranceCode,
                TimeKeepingStandardId = entity.TimeKeepingStandardId,
                SaturdayPolicy = entity.SaturdayPolicy,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static void ApplyCommandFields(CompanyEntity entity, CompanyCommandFields fields)
        {
            entity.Code = fields.Code.Trim();
            entity.Name = fields.Name.Trim();
            entity.Description = TrimOrNull(fields.Description);
            entity.Address = TrimOrNull(fields.Address);
            entity.TaxCode = TrimOrNull(fields.TaxCode);
            entity.Hotline = TrimOrNull(fields.Hotline);
            entity.PrefixMaleCode = TrimOrNull(fields.PrefixMaleCode);
            entity.PrefixFemaleCode = TrimOrNull(fields.PrefixFemaleCode);
            entity.PrefixFullTimeCode = TrimOrNull(fields.PrefixFullTimeCode);
            entity.PrefixPartTimeCode = TrimOrNull(fields.PrefixPartTimeCode);
            entity.ParentId = fields.ParentId;
            entity.DayComputeSalary = fields.DayComputeSalary;
            entity.IsComputePrevMonth = fields.IsComputePrevMonth;
            entity.Email = TrimOrNull(fields.Email);
            entity.Website = TrimOrNull(fields.Website);
            entity.Fax = TrimOrNull(fields.Fax);
            entity.Country = TrimOrNull(fields.Country);
            entity.City = TrimOrNull(fields.City);
            entity.District = TrimOrNull(fields.District);
            entity.Ward = TrimOrNull(fields.Ward);
            entity.BusinessRegistrationCode = TrimOrNull(fields.BusinessRegistrationCode);
            entity.FoundedDate = fields.FoundedDate;
            entity.OperatingStatus = TrimOrNull(fields.OperatingStatus);
            entity.LegalRepresentative = TrimOrNull(fields.LegalRepresentative);
            entity.LegalRepresentativePosition = TrimOrNull(fields.LegalRepresentativePosition);
            entity.CompanyType = TrimOrNull(fields.CompanyType);
            entity.Industry = TrimOrNull(fields.Industry);
            entity.BankAccountNumber = TrimOrNull(fields.BankAccountNumber);
            entity.BankName = TrimOrNull(fields.BankName);
            entity.BankBranch = TrimOrNull(fields.BankBranch);
            entity.TimeZone = TrimOrNull(fields.TimeZone);
            entity.DefaultLanguage = TrimOrNull(fields.DefaultLanguage);
            entity.LogoUrl = TrimOrNull(fields.LogoUrl);
            entity.IsActive = fields.IsActive;
            entity.SocialInsuranceCode = TrimOrNull(fields.SocialInsuranceCode);
            entity.TimeKeepingStandardId = fields.TimeKeepingStandardId;
            if (fields.SaturdayPolicy.HasValue)
            {
                entity.SaturdayPolicy = fields.SaturdayPolicy.Value;
            }
        }

        public static object ToLogObject(CompanyEntity entity)
        {
            return new
            {
                entity.Code,
                entity.Name,
                entity.Description,
                entity.Address,
                entity.TaxCode,
                entity.Hotline,
                entity.PrefixMaleCode,
                entity.PrefixFemaleCode,
                entity.PrefixFullTimeCode,
                entity.PrefixPartTimeCode,
                entity.ParentId,
                entity.DayComputeSalary,
                entity.IsComputePrevMonth,
                entity.Email,
                entity.Website,
                entity.Fax,
                entity.Country,
                entity.City,
                entity.District,
                entity.Ward,
                entity.BusinessRegistrationCode,
                entity.FoundedDate,
                entity.OperatingStatus,
                entity.LegalRepresentative,
                entity.LegalRepresentativePosition,
                entity.CompanyType,
                entity.Industry,
                entity.BankAccountNumber,
                entity.BankName,
                entity.BankBranch,
                entity.TimeZone,
                entity.DefaultLanguage,
                entity.LogoUrl,
                entity.IsActive,
                entity.SocialInsuranceCode,
                entity.TimeKeepingStandardId
            };
        }

        private static string? TrimOrNull(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public class CompanyCommandFields
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public string? Hotline { get; set; }
        public string? PrefixMaleCode { get; set; }
        public string? PrefixFemaleCode { get; set; }
        public string? PrefixFullTimeCode { get; set; }
        public string? PrefixPartTimeCode { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime? DayComputeSalary { get; set; }
        public bool? IsComputePrevMonth { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Fax { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? BusinessRegistrationCode { get; set; }
        public DateTime? FoundedDate { get; set; }
        public string? OperatingStatus { get; set; }
        public string? LegalRepresentative { get; set; }
        public string? LegalRepresentativePosition { get; set; }
        public string? CompanyType { get; set; }
        public string? Industry { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankBranch { get; set; }
        public string? TimeZone { get; set; }
        public string? DefaultLanguage { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public string? SocialInsuranceCode { get; set; }
        public Guid? TimeKeepingStandardId { get; set; }
        public HrmApi.Domain.Enums.SaturdayPolicy? SaturdayPolicy { get; set; }
    }
}

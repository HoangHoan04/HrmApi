namespace HrmApi.Application.DTOs.Company
{
    public class CompanyDto : BaseDto
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
        public string? ParentName { get; set; }
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
        public string? PrimaryColor { get; set; }
        public bool IsActive { get; set; } = true;
        public string? SocialInsuranceCode { get; set; }
        public Guid? TimeKeepingStandardId { get; set; }
        public HrmApi.Domain.Enums.SaturdayPolicy SaturdayPolicy { get; set; } = HrmApi.Domain.Enums.SaturdayPolicy.Work;
        public int? MaxEmployeeCapacity { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
    public class CompanySelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

}

namespace HrmApi.Application.DTOs.ContractType
{
    public class ContractTypeDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public List<Guid> CompanyIds { get; set; } = [];
        public List<string> CompanyNames { get; set; } = [];
        public bool IsProbation { get; set; }
        public bool IsUnlimited { get; set; }
        public int? DefaultDurationMonths { get; set; }
        public int? MaxRenewalTimes { get; set; }
        public int? NotifyBeforeExpiryDays { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public class ContractTypeSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public List<Guid> CompanyIds { get; set; } = [];
        public bool IsProbation { get; set; }
        public bool IsUnlimited { get; set; }
        public int? DefaultDurationMonths { get; set; }
        public int? MaxRenewalTimes { get; set; }
        public int? NotifyBeforeExpiryDays { get; set; }
    }
}

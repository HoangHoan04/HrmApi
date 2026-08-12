namespace HrmApi.Application.DTOs.ReviewRenewal
{
    public class ReviewRenewalDto : BaseDto
    {
        public Guid ContractId { get; set; }
        public string? ContractCode { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? ReviewedBy { get; set; }
        public decimal? PerformanceScore { get; set; }
        public string? ReviewResult { get; set; }
        public string? ReviewComment { get; set; }
        public string? Recommendation { get; set; }
        public Guid? ProposedContractTypeId { get; set; }
        public string? ProposedContractTypeName { get; set; }
        public DateTime? ProposedStartDate { get; set; }
        public DateTime? ProposedEndDate { get; set; }
        public decimal? ProposedBasicSalary { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? Status { get; set; }
        public Guid? NewContractId { get; set; }
        public string? NewContractCode { get; set; }
        public string? Note { get; set; }
        public DateTime? ContractEndDate { get; set; }
    }
}

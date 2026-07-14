using System;

namespace HrmApi.Application.DTOs
{
    public class BranchDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string GroupSalary { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

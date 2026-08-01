using System;

namespace HrmApi.Application.DTOs.Company
{
    public class CompanySelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

using System;

namespace HrmApi.Application.DTOs.Employee
{
    public class EmployeeSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

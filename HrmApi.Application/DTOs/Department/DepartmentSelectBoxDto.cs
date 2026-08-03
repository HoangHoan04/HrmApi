using System;

namespace HrmApi.Application.DTOs.Department
{
    public class DepartmentSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

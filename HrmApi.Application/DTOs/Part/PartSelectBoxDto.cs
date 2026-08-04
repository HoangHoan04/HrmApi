using System;

namespace HrmApi.Application.DTOs.Part
{
    public class PartSelectBoxDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PartMasterId { get; set; }
    }
}
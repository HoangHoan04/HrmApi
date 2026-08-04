using System;

namespace HrmApi.Application.DTOs.Position
{
    public class PositionSelectBoxDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? PositionMasterId { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}

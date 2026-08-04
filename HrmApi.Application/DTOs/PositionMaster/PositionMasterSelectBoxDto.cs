using System;

namespace HrmApi.Application.DTOs.PositionMaster
{
    public class PositionMasterSelectBoxDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
    }
}

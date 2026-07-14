using HrmApi.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Xml.Linq;

namespace HrmApi.Domain.Entities.Organization
{
    /* Phòng ban trong mỗi chi nhánh */
    public class DepartmentEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Limit { get; set; }

        public Guid? CompanyId { get; set; }

        public Guid? BranchId { get; set; }

        public bool IsNotifyMarketing { get; set; }

        public string? Type { get; set; }
    }
}

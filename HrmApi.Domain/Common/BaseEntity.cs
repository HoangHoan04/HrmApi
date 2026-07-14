using System;
using System.Collections.Generic;
using System.Text;

namespace HrmApi.Domain.Common
{
    public class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CreatedBy { get; set; } = Guid.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? UpdatedBy { get; set; } = null;

        public DateTime? UpdatedAt { get; set; } = null;

        public bool IsDeleted { get; set; }
    }
}

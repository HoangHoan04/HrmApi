using HrmApi.Domain.Common;
using System;

namespace HrmApi.Domain.Entities.Permission
{
    public class UserRoleEntity : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        /* Role có hiệu lực từ ngày nào - hữu ích khi phân quyền tạm thời (kiêm nhiệm) */
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public UserEntity User { get; set; } = null!;
        public RoleEntity Role { get; set; } = null!;
    }
}
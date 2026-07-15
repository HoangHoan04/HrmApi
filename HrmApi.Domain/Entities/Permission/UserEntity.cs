using HrmApi.Domain.Common;
using System;
using System.Collections.Generic;

namespace HrmApi.Domain.Entities.Permission
{
    /* Tài khoản đăng nhập hệ thống */
    public class UserEntity : BaseEntity
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        /* Loại tài khoản: EMPLOYEE / ADMIN / API_SERVICE */
        public string Type { get; set; } = "EMPLOYEE";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;
        public DateTime? LockedUntil { get; set; }
        /* Số lần đăng nhập sai liên tiếp - dùng để tự khóa tài khoản */
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        /* Bắt buộc đổi mật khẩu ở lần đăng nhập kế tiếp */
        public bool MustChangePassword { get; set; } = false;
        public DateTime? PasswordChangedAt { get; set; }
        public string? ResetPasswordOtp { get; set; }
        public DateTime? ResetPasswordOtpExpiresAt { get; set; }
        public string? FcmToken { get; set; }
        public string? FcmTokenMobile { get; set; }
        public List<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
        public List<UserTokenEntity> UserTokens { get; set; } = new List<UserTokenEntity>();
    }
}
using HrmApi.Domain.Common;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Organization;

namespace HrmApi.Domain.Entities.Permission
{
    /// <summary>
    /// Tài khoản đăng nhập hệ thống
    /// </summary>
    public class UserEntity : BaseEntity
    {
        /// <summary>
        /// Công ty trực thuộc
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Chi nhánh trực thuộc
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Mã nhân viên
        /// </summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu đăng nhập
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Loại tài khoản
        /// </summary>
        public string Type { get; set; } = "EMPLOYEE";

        /// <summary>
        /// Email nhân viên (Nếu là nhân viên)
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Số điện thoại nhân viên
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Có bị khoá tài khoản không
        /// </summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Khoá tài khoản cho đến khi ?
        /// </summary>
        public DateTime? LockedUntil { get; set; }

        /// <summary>
        /// Số lần đăng nhập sai liên tiếp - dùng để tự khóa tài khoản
        /// </summary>
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>
        /// Lần đăng nhập cuối
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        public string? LastLoginIp { get; set; }

        /// <summary>
        /// /Bắt buộc đổi mật khẩu ở lần đăng nhập kế tiếp
        /// </summary>
        public bool MustChangePassword { get; set; } = false;

        /// <summary>
        /// Mật khẩu mới
        /// </summary>
        public DateTime? PasswordChangedAt { get; set; }

        /// <summary>
        /// Mã OTP đổi mật khẩu
        /// </summary>
        public string? ResetPasswordOtp { get; set; }

        /// <summary>
        /// Thời gian hết hạn OTP
        /// </summary>
        public DateTime? ResetPasswordOtpExpiresAt { get; set; }

        /// <summary>
        /// Token gửi FireBase
        /// </summary>
        public string? FcmToken { get; set; }
        /// <summary>
        /// Token gửi FireBase cho mobile
        /// </summary>
        public string? FcmTokenMobile { get; set; }
        /// <summary>
        /// Danh sách vai trò của người dùng
        /// </summary>
        public List<UserRoleEntity> UserRoles { get; set; } = [];
        /// <summary>
        /// Danh sách token của người dùng
        /// </summary>
        public List<UserTokenEntity> UserTokens { get; set; } = [];
        /// <summary>
        /// Navigation property tới công ty sở hữu tài khoản này
        /// </summary>
        public virtual CompanyEntity? Company { get; set; }
        /// <summary>
        /// Navigation property tới chi nhánh sở hữu tài khoản này
        /// </summary>
        public virtual BranchEntity? Branch { get; set; }
        /// <summary>
        /// Navigation property tới nhân viên sở hữu tài khoản này
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }
    }
}
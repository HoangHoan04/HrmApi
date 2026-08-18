using System;
using System.Collections.Generic;

namespace HrmApi.Application.DTOs.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? FullName { get; set; }
        public bool MustChangePassword { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string? TempToken { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public List<string> Roles { get; set; } = [];
        public List<string> Permissions { get; set; } = [];
    }

    public class AuthMeDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? FullName { get; set; }
        public string Type { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public List<string> Roles { get; set; } = [];
        public List<string> Permissions { get; set; } = [];
        public bool TwoFactorEnabled { get; set; }
    }

    public class TwoFactorSetupResponse
    {
        public string Secret { get; set; } = string.Empty;
        public string OtpAuthUri { get; set; } = string.Empty;
    }

    public class TwoFactorCodeRequest
    {
        public string Code { get; set; } = string.Empty;
        public string? Password { get; set; }
    }

    public class TwoFactorVerifyRequest
    {
        public string TempToken { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class SsoStartResponse
    {
        public string AuthorizeUrl { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }

    public class SsoCallbackRequest
    {
        public string Code { get; set; } = string.Empty;
        public string? RedirectUri { get; set; }
    }

    public class SsoStatusResponse
    {
        public SsoProviderStatusDto Google { get; set; } = new();
        public SsoProviderStatusDto Microsoft { get; set; } = new();
    }

    public class SsoProviderStatusDto
    {
        public bool Enabled { get; set; }
        public bool Configured { get; set; }
        public string? ClientIdMasked { get; set; }
    }

    public class SessionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class SessionListRequest
    {
        public bool? IncludeRevoked { get; set; }
        public bool? AllUsers { get; set; }
    }

    public class SessionRevokeRequest
    {
        public Guid Id { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordWithOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
    public class MobileProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PhoneNumber { get; set; }
        public string Type { get; set; } = string.Empty;

        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? PermanentAddress { get; set; }
        public string? CompanyEmail { get; set; }

        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? Company { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? Branch { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Department { get; set; }
        public Guid? PartId { get; set; }
        public string? PartName { get; set; }
        public string? Part { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? Position { get; set; }

        public string? JoinDate { get; set; }
        public string? Status { get; set; }
        public string? Level { get; set; }
        public string? WorkingMode { get; set; }
        public string? ContractType { get; set; }

        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }

        public List<string> Roles { get; set; } = [];
        public List<string> Permissions { get; set; } = [];
        public bool TwoFactorEnabled { get; set; }

        public MobileProfileStatsDto Stats { get; set; } = new();
    }

    public class MobileProfileStatsDto
    {
        public int? WorkDaysThisMonth { get; set; }
        public int? LeaveDaysRemaining { get; set; }
        public int? LeaveDaysThisMonth { get; set; }
        public int? OnTimeDays { get; set; }
        public int? LateDays { get; set; }
        public int? AbsentDays { get; set; }
    }
}

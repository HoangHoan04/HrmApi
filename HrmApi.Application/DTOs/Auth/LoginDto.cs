using System;

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
        public bool MustChangePassword { get; set; }
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

namespace HrmApi.Application.Common.Interfaces;

/// <summary>
/// Service gọi sang AuthApi để tạo/cập nhật tài khoản người dùng khi HRM tạo nhân viên.
/// </summary>
public interface IAuthProvisioningService
{
    /// <summary>
    /// Tạo mới tài khoản Auth cho nhân viên vừa được thêm vào HRM.
    /// Trả về UserId từ Auth (Guid.Empty nếu thất bại không nghiêm trọng).
    /// </summary>
    Task<Guid> ProvisionEmployeeAccountAsync(
        string email,
        string fullName,
        string? phone,
        Guid? companyId,
        CancellationToken cancellationToken = default);
}

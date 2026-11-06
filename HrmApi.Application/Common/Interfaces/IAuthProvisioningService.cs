namespace HrmApi.Application.Common.Interfaces;

/// <summary>
/// Tạo tài khoản đăng nhập (UserEntities) ngay trong DB HRM khi thêm nhân viên mới.
/// </summary>
public interface IAuthProvisioningService
{
    /// <summary>
    /// Tạo (hoặc liên kết nếu đã tồn tại) tài khoản đăng nhập cho nhân viên.
    /// Username = email; mật khẩu mặc định lấy từ AuthSettings:DefaultEmployeePassword, bắt buộc đổi ở lần đăng nhập đầu.
    /// Trả về UserId (Guid.Empty nếu không tạo được — không chặn luồng tạo nhân viên).
    /// </summary>
    Task<Guid> ProvisionEmployeeAccountAsync(
        Guid employeeId,
        string email,
        string? phone,
        Guid? companyId,
        Guid? branchId,
        CancellationToken cancellationToken = default);
}

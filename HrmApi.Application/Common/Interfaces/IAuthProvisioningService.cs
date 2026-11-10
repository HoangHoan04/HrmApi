namespace HrmApi.Application.Common.Interfaces;

/// <summary>
/// Tạo / đồng bộ tài khoản đăng nhập (UserEntities) cho nhân viên.
/// Quy ước đăng nhập: tên đăng nhập = EMAIL CÔNG TY của nhân viên (EmployeeEntity.CompanyEmail), mật khẩu riêng.
/// </summary>
public interface IAuthProvisioningService
{
    /// <summary>
    /// Tạo (hoặc liên kết nếu đã tồn tại) tài khoản đăng nhập cho nhân viên, dùng email công ty làm tên đăng nhập.
    /// Mật khẩu mặc định lấy từ AuthSettings:DefaultEmployeePassword, bắt buộc đổi ở lần đăng nhập đầu.
    /// Trả về UserId (Guid.Empty nếu không tạo được — không chặn luồng tạo nhân viên).
    /// </summary>
    Task<Guid> ProvisionEmployeeAccountAsync(
        Guid employeeId,
        string companyEmail,
        string? phone,
        Guid? companyId,
        Guid? branchId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gọi sau khi cập nhật nhân viên: nếu email công ty thay đổi thì đổi tên đăng nhập của tài khoản theo;
    /// nếu nhân viên chưa có tài khoản mà đã có email công ty thì tạo mới.
    /// Trả về null nếu thành công / không cần làm gì, ngược lại trả về lý do không đồng bộ được.
    /// </summary>
    Task<string?> SyncEmployeeLoginAsync(Guid employeeId, CancellationToken cancellationToken = default);
}

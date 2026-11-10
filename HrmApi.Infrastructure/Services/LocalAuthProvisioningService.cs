using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HrmApi.Infrastructure.Services;

/// <summary>
/// Tạo tài khoản đăng nhập cho nhân viên trực tiếp trong bảng UserEntities của HRM.
/// Tên đăng nhập (Username) và Email của tài khoản = email công ty của nhân viên (chữ thường).
/// </summary>
public class LocalAuthProvisioningService : IAuthProvisioningService
{
    public const string FallbackDefaultPassword = "Hrm@123456";

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher<UserEntity> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocalAuthProvisioningService> _logger;

    public LocalAuthProvisioningService(
        IApplicationDbContext context,
        IPasswordHasher<UserEntity> passwordHasher,
        IConfiguration configuration,
        ILogger<LocalAuthProvisioningService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    private static string? NormalizeLogin(string? email)
        => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    /// <summary>Có tài khoản KHÁC đang dùng tên đăng nhập / email này không.</summary>
    private Task<bool> IsLoginTakenAsync(string login, Guid? exceptUserId, CancellationToken cancellationToken)
        => _context.UserEntities.AnyAsync(u =>
                !u.IsDeleted
                && (!exceptUserId.HasValue || u.Id != exceptUserId.Value)
                && (u.Username.ToLower() == login || (u.Email != null && u.Email.ToLower() == login)),
            cancellationToken);

    public async Task<Guid> ProvisionEmployeeAccountAsync(
        Guid employeeId,
        string companyEmail,
        string? phone,
        Guid? companyId,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        string? username = NormalizeLogin(companyEmail);
        if (username == null)
        {
            _logger.LogWarning("[AuthProvisioning] Nhân viên {EmployeeId} chưa có email công ty, chưa tạo tài khoản.", employeeId);
            return Guid.Empty;
        }

        try
        {
            // Nhân viên đã có tài khoản -> chỉ đồng bộ tên đăng nhập
            UserEntity? linked = await _context.UserEntities
                .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && !u.IsDeleted, cancellationToken);
            if (linked != null)
            {
                await ApplyLoginAsync(linked, username, cancellationToken);
                return linked.Id;
            }

            // Tài khoản cùng email công ty đã tồn tại (chưa gắn nhân viên) -> liên kết lại
            UserEntity? existing = await _context.UserEntities
                .FirstOrDefaultAsync(u => !u.IsDeleted
                    && (u.Username.ToLower() == username || (u.Email != null && u.Email.ToLower() == username)),
                    cancellationToken);
            if (existing != null)
            {
                if (existing.EmployeeId.HasValue && existing.EmployeeId != employeeId)
                {
                    _logger.LogWarning("[AuthProvisioning] Email công ty {Username} đã gắn với nhân viên khác, bỏ qua.", username);
                    return Guid.Empty;
                }

                existing.EmployeeId = employeeId;
                existing.CompanyId ??= companyId;
                existing.BranchId ??= branchId;
                existing.Username = username;
                existing.Email = username;
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return existing.Id;
            }

            string? defaultPassword = _configuration["AuthSettings:DefaultEmployeePassword"];
            if (string.IsNullOrWhiteSpace(defaultPassword))
            {
                defaultPassword = FallbackDefaultPassword;
            }

            UserEntity user = new()
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = username,
                PhoneNumber = phone,
                Type = UserType.Employee,
                EmployeeId = employeeId,
                CompanyId = companyId,
                BranchId = branchId,
                IsActive = true,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow,
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, defaultPassword);

            _context.UserEntities.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[AuthProvisioning] Đã tạo tài khoản {Username} cho nhân viên {EmployeeId}.", username, employeeId);
            return user.Id;
        }
        catch (Exception ex)
        {
            // Không để lỗi tạo tài khoản chặn việc tạo nhân viên
            _logger.LogError(ex, "[AuthProvisioning] Lỗi tạo tài khoản cho {Email}", companyEmail);
            return Guid.Empty;
        }
    }

    public async Task<string?> SyncEmployeeLoginAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        EmployeeEntity? employee = await _context.EmployeeEntities
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
        if (employee == null || employee.IsDeleted)
        {
            return null;
        }

        string? login = NormalizeLogin(employee.CompanyEmail);

        UserEntity? user = await _context.UserEntities
            .FirstOrDefaultAsync(u => !u.IsDeleted
                && (u.EmployeeId == employee.Id || (employee.UserId.HasValue && u.Id == employee.UserId.Value)),
                cancellationToken);

        if (user == null)
        {
            if (login == null) return null;

            Guid userId = await ProvisionEmployeeAccountAsync(
                employee.Id, login, employee.Phone, employee.CompanyId, employee.BranchId, cancellationToken);
            if (userId == Guid.Empty)
            {
                return $"Không tạo được tài khoản đăng nhập: email công ty {login} đã được dùng cho tài khoản khác.";
            }

            employee.UserId = userId;
            await _context.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (login == null)
        {
            // Xoá email công ty -> giữ nguyên tài khoản cũ để nhân viên không bị mất quyền đăng nhập.
            return null;
        }

        return await ApplyLoginAsync(user, login, cancellationToken);
    }

    /// <summary>Đặt tên đăng nhập + email tài khoản = email công ty (nếu chưa ai dùng).</summary>
    private async Task<string?> ApplyLoginAsync(UserEntity user, string login, CancellationToken cancellationToken)
    {
        bool same = string.Equals(user.Username, login, StringComparison.OrdinalIgnoreCase)
            && string.Equals(user.Email, login, StringComparison.OrdinalIgnoreCase);
        if (same) return null;

        if (await IsLoginTakenAsync(login, user.Id, cancellationToken))
        {
            _logger.LogWarning("[AuthProvisioning] Không đổi tên đăng nhập sang {Login}: đã có tài khoản khác dùng.", login);
            return $"Email công ty {login} đã được dùng cho tài khoản đăng nhập khác.";
        }

        user.Username = login;
        user.Email = login;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return null;
    }
}

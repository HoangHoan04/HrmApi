using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HrmApi.Infrastructure.Services;

/// <summary>
/// Tạo tài khoản đăng nhập cho nhân viên trực tiếp trong bảng UserEntities của HRM.
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

    public async Task<Guid> ProvisionEmployeeAccountAsync(
        Guid employeeId,
        string email,
        string? phone,
        Guid? companyId,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Guid.Empty;
        }

        string username = email.Trim().ToLowerInvariant();

        try
        {
            // Nhân viên đã có tài khoản
            UserEntity? linked = await _context.UserEntities
                .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && !u.IsDeleted, cancellationToken);
            if (linked != null)
            {
                return linked.Id;
            }

            // Tài khoản cùng username/email đã tồn tại (chưa gắn nhân viên) -> liên kết lại
            UserEntity? existing = await _context.UserEntities
                .FirstOrDefaultAsync(u => !u.IsDeleted
                    && (u.Username.ToLower() == username || (u.Email != null && u.Email.ToLower() == username)),
                    cancellationToken);
            if (existing != null)
            {
                if (existing.EmployeeId.HasValue && existing.EmployeeId != employeeId)
                {
                    _logger.LogWarning("[AuthProvisioning] Tài khoản {Username} đã gắn với nhân viên khác, bỏ qua.", username);
                    return Guid.Empty;
                }

                existing.EmployeeId = employeeId;
                existing.CompanyId ??= companyId;
                existing.BranchId ??= branchId;
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return existing.Id;
            }

            string defaultPassword = _configuration["AuthSettings:DefaultEmployeePassword"];
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
            _logger.LogError(ex, "[AuthProvisioning] Lỗi tạo tài khoản cho {Email}", email);
            return Guid.Empty;
        }
    }
}

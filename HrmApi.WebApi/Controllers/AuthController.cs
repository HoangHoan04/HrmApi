using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Features.MobileTimekeeping.Queries;
using HrmApi.Application.Features.RegisterDayOffs.Queries;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Route("api/auth")]
    [Route("api/v1/admin/auth")]
    [Route("api/admin/auth")]
    [Route("api/v1/mobile/auth")]
    [Route("api/mobile/auth")]
    public class AuthController : ControllerBase
    {
        private const int MaxFailedLoginAttempts = 5;
        private const int MaxOtpAttempts = 5;
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);

        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher<UserEntity> _passwordHasher;
        private readonly IAuthContextService _authContextService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IApplicationDbContext context,
            IPasswordHasher<UserEntity> passwordHasher,
            IAuthContextService authContextService,
            IConfiguration configuration,
            IEmailService emailService,
            IMediator mediator,
            ILogger<AuthController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _authContextService = authContextService;
            _configuration = configuration;
            _emailService = emailService;
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var identifier = (!string.IsNullOrWhiteSpace(request.Username) ? request.Username : request.Email)?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Vui lòng nhập email công ty và mật khẩu." });
            }

            // Đăng nhập bằng EMAIL CÔNG TY (tài khoản nhân viên) hoặc tên đăng nhập (tài khoản quản trị, vd "admin")
            var user = await FindUserByLoginAsync(identifier);

            if (user == null)
            {
                return Unauthorized(new { message = "Email công ty hoặc mật khẩu không chính xác." });
            }

            if (!user.IsActive)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên." });
            }

            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            {
                var remainingMinutes = Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
                return StatusCode(StatusCodes.Status423Locked, new { message = $"Tài khoản tạm thời bị khóa do nhiều lần đăng nhập thất bại. Vui lòng thử lại sau {remainingMinutes} phút." });
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                await RegisterFailedLoginAsync(user);
                return Unauthorized(new { message = "Email công ty hoặc mật khẩu không chính xác." });
            }

            if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            }

            // Xác thực 2 bước (TOTP)
            if (user.TwoFactorEnabled && !string.IsNullOrWhiteSpace(user.TwoFactorSecret))
            {
                if (string.IsNullOrWhiteSpace(request.Code2fa))
                {
                    return Ok(new
                    {
                        requiresTwoFactor = true,
                        username = user.Username
                    });
                }

                if (!TotpHelper.VerifyCode(user.TwoFactorSecret, request.Code2fa))
                {
                    await RegisterFailedLoginAsync(user);
                    return Unauthorized(new { message = "Mã xác thực 2 bước không chính xác hoặc đã hết hạn." });
                }
            }

            // Đăng nhập thành công - reset bộ đếm sai
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            var authContext = await _authContextService.LoadAuthContextAsync(user.Id, HttpContext.RequestAborted);
            var roles = authContext.Roles.ToList();
            var permissions = authContext.Permissions.ToList();

            if (user.Type == UserType.Admin && !roles.Contains(RoleCodes.Admin))
            {
                roles.Insert(0, RoleCodes.Admin);
            }
            if (roles.Count == 0)
            {
                roles.Add(user.Type ?? RoleCodes.Employee);
            }

            var (accessToken, expiresInMinutes) = GenerateJwtToken(user, roles, permissions);
            var rawRefreshToken = GenerateSecureToken();
            var refreshTokenHash = HashToken(rawRefreshToken);

            var userToken = new UserTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenHash = refreshTokenHash,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName ?? Request.Headers.UserAgent.ToString(),
                Platform = "Web",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTokenEntities.Add(userToken);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);

            var primaryRole = roles.FirstOrDefault() ?? RoleCodes.Employee;
            var displayName = user.Employee != null ? $"{user.Employee.LastName} {user.Employee.FirstName}".Trim() : user.Username;

            return Ok(new
            {
                token = accessToken,
                accessToken = accessToken,
                refreshToken = rawRefreshToken,
                expiresIn = expiresInMinutes * 60,
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email ?? "",
                    fullName = displayName,
                    role = primaryRole,
                    type = user.Type,
                    employeeId = user.EmployeeId,
                    companyId = user.CompanyId,
                    branchId = user.BranchId,
                    mustChangePassword = user.MustChangePassword
                },
                roles,
                permissions,
                mustChangePassword = user.MustChangePassword
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token không hợp lệ." });
            }

            var hashed = HashToken(request.RefreshToken.Trim());
            var storedToken = await _context.UserTokenEntities
                .Include(t => t.User)
                    .ThenInclude(u => u.Employee)
                .FirstOrDefaultAsync(t => t.RefreshTokenHash == hashed, HttpContext.RequestAborted);

            if (storedToken == null || storedToken.RevokedAt != null || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại." });
            }

            var user = storedToken.User;
            if (!user.IsActive || (user.IsLocked && user.LockedUntil > DateTime.UtcNow))
            {
                return Unauthorized(new { message = "Tài khoản không hợp lệ hoặc đã bị khóa." });
            }

            var authContext = await _authContextService.LoadAuthContextAsync(user.Id, HttpContext.RequestAborted);
            var roles = authContext.Roles.ToList();
            var permissions = authContext.Permissions.ToList();

            if (user.Type == UserType.Admin && !roles.Contains(RoleCodes.Admin))
            {
                roles.Insert(0, RoleCodes.Admin);
            }
            if (roles.Count == 0)
            {
                roles.Add(user.Type ?? RoleCodes.Employee);
            }

            var (accessToken, expiresInMinutes) = GenerateJwtToken(user, roles, permissions);

            // Rotate refresh token
            var newRawRefreshToken = GenerateSecureToken();
            var newRefreshToken = new UserTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenHash = HashToken(newRawRefreshToken),
                DeviceId = storedToken.DeviceId,
                DeviceName = storedToken.DeviceName,
                Platform = storedToken.Platform,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            // Lưu token mới trước, sau đó mới trỏ ReplacedByTokenId (DB có FK self-reference)
            _context.UserTokenEntities.Add(newRefreshToken);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedReason = "Replaced by new token";
            storedToken.ReplacedByTokenId = newRefreshToken.Id;
            await _context.SaveChangesAsync(HttpContext.RequestAborted);

            var primaryRole = roles.FirstOrDefault() ?? RoleCodes.Employee;
            var displayName = user.Employee != null ? $"{user.Employee.LastName} {user.Employee.FirstName}".Trim() : user.Username;

            return Ok(new
            {
                token = accessToken,
                accessToken = accessToken,
                refreshToken = newRawRefreshToken,
                expiresIn = expiresInMinutes * 60,
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email ?? "",
                    fullName = displayName,
                    role = primaryRole,
                    type = user.Type,
                    employeeId = user.EmployeeId,
                    companyId = user.CompanyId,
                    branchId = user.BranchId,
                    mustChangePassword = user.MustChangePassword
                },
                roles,
                permissions
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                var hash = HashToken(request.RefreshToken.Trim());
                var token = await _context.UserTokenEntities.FirstOrDefaultAsync(t => t.RefreshTokenHash == hash, HttpContext.RequestAborted);
                if (token != null && token.RevokedAt == null)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedReason = "User logged out";
                    await _context.SaveChangesAsync(HttpContext.RequestAborted);
                }
            }
            return Ok(new { success = true, message = "Đăng xuất thành công." });
        }

        [Authorize]
        [HttpGet("me")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            }

            var user = await _context.UserEntities
                .Include(u => u.Employee).ThenInclude(e => e!.Company)
                .Include(u => u.Employee).ThenInclude(e => e!.Branch)
                .Include(u => u.Employee).ThenInclude(e => e!.Department)
                .Include(u => u.Employee).ThenInclude(e => e!.Part)
                .Include(u => u.Employee).ThenInclude(e => e!.Position).ThenInclude(p => p!.PositionMaster)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, HttpContext.RequestAborted);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Tài khoản không tồn tại hoặc đã bị khóa." });
            }

            // Một số tài khoản chỉ được liên kết qua EmployeeEntities.UserId
            EmployeeEntity? employee = user.Employee;
            if (employee == null)
            {
                employee = await _context.EmployeeEntities
                    .Include(e => e.Company)
                    .Include(e => e.Branch)
                    .Include(e => e.Department)
                    .Include(e => e.Part)
                    .Include(e => e.Position).ThenInclude(p => p!.PositionMaster)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.UserId == user.Id && !e.IsDeleted, HttpContext.RequestAborted);
            }

            var authContext = await _authContextService.LoadAuthContextAsync(user.Id, HttpContext.RequestAborted);
            var roles = authContext.Roles.ToList();
            var permissions = authContext.Permissions.ToList();

            if (user.Type == UserType.Admin && !roles.Contains(RoleCodes.Admin))
            {
                roles.Insert(0, RoleCodes.Admin);
            }
            if (roles.Count == 0)
            {
                roles.Add(user.Type ?? RoleCodes.Employee);
            }

            var primaryRole = roles.FirstOrDefault() ?? RoleCodes.Employee;
            var displayName = employee != null
                ? (!string.IsNullOrWhiteSpace(employee.FullName)
                    ? employee.FullName!.Trim()
                    : $"{employee.LastName} {employee.FirstName}".Trim())
                : user.Username;

            var stats = employee != null ? await LoadProfileStatsAsync() : null;

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                // email = email cá nhân (liên hệ); loginEmail = email công ty dùng để đăng nhập
                email = FirstNonEmpty(employee?.Email, user.Email) ?? "",
                loginEmail = FirstNonEmpty(employee?.CompanyEmail, user.Username),
                fullName = displayName,
                role = primaryRole,
                type = user.Type,
                employeeId = employee?.Id ?? user.EmployeeId,
                companyId = employee?.CompanyId ?? user.CompanyId,
                branchId = employee?.BranchId ?? user.BranchId,
                mustChangePassword = user.MustChangePassword,
                twoFactorEnabled = user.TwoFactorEnabled,
                roles,
                permissions,

                // Thông tin hồ sơ nhân viên (dùng cho tab Hồ sơ trên mobile)
                employeeCode = employee?.Code,
                firstName = employee?.FirstName,
                lastName = employee?.LastName,
                gender = employee?.Gender,
                avatarUrl = employee?.AvatarUrl,
                phone = FirstNonEmpty(employee?.Phone, user.PhoneNumber),
                phoneNumber = FirstNonEmpty(employee?.Phone, user.PhoneNumber),
                companyEmail = employee?.CompanyEmail,
                dateOfBirth = FormatDate(employee?.DayOfBirth),
                address = FirstNonEmpty(employee?.NowAddress, employee?.PermanentAddress),
                permanentAddress = employee?.PermanentAddress,
                departmentId = employee?.DepartmentId,
                partId = employee?.PartId,
                positionId = employee?.PositionId,
                companyName = employee?.Company?.Name,
                branchName = employee?.Branch?.Name,
                departmentName = employee?.Department?.Name,
                partName = FirstNonEmpty(employee?.Part?.Name, employee?.Part?.Code),
                positionName = employee?.Position?.PositionMaster?.Name,
                joinDate = FormatDate(employee?.JoinDate),
                status = employee?.Status,
                level = employee?.Level,
                workingMode = employee?.WorkingMode,
                contractType = employee?.ContractType,
                bankAccountNumber = employee?.BankAccountNumber,
                bankName = employee?.BankName,
                stats
            });
        }

        /// <summary>
        /// Thống kê tháng hiện tại cho tab Hồ sơ. Dùng lại các query của chấm công / nghỉ phép trên mobile
        /// để số liệu khớp với các tab khác. Lỗi ở đây không được làm hỏng việc tải hồ sơ.
        /// </summary>
        private async Task<object?> LoadProfileStatsAsync()
        {
            int? workDays = null, onTimeDays = null, lateDays = null, absentDays = null, leaveDaysThisMonth = null;
            decimal? leaveRemaining = null;
            var now = DateTime.Now;

            try
            {
                var month = await _mediator.Send(new GetMobileMonthQuery { Year = now.Year, Month = now.Month }, HttpContext.RequestAborted);
                workDays = month.OnTimeDays + month.LateDays + month.EarlyDays;
                onTimeDays = month.OnTimeDays;
                lateDays = month.LateDays;
                absentDays = month.AbsentDays;
                leaveDaysThisMonth = month.LeaveDays;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Profile] Không tải được thống kê chấm công tháng.");
            }

            try
            {
                var balance = await _mediator.Send(new GetMyLeaveBalanceQuery { Year = now.Year }, HttpContext.RequestAborted);
                leaveRemaining = balance.AnnualRemaining;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Profile] Không tải được số ngày phép còn lại.");
            }

            return new
            {
                workDaysThisMonth = workDays,
                onTimeDays,
                lateDays,
                absentDays,
                leaveDaysThisMonth,
                leaveDaysRemaining = leaveRemaining
            };
        }

        private static string? FirstNonEmpty(params string?[] values)
            => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();

        private static string? FormatDate(DateTime? value)
            => value.HasValue && value.Value.Year > 1900 ? value.Value.ToString("dd/MM/yyyy") : null;

        /// <summary>
        /// Nhân viên tự cập nhật thông tin liên hệ (email cá nhân, số điện thoại) và ảnh đại diện từ mobile.
        /// KHÔNG đổi tài khoản đăng nhập: tên đăng nhập luôn là email công ty, do admin quản lý trên hồ sơ nhân viên.
        /// </summary>
        [Authorize]
        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] HrmApi.Application.DTOs.Auth.UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            }

            var user = await _context.UserEntities
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, HttpContext.RequestAborted);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Tài khoản không tồn tại hoặc đã bị khóa." });
            }

            EmployeeEntity? employee = user.Employee
                ?? await _context.EmployeeEntities
                    .FirstOrDefaultAsync(e => e.UserId == user.Id && !e.IsDeleted, HttpContext.RequestAborted);

            string email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
            string phone = (request.PhoneNumber ?? string.Empty).Trim();
            string? avatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Vui lòng nhập email." });
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest(new { message = "Email không hợp lệ." });
            }
            if (phone.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?[0-9 .\-()]{8,20}$"))
            {
                return BadRequest(new { message = "Số điện thoại không hợp lệ." });
            }

            if (phone.Length > 0)
            {
                user.PhoneNumber = phone;
            }
            user.UpdatedAt = DateTime.UtcNow;

            if (employee != null)
            {
                // Email cá nhân chỉ là thông tin liên hệ, không phải tài khoản đăng nhập
                employee.Email = email;
                if (phone.Length > 0) employee.Phone = phone;
                if (avatarUrl != null) employee.AvatarUrl = avatarUrl;
                employee.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(HttpContext.RequestAborted);
            return Ok(new { success = true, message = "Cập nhật thông tin thành công." });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Không xác thực được danh tính." });
            }

            var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == userId, HttpContext.RequestAborted);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            var currentPassword = !string.IsNullOrEmpty(request.CurrentPassword) ? request.CurrentPassword : request.OldPassword ?? string.Empty;
            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verify == PasswordVerificationResult.Failed)
            {
                return BadRequest(new { message = "Mật khẩu hiện tại không chính xác." });
            }

            var passwordError = ValidateNewPassword(request.NewPassword, request.ConfirmNewPassword);
            if (passwordError != null)
            {
                return BadRequest(new { message = passwordError });
            }

            if (request.NewPassword == currentPassword)
            {
                return BadRequest(new { message = "Mật khẩu mới phải khác mật khẩu hiện tại." });
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.MustChangePassword = false;
            user.PasswordChangedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(HttpContext.RequestAborted);
            return Ok(new { success = true, message = "Đổi mật khẩu thành công." });
        }

        // ===================== QUÊN MẬT KHẨU (OTP qua email) =====================

        /// <summary>
        /// Gửi mã OTP 6 số tới email. Luôn trả 200 với cùng thông điệp để không lộ email có tồn tại hay không.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            const string genericMessage = "Nếu email tồn tại trong hệ thống, mã xác thực đã được gửi. Vui lòng kiểm tra hộp thư.";

            var email = request.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Vui lòng nhập email." });
            }

            var user = await FindUserByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                return Ok(new { success = true, message = genericMessage });
            }

            var now = DateTime.UtcNow;
            // Chống spam: chỉ cho gửi lại sau ResendCooldown kể từ lần gửi trước
            if (user.ResetPasswordOtpExpiresAt.HasValue
                && user.ResetPasswordOtpExpiresAt.Value - OtpLifetime + OtpResendCooldown > now)
            {
                return Ok(new { success = true, message = genericMessage });
            }

            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            user.ResetPasswordOtp = $"{HashOtp(user.Id, otp)}|0";
            user.ResetPasswordOtpExpiresAt = now.Add(OtpLifetime);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);

            var to = ResolveEmail(user);
            if (!string.IsNullOrWhiteSpace(to))
            {
                await _emailService.SendEmailAsync(
                    to,
                    "[HRM] Mã xác thực đặt lại mật khẩu",
                    $"""
                    <p>Xin chào <b>{System.Net.WebUtility.HtmlEncode(user.Username)}</b>,</p>
                    <p>Mã xác thực đặt lại mật khẩu của bạn là:</p>
                    <p style="font-size:24px;font-weight:bold;letter-spacing:6px">{otp}</p>
                    <p>Mã có hiệu lực trong {(int)OtpLifetime.TotalMinutes} phút. Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
                    """);
            }

            return Ok(new { success = true, message = genericMessage, expiresInSeconds = (int)OtpLifetime.TotalSeconds });
        }

        /// <summary>
        /// Kiểm tra OTP (không tiêu thụ OTP) — dùng cho bước 2 của màn quên mật khẩu.
        /// </summary>
        [HttpPost("verify-reset-otp")]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyResetOtpRequest request)
        {
            var (user, error) = await ValidateResetOtpAsync(request.Email, request.Otp);
            if (user == null)
            {
                return BadRequest(new { message = error });
            }

            return Ok(new { success = true });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var passwordError = ValidateNewPassword(request.NewPassword, request.ConfirmNewPassword);
            if (passwordError != null)
            {
                return BadRequest(new { message = passwordError });
            }

            var (user, error) = await ValidateResetOtpAsync(request.Email, request.Otp);
            if (user == null)
            {
                return BadRequest(new { message = error });
            }

            var now = DateTime.UtcNow;
            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.PasswordChangedAt = now;
            user.MustChangePassword = false;
            user.ResetPasswordOtp = null;
            user.ResetPasswordOtpExpiresAt = null;
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;

            // Thu hồi mọi phiên đăng nhập cũ
            var activeTokens = await _context.UserTokenEntities
                .Where(t => t.UserId == user.Id && t.RevokedAt == null)
                .ToListAsync(HttpContext.RequestAborted);
            foreach (var t in activeTokens)
            {
                t.RevokedAt = now;
                t.RevokedReason = "Password reset";
            }

            await _context.SaveChangesAsync(HttpContext.RequestAborted);
            return Ok(new { success = true, message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại." });
        }

        // ===================== XÁC THỰC 2 BƯỚC (TOTP) =====================

        /// <summary>
        /// Sinh secret TOTP mới (chưa bật). Người dùng quét QR rồi gọi /2fa/enable với mã 6 số để kích hoạt.
        /// </summary>
        [Authorize]
        [HttpPost("2fa/setup")]
        public async Task<IActionResult> SetupTwoFactor()
        {
            var user = await GetCurrentUserEntityAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.TwoFactorEnabled)
            {
                return BadRequest(new { message = "Xác thực 2 bước đã được bật. Hãy tắt trước khi thiết lập lại." });
            }

            var secret = TotpHelper.GenerateSecret();
            user.TwoFactorSecret = secret;
            await _context.SaveChangesAsync(HttpContext.RequestAborted);

            var issuer = _configuration["AuthSettings:TotpIssuer"] ?? "HRM";
            return Ok(new
            {
                secret,
                otpAuthUri = TotpHelper.BuildOtpAuthUri(secret, user.Email ?? user.Username, issuer)
            });
        }

        [Authorize]
        [HttpPost("2fa/enable")]
        public async Task<IActionResult> EnableTwoFactor([FromBody] TwoFactorCodeRequest request)
        {
            var user = await GetCurrentUserEntityAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.TwoFactorEnabled)
            {
                return BadRequest(new { message = "Xác thực 2 bước đã được bật." });
            }

            if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
            {
                return BadRequest(new { message = "Chưa thiết lập xác thực 2 bước. Vui lòng tạo mã QR trước." });
            }

            if (!TotpHelper.VerifyCode(user.TwoFactorSecret, request.Code))
            {
                return BadRequest(new { message = "Mã xác thực không chính xác. Kiểm tra lại đồng hồ trên điện thoại và thử lại." });
            }

            user.TwoFactorEnabled = true;
            user.TwoFactorEnabledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(HttpContext.RequestAborted);

            return Ok(new { success = true, message = "Đã bật xác thực 2 bước." });
        }

        [Authorize]
        [HttpPost("2fa/disable")]
        public async Task<IActionResult> DisableTwoFactor([FromBody] TwoFactorDisableRequest request)
        {
            var user = await GetCurrentUserEntityAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            if (!user.TwoFactorEnabled)
            {
                return BadRequest(new { message = "Xác thực 2 bước chưa được bật." });
            }

            if (string.IsNullOrEmpty(request.Password)
                || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return BadRequest(new { message = "Mật khẩu không chính xác." });
            }

            if (!TotpHelper.VerifyCode(user.TwoFactorSecret ?? string.Empty, request.Code))
            {
                return BadRequest(new { message = "Mã xác thực không chính xác." });
            }

            user.TwoFactorEnabled = false;
            user.TwoFactorEnabledAt = null;
            user.TwoFactorSecret = null;
            await _context.SaveChangesAsync(HttpContext.RequestAborted);

            return Ok(new { success = true, message = "Đã tắt xác thực 2 bước." });
        }

        [Authorize]
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions([FromQuery] bool includeRevoked = false)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var query = _context.UserTokenEntities.Where(t => t.UserId == userId);
            if (!includeRevoked)
            {
                query = query.Where(t => t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
            }

            var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var sessions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.DeviceName,
                    t.Platform,
                    t.IpAddress,
                    t.CreatedAt,
                    t.ExpiresAt,
                    t.RevokedAt,
                    isCurrent = t.IpAddress == currentIp
                })
                .ToListAsync(HttpContext.RequestAborted);

            return Ok(sessions);
        }

        [Authorize]
        [HttpDelete("sessions/{id}")]
        public async Task<IActionResult> RevokeSession(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var session = await _context.UserTokenEntities.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, HttpContext.RequestAborted);
            if (session != null && session.RevokedAt == null)
            {
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = "Revoked by user";
                await _context.SaveChangesAsync(HttpContext.RequestAborted);
            }

            return Ok(new { success = true, message = "Đã thu hồi phiên đăng nhập." });
        }

        private (string token, int expiresInMinutes) GenerateJwtToken(UserEntity user, List<string> roles, List<string> permissions)
        {
            var secret = _configuration["JwtSettings:Secret"] ?? HrmApi.Infrastructure.JwtDefaults.Secret;
            var issuer = _configuration["JwtSettings:Issuer"] ?? HrmApi.Infrastructure.JwtDefaults.Issuer;
            var audience = _configuration["JwtSettings:Audience"] ?? HrmApi.Infrastructure.JwtDefaults.Audience;
            var expiryMinutes = int.TryParse(_configuration["JwtSettings:ExpiryInMinutes"], out var exp) ? exp : 720;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("username", user.Username),
                new Claim("type", user.Type ?? "EMPLOYEE")
            };

            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
                claims.Add(new Claim("email", user.Email));
            }

            if (user.CompanyId.HasValue)
            {
                claims.Add(new Claim("companyId", user.CompanyId.Value.ToString()));
            }

            if (user.BranchId.HasValue)
            {
                claims.Add(new Claim("branchId", user.BranchId.Value.ToString()));
            }

            if (user.EmployeeId.HasValue)
            {
                claims.Add(new Claim("employeeId", user.EmployeeId.Value.ToString()));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("roles", role));
            }

            foreach (var perm in permissions)
            {
                claims.Add(new Claim("permission", perm));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiryMinutes);
        }

        private async Task RegisterFailedLoginAsync(UserEntity user)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
            {
                user.IsLocked = true;
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
            }
            await _context.SaveChangesAsync(HttpContext.RequestAborted);
        }

        private async Task<UserEntity?> GetCurrentUserEntityAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return await _context.UserEntities
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, HttpContext.RequestAborted);
        }

        /// <summary>
        /// Tìm tài khoản theo thông tin đăng nhập: tên đăng nhập, email tài khoản, hoặc email công ty của nhân viên.
        /// Email cá nhân của nhân viên KHÔNG được dùng để đăng nhập.
        /// </summary>
        private async Task<UserEntity?> FindUserByLoginAsync(string login)
        {
            login = login.Trim().ToLowerInvariant();
            return await _context.UserEntities
                .Include(u => u.Employee)
                .Where(u => !u.IsDeleted)
                .FirstOrDefaultAsync(u =>
                        u.Username.ToLower() == login
                        || (u.Email != null && u.Email.ToLower() == login)
                        || (u.Employee != null && u.Employee.CompanyEmail != null && u.Employee.CompanyEmail.ToLower() == login),
                    HttpContext.RequestAborted);
        }

        /// <summary>Quên mật khẩu: nhập email công ty (hoặc email tài khoản quản trị).</summary>
        private Task<UserEntity?> FindUserByEmailAsync(string email) => FindUserByLoginAsync(email);

        /// <summary>Nơi nhận mã OTP: email công ty / email tài khoản.</summary>
        private static string? ResolveEmail(UserEntity user)
        {
            if (!string.IsNullOrWhiteSpace(user.Employee?.CompanyEmail)) return user.Employee.CompanyEmail;
            if (!string.IsNullOrWhiteSpace(user.Email)) return user.Email;
            return user.Username.Contains('@') ? user.Username : null;
        }

        /// <summary>
        /// Kiểm tra OTP đặt lại mật khẩu. Sai quá MaxOtpAttempts lần thì OTP bị hủy.
        /// ResetPasswordOtp lưu dạng "{hash}|{số lần nhập sai}".
        /// </summary>
        private async Task<(UserEntity? user, string error)> ValidateResetOtpAsync(string? email, string? otp)
        {
            const string invalid = "Mã xác thực không hợp lệ hoặc đã hết hạn.";

            var normalizedEmail = email?.Trim().ToLowerInvariant();
            var normalizedOtp = otp?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(normalizedOtp))
            {
                return (null, "Vui lòng nhập email và mã xác thực.");
            }

            var user = await FindUserByEmailAsync(normalizedEmail);
            if (user == null || !user.IsActive
                || string.IsNullOrEmpty(user.ResetPasswordOtp)
                || user.ResetPasswordOtpExpiresAt == null
                || user.ResetPasswordOtpExpiresAt < DateTime.UtcNow)
            {
                return (null, invalid);
            }

            var parts = user.ResetPasswordOtp.Split('|');
            var storedHash = parts[0];
            var attempts = parts.Length > 1 && int.TryParse(parts[1], out var a) ? a : 0;

            var match = CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(storedHash),
                Encoding.ASCII.GetBytes(HashOtp(user.Id, normalizedOtp)));

            if (!match)
            {
                attempts++;
                if (attempts >= MaxOtpAttempts)
                {
                    user.ResetPasswordOtp = null;
                    user.ResetPasswordOtpExpiresAt = null;
                    await _context.SaveChangesAsync(HttpContext.RequestAborted);
                    return (null, "Nhập sai mã quá nhiều lần. Vui lòng yêu cầu mã mới.");
                }

                user.ResetPasswordOtp = $"{storedHash}|{attempts}";
                await _context.SaveChangesAsync(HttpContext.RequestAborted);
                return (null, invalid);
            }

            return (user, string.Empty);
        }

        private static string HashOtp(Guid userId, string otp) => HashToken($"{userId:N}:{otp}");

        private static string? ValidateNewPassword(string? newPassword, string? confirmNewPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return "Mật khẩu mới phải có ít nhất 6 ký tự.";
            }

            if (confirmNewPassword != null && confirmNewPassword != newPassword)
            {
                return "Mật khẩu xác nhận không khớp.";
            }

            return null;
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }

    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; } = string.Empty;
        public string? Code2fa { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string? RefreshToken { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string? CurrentPassword { get; set; }
        /// <summary>Alias của CurrentPassword (một số màn hình gửi oldPassword).</summary>
        public string? OldPassword { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string? ConfirmNewPassword { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string? Email { get; set; }
    }

    public class VerifyResetOtpRequest
    {
        public string? Email { get; set; }
        public string? Otp { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string? Email { get; set; }
        public string? Otp { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string? ConfirmNewPassword { get; set; }
    }

    public class TwoFactorCodeRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    public class TwoFactorDisableRequest
    {
        public string Code { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}

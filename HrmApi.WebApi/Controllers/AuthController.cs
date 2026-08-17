using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HrmApi.Application.Common.Constants;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Auth;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Permission;
using HrmApi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    public abstract class AuthController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher<UserEntity> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IAuthContextService _authContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public AuthController(
            IApplicationDbContext context,
            IPasswordHasher<UserEntity> passwordHasher,
            IConfiguration configuration,
            IEmailService emailService,
            IAuthContextService authContext,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _emailService = emailService;
            _authContext = authContext;
            _httpClientFactory = httpClientFactory;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>Shell auth nhẹ — navbar / cold start (không Include org graph / attendance).</summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized();
            }

            UserEntity? user = await _context.UserEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return Unauthorized("Phiên đăng nhập không còn hợp lệ. Vui lòng đăng nhập lại.");
            }

            string? email = user.Email;
            string? avatarUrl = null;
            string? fullName = null;
            Guid? companyId = user.CompanyId;
            Guid? branchId = user.BranchId;
            Guid? employeeId = user.EmployeeId;

            if (user.EmployeeId.HasValue && user.EmployeeId != Guid.Empty)
            {
                var empLight = await _context.EmployeeEntities.AsNoTracking()
                    .Where(e => e.Id == user.EmployeeId.Value && !e.IsDeleted)
                    .Select(e => new
                    {
                        e.Email,
                        e.CompanyEmail,
                        e.AvatarUrl,
                        e.FullName,
                        e.FirstName,
                        e.LastName,
                        e.CompanyId,
                        e.BranchId,
                    })
                    .FirstOrDefaultAsync();

                if (empLight != null)
                {
                    employeeId = user.EmployeeId;
                    companyId = empLight.CompanyId ?? companyId;
                    branchId = empLight.BranchId ?? branchId;
                    avatarUrl = empLight.AvatarUrl;
                    email = empLight.Email ?? empLight.CompanyEmail ?? email;
                    fullName = string.IsNullOrWhiteSpace(empLight.FullName)
                        ? $"{empLight.LastName} {empLight.FirstName}".Trim()
                        : empLight.FullName;
                    if (string.IsNullOrWhiteSpace(fullName)) fullName = null;
                }
            }

            if (string.IsNullOrWhiteSpace(email))
                email = $"{user.Username}@hrm.com";

            AuthContextDto authContext = await _authContext.LoadAuthContextAsync(user.Id);

            ActionResult? mobileDenied = ValidateMobileAccess(user, authContext);
            if (mobileDenied != null)
                return mobileDenied;

            return Ok(new AuthMeDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = email,
                AvatarUrl = avatarUrl,
                FullName = fullName,
                Type = user.Type ?? string.Empty,
                EmployeeId = employeeId,
                CompanyId = companyId,
                BranchId = branchId,
                Roles = authContext.Roles,
                Permissions = authContext.Permissions,
                TwoFactorEnabled = user.TwoFactorEnabled,
            });
        }

        /// <summary>Hồ sơ đầy đủ + stats tháng — màn Cá nhân / drawer (không dùng cho shell sau login).</summary>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized();
            }

            UserEntity? user = await _context.UserEntities
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return Unauthorized("Phiên đăng nhập không còn hợp lệ. Vui lòng đăng nhập lại.");
            }

            EmployeeEntity? employee = null;
            if (user.EmployeeId.HasValue && user.EmployeeId != Guid.Empty)
            {
                employee = await _context.EmployeeEntities
                    .AsNoTracking()
                    .Include(e => e.Company)
                    .Include(e => e.Branch)
                    .Include(e => e.Department)
                    .Include(e => e.Part)
                    .Include(e => e.Position)
                        .ThenInclude(p => p!.PositionMaster)
                    .FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value && !e.IsDeleted);
            }

            Guid? companyId = employee?.CompanyId ?? user.CompanyId;
            Guid? branchId = employee?.BranchId ?? user.BranchId;

            string? companyName = employee?.Company?.Name;
            if (string.IsNullOrWhiteSpace(companyName) && companyId.HasValue)
            {
                companyName = await _context.CompanyEntities.AsNoTracking()
                    .Where(x => x.Id == companyId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync();
            }

            string? branchName = employee?.Branch?.Name;
            if (string.IsNullOrWhiteSpace(branchName) && branchId.HasValue)
            {
                branchName = await _context.BranchEntities.AsNoTracking()
                    .Where(x => x.Id == branchId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync();
            }

            string? departmentName = employee?.Department?.Name;
            string? partName = employee?.Part?.Name;
            string? positionName = employee?.Position?.PositionMaster?.Name;

            string? fullName = employee?.FullName;
            if (string.IsNullOrWhiteSpace(fullName) && employee != null)
            {
                fullName = $"{employee.LastName} {employee.FirstName}".Trim();
            }

            string? address = employee?.NowAddress;
            if (string.IsNullOrWhiteSpace(address))
            {
                address = employee?.PermanentAddress;
            }

            var businessToday = BusinessDateHelper.Today();
            int year = businessToday.Year;
            int month = businessToday.Month;

            int? workDaysThisMonth = null;
            int? onTimeDays = null;
            int? lateDays = null;
            int? absentDays = null;
            int? leaveDaysThisMonth = null;
            if (employee != null)
            {
                var summary = await _context.TimekeepingSummaryEntities.AsNoTracking()
                    .Where(x => x.EmployeeId == employee.Id && !x.IsDeleted && x.Year == year && x.Month == month)
                    .Select(x => new
                    {
                        x.OnTimeDays,
                        x.LateDays,
                        x.EarlyDays,
                        x.AbsentDays,
                        x.LeaveDays
                    })
                    .FirstOrDefaultAsync();

                if (summary != null)
                {
                    workDaysThisMonth = summary.OnTimeDays + summary.LateDays + summary.EarlyDays;
                    onTimeDays = summary.OnTimeDays;
                    lateDays = summary.LateDays;
                    absentDays = summary.AbsentDays;
                    leaveDaysThisMonth = summary.LeaveDays;
                }
                else
                {
                    DateOnly from = new(year, month, 1);
                    DateOnly to = from.AddMonths(1).AddDays(-1);
                    List<AttendanceStatus> statuses = await _context.TimekeepingEntities.AsNoTracking()
                        .Where(x =>
                            x.EmployeeId == employee.Id
                            && !x.IsDeleted
                            && x.WorkDate >= from
                            && x.WorkDate <= to)
                        .Select(x => x.Status)
                        .ToListAsync();

                    onTimeDays = statuses.Count(s => s == AttendanceStatus.ON_TIME);
                    lateDays = statuses.Count(s => s == AttendanceStatus.LATE);
                    absentDays = statuses.Count(s => s == AttendanceStatus.ABSENT);
                    leaveDaysThisMonth = statuses.Count(s => s == AttendanceStatus.LEAVE);
                    workDaysThisMonth = statuses.Count(s =>
                        s == AttendanceStatus.ON_TIME
                        || s == AttendanceStatus.LATE
                        || s == AttendanceStatus.EARLY);
                }
            }

            AuthContextDto authContext = await _authContext.LoadAuthContextAsync(user.Id);

            var profile = new MobileProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = employee?.Email ?? user.Email ?? $"{user.Username}@hrm.com",
                Phone = employee?.Phone ?? user.PhoneNumber,
                PhoneNumber = user.PhoneNumber ?? employee?.Phone,
                Type = user.Type ?? string.Empty,
                EmployeeId = employee?.Id ?? user.EmployeeId,
                EmployeeCode = employee?.Code,
                FullName = fullName,
                FirstName = employee?.FirstName,
                LastName = employee?.LastName,
                Gender = employee?.Gender,
                AvatarUrl = employee?.AvatarUrl,
                DateOfBirth = employee?.DayOfBirth == default ? null : employee!.DayOfBirth.ToString("dd/MM/yyyy"),
                Address = address,
                PermanentAddress = employee?.PermanentAddress,
                CompanyEmail = employee?.CompanyEmail,
                CompanyId = companyId,
                CompanyName = companyName,
                Company = companyName,
                BranchId = branchId,
                BranchName = branchName,
                Branch = branchName,
                DepartmentId = employee?.DepartmentId,
                DepartmentName = departmentName,
                Department = departmentName,
                PartId = employee?.PartId,
                PartName = partName,
                Part = partName,
                PositionId = employee?.PositionId,
                PositionName = positionName,
                Position = positionName,
                JoinDate = employee == null || employee.JoinDate == default
                    ? null
                    : employee.JoinDate.ToString("dd/MM/yyyy"),
                Status = employee?.Status,
                Level = employee?.Level,
                WorkingMode = employee?.WorkingMode,
                ContractType = employee?.ContractType,
                BankAccountNumber = employee?.BankAccountNumber,
                BankName = employee?.BankName,
                Roles = authContext.Roles,
                Permissions = authContext.Permissions,
                TwoFactorEnabled = user.TwoFactorEnabled,
                Stats = new MobileProfileStatsDto
                {
                    WorkDaysThisMonth = workDaysThisMonth,
                    LeaveDaysRemaining = null,
                    LeaveDaysThisMonth = leaveDaysThisMonth,
                    OnTimeDays = onTimeDays,
                    LateDays = lateDays,
                    AbsentDays = absentDays,
                }
            };

            return Ok(profile);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for username: {Username}", request?.Username);
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogDebug("Login rejected: Username or password null/empty.");
                return BadRequest("Tài khoản và mật khẩu không được để trống.");
            }

            UserEntity? user = await _context.UserEntities
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower() && !u.IsDeleted);

            if (user == null)
            {
                _logger.LogDebug("Login rejected: User {Username} not found.", request.Username);
                return BadRequest("Tài khoản hoặc mật khẩu không chính xác.");
            }

            if (!user.IsActive)
            {
                _logger.LogDebug("Login rejected: User {Username} is inactive.", user.Username);
                return BadRequest("Tài khoản đang bị khóa hoặc ngưng hoạt động.");
            }

            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            {
                double lockTimeRemaining = Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
                _logger.LogDebug("Login rejected: User {Username} locked for {Minutes} minutes.", user.Username, lockTimeRemaining);
                return BadRequest($"Tài khoản đang bị khóa tạm thời. Vui lòng thử lại sau {lockTimeRemaining} phút.");
            }

            string passwordHash = user.PasswordHash ?? string.Empty;
            PasswordVerificationResult passwordResult = _passwordHasher.VerifyHashedPassword(user, passwordHash, request.Password ?? string.Empty);
            _logger.LogDebug("Password verify for {Username}: {Result}", user.Username, passwordResult);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                user.FailedLoginAttempts++;
                _logger.LogDebug("Login rejected: Incorrect password for {Username}. Failed attempts: {Attempts}", user.Username, user.FailedLoginAttempts);
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLoginAttempts = 0;
                    _ = await _context.SaveChangesAsync(default);
                    _logger.LogInformation("User {Username} locked due to too many failed attempts.", user.Username);
                    return BadRequest("Tài khoản của bạn đã bị khóa tạm thời 15 phút do nhập sai mật khẩu quá 5 lần.");
                }

                _ = await _context.SaveChangesAsync(default);
                int attemptsLeft = 5 - user.FailedLoginAttempts;
                return BadRequest($"Tài khoản hoặc mật khẩu không chính xác. Bạn còn {attemptsLeft} lần thử.");
            }

            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;

            if (user.TwoFactorEnabled)
            {
                _ = await _context.SaveChangesAsync(default);
                string tempToken = GenerateTwoFactorTempToken(user);
                return Ok(new LoginResponse
                {
                    RequiresTwoFactor = true,
                    TempToken = tempToken,
                    Username = user.Username,
                    Type = user.Type,
                    TwoFactorEnabled = true,
                });
            }

            return Ok(await CompleteLoginAsync(user));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("RefreshToken không được để trống.");
            }

            string refreshTokenHash = HashToken(request.RefreshToken);
            UserTokenEntity? tokenEntity = await _context.UserTokenEntities
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.RefreshTokenHash == refreshTokenHash && !t.IsDeleted);

            if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow || tokenEntity.RevokedAt.HasValue)
            {
                return Unauthorized("Phiên đăng nhập đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.");
            }

            UserEntity user = tokenEntity.User;
            if (user == null || !user.IsActive || (user.IsLocked && user.LockedUntil > DateTime.UtcNow))
            {
                return Unauthorized("Tài khoản đang bị khóa hoặc ngưng hoạt động.");
            }

            AuthContextDto authContext = await _authContext.LoadAuthContextAsync(user.Id);

            bool isMobileSession = string.Equals(tokenEntity.Platform, "MOBILE", StringComparison.OrdinalIgnoreCase)
                || IsMobileAuthRequest;
            if (isMobileSession)
            {
                ActionResult? mobileDenied = ValidateMobileAccess(user, authContext);
                if (mobileDenied != null)
                    return mobileDenied;
            }

            string newAccessToken = GenerateJwtToken(user, authContext);
            string newRefreshToken = GenerateRefreshToken();

            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.RevokedReason = "Rotated";

            string newRefreshTokenHash = HashToken(newRefreshToken);
            UserTokenEntity newUserToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenHash = newRefreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Platform = tokenEntity.Platform,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            tokenEntity.ReplacedByTokenId = newUserToken.Id;

            _ = _context.UserTokenEntities.Add(newUserToken);
            _ = await _context.SaveChangesAsync(default);

            return Ok(new LoginResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Username = user.Username,
                Type = user.Type,
                EmployeeId = user.EmployeeId,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId,
                MustChangePassword = user.MustChangePassword,
                Roles = authContext.Roles,
                Permissions = authContext.Permissions,
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Email không được để trống.");
            }

            UserEntity? user = await _context.UserEntities
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted);

            if (user == null)
            {
                return BadRequest("Email không tồn tại trong hệ thống.");
            }

            string otp = new Random().Next(100000, 999999).ToString();
            user.ResetPasswordOtp = otp;
            user.ResetPasswordOtpExpiresAt = DateTime.UtcNow.AddMinutes(10);

            _ = await _context.SaveChangesAsync(default);

            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                    <h2 style='color: #3b82f6;'>Đặt lại mật khẩu HRM System</h2>
                    <p>Xin chào <strong>{user.Username}</strong>,</p>
                    <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã OTP dưới đây để hoàn tất:</p>
                    <div style='background-color: #f1f5f9; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #1e293b; border-radius: 6px; margin: 20px 0;'>
                        {otp}
                    </div>
                    <p style='color: #64748b; font-size: 13px;'>Mã OTP này có hiệu lực trong vòng <strong>10 phút</strong>. Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email!, "Mã OTP Đặt lại mật khẩu - HRM System", emailBody);

            return Ok(new { message = "Mã OTP đã được gửi về email đăng ký của bạn." });
        }

        [HttpPost("reset-password-with-otp")]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("Các trường thông tin không được để trống.");
            }

            UserEntity? user = await _context.UserEntities
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted);

            if (user == null)
            {
                return BadRequest("Email không tồn tại trong hệ thống.");
            }

            if (user.ResetPasswordOtp != request.Otp || !user.ResetPasswordOtpExpiresAt.HasValue || user.ResetPasswordOtpExpiresAt.Value < DateTime.UtcNow)
            {
                return BadRequest("Mã OTP không chính xác hoặc đã hết hạn.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.ResetPasswordOtp = null;
            user.ResetPasswordOtpExpiresAt = null;
            user.MustChangePassword = true;
            user.PasswordChangedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(default);

            return Ok(new { message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới." });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("Thông tin mật khẩu không được để trống.");
            }

            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized();
            }

            UserEntity? user = await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return BadRequest("Tài khoản không tồn tại.");
            }

            PasswordVerificationResult passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return BadRequest("Mật khẩu cũ không chính xác.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.MustChangePassword = false;
            user.PasswordChangedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(default);

            return Ok(new { message = "Thay đổi mật khẩu thành công." });
        }

        [Authorize]
        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (request == null)
            {
                return BadRequest("Thông tin cập nhật không hợp lệ.");
            }

            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized();
            }

            UserEntity? user = await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return BadRequest("Tài khoản không tồn tại.");
            }

            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;

            if (user.EmployeeId.HasValue && user.EmployeeId != Guid.Empty)
            {
                EmployeeEntity? employee = await _context.EmployeeEntities
                    .FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value && !e.IsDeleted);
                if (employee != null)
                {
                    if (!string.IsNullOrWhiteSpace(request.Email))
                    {
                        employee.Email = request.Email.Trim();
                    }
                    if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    {
                        employee.Phone = request.PhoneNumber.Trim();
                    }
                    if (request.AvatarUrl != null)
                    {
                        employee.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
                            ? null
                            : request.AvatarUrl.Trim();
                    }
                    employee.UpdatedAt = DateTime.UtcNow;
                }
            }

            _ = await _context.SaveChangesAsync(default);

            return Ok(new { message = "Cập nhật thông tin thành công." });
        }

        [Authorize]
        [HttpPost("2fa/setup")]
        public async Task<ActionResult<TwoFactorSetupResponse>> SetupTwoFactor()
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized();

            UserEntity? user = await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return NotFound("Người dùng không tồn tại.");

            string secret = TotpHelper.GenerateSecret();
            user.TwoFactorSecret = secret;
            user.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(default);

            return Ok(new TwoFactorSetupResponse
            {
                Secret = secret,
                OtpAuthUri = TotpHelper.BuildOtpAuthUri(secret, user.Username, "HRM"),
            });
        }

        [Authorize]
        [HttpPost("2fa/enable")]
        public async Task<IActionResult> EnableTwoFactor([FromBody] TwoFactorCodeRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized();
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Mã xác thực không được để trống.");

            UserEntity? user = await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return NotFound("Người dùng không tồn tại.");
            if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
                return BadRequest("Chưa thiết lập secret 2FA. Vui lòng gọi setup trước.");
            if (!TotpHelper.VerifyCode(user.TwoFactorSecret, request.Code))
                return BadRequest("Mã xác thực không đúng.");

            user.TwoFactorEnabled = true;
            user.TwoFactorEnabledAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(default);
            return Ok(new { message = "Đã bật xác thực hai bước.", twoFactorEnabled = true });
        }

        [Authorize]
        [HttpPost("2fa/disable")]
        public async Task<IActionResult> DisableTwoFactor([FromBody] TwoFactorCodeRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized();
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Mã xác thực không được để trống.");

            UserEntity? user = await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return NotFound("Người dùng không tồn tại.");

            bool isAdminForce = User.IsInRole("ADMIN")
                || User.Claims.Any(c => c.Type == ClaimTypesEx.Permission
                    && string.Equals(c.Value, PermissionCodes.SystemSettingsManage, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                PasswordVerificationResult pwd = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (pwd == PasswordVerificationResult.Failed)
                    return BadRequest("Mật khẩu không chính xác.");
            }

            if (!isAdminForce)
            {
                if (string.IsNullOrWhiteSpace(user.TwoFactorSecret)
                    || !TotpHelper.VerifyCode(user.TwoFactorSecret, request.Code))
                {
                    return BadRequest("Mã xác thực không đúng.");
                }
            }

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.TwoFactorEnabledAt = null;
            user.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(default);
            return Ok(new { message = "Đã tắt xác thực hai bước.", twoFactorEnabled = false });
        }

        [HttpPost("2fa/verify")]
        public async Task<ActionResult<LoginResponse>> VerifyTwoFactor([FromBody] TwoFactorVerifyRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TempToken) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("TempToken và mã xác thực là bắt buộc.");

            Guid? userId = ValidateTwoFactorTempToken(request.TempToken);
            if (!userId.HasValue)
                return Unauthorized("Temp token không hợp lệ hoặc đã hết hạn.");

            UserEntity? user = await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted);
            if (user == null || !user.IsActive)
                return Unauthorized("Tài khoản không hợp lệ.");
            if (!user.TwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecret))
                return BadRequest("Tài khoản chưa bật 2FA.");
            if (!TotpHelper.VerifyCode(user.TwoFactorSecret, request.Code))
                return BadRequest("Mã xác thực không đúng.");

            return Ok(await CompleteLoginAsync(user));
        }

        [HttpGet("sso/status")]
        [Authorize]
        public ActionResult<SsoStatusResponse> GetSsoStatus()
        {
            return Ok(BuildSsoStatus());
        }

        [HttpPost("sso/{provider}/start")]
        public ActionResult<SsoStartResponse> StartSso(string provider)
        {
            string? key = NormalizeProvider(provider);
            if (key == null) return BadRequest("Provider không hỗ trợ. Dùng google hoặc microsoft.");

            string section = $"Authentication:{Capitalize(key)}";
            bool enabled = bool.TryParse(_configuration[$"{section}:Enabled"], out bool en) && en;
            string? clientId = _configuration[$"{section}:ClientId"];
            if (!enabled || string.IsNullOrWhiteSpace(clientId))
            {
                return StatusCode(StatusCodes.Status501NotImplemented,
                    $"SSO provider '{key}' chưa được cấu hình (ClientId / Enabled).");
            }

            string redirectUri = _configuration[$"{section}:RedirectUri"]
                ?? $"http://localhost:4200/auth/sso/{key}/callback";
            string authorizeUrl = key switch
            {
                "google" =>
                    "https://accounts.google.com/o/oauth2/v2/auth"
                    + $"?client_id={Uri.EscapeDataString(clientId)}"
                    + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
                    + "&response_type=code&scope=openid%20email%20profile&access_type=offline",
                "microsoft" =>
                    $"https://login.microsoftonline.com/{_configuration[$"{section}:TenantId"] ?? "common"}/oauth2/v2.0/authorize"
                    + $"?client_id={Uri.EscapeDataString(clientId)}"
                    + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
                    + "&response_type=code&scope=openid%20email%20profile%20User.Read",
                _ => string.Empty,
            };

            return Ok(new SsoStartResponse { Provider = key, AuthorizeUrl = authorizeUrl });
        }

        [HttpPost("sso/{provider}/callback")]
        public async Task<ActionResult<LoginResponse>> SsoCallback(string provider, [FromBody] SsoCallbackRequest request)
        {
            string? key = NormalizeProvider(provider);
            if (key == null) return BadRequest("Provider không hỗ trợ.");
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Authorization code là bắt buộc.");

            string section = $"Authentication:{Capitalize(key)}";
            bool enabled = bool.TryParse(_configuration[$"{section}:Enabled"], out bool en) && en;
            string? clientId = _configuration[$"{section}:ClientId"];
            string? clientSecret = _configuration[$"{section}:ClientSecret"];
            if (!enabled || string.IsNullOrWhiteSpace(clientId))
            {
                return StatusCode(StatusCodes.Status501NotImplemented,
                    $"SSO provider '{key}' chưa được cấu hình (ClientId / Enabled).");
            }

            string redirectUri = request.RedirectUri
                ?? _configuration[$"{section}:RedirectUri"]
                ?? $"http://localhost:4200/auth/sso/{key}/callback";

            string subject;
            string? email = null;
            if (!string.IsNullOrWhiteSpace(clientSecret))
            {
                try
                {
                    (subject, email) = await ExchangeOidcCodeAsync(key, clientId!, clientSecret!, redirectUri, request.Code);
                }
                catch (Exception ex)
                {
                    return BadRequest($"SSO token exchange failed: {ex.Message}");
                }
            }
            else
            {
                subject = $"stub-{key}-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Code)))[..16].ToLowerInvariant();
            }

            UserEntity? user = key == "google"
                ? await _context.UserEntities.FirstOrDefaultAsync(u => u.GoogleSubject == subject && !u.IsDeleted)
                : await _context.UserEntities.FirstOrDefaultAsync(u => u.MicrosoftSubject == subject && !u.IsDeleted);

            if (user == null && !string.IsNullOrWhiteSpace(email))
            {
                user = await _context.UserEntities.FirstOrDefaultAsync(
                    u => u.Email != null && u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
                if (user != null)
                {
                    if (key == "google") user.GoogleSubject = subject;
                    else user.MicrosoftSubject = subject;
                }
            }

            if (user == null)
            {
                string username = !string.IsNullOrWhiteSpace(email)
                    ? email.Split('@')[0]
                    : $"{key}_{subject[..Math.Min(12, subject.Length)]}";
                username = await EnsureUniqueUsernameAsync(username);
                user = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    Email = email,
                    PasswordHash = _passwordHasher.HashPassword(new UserEntity(), Guid.NewGuid().ToString("N")),
                    Type = "EMPLOYEE",
                    IsActive = true,
                    MustChangePassword = false,
                    CreatedAt = DateTime.UtcNow,
                    GoogleSubject = key == "google" ? subject : null,
                    MicrosoftSubject = key == "microsoft" ? subject : null,
                };
                _ = _context.UserEntities.Add(user);
            }

            if (!user.IsActive)
                return BadRequest("Tài khoản đang bị khóa hoặc ngưng hoạt động.");

            _ = await _context.SaveChangesAsync(default);

            if (user.TwoFactorEnabled)
            {
                return Ok(new LoginResponse
                {
                    RequiresTwoFactor = true,
                    TempToken = GenerateTwoFactorTempToken(user),
                    Username = user.Username,
                    Type = user.Type,
                    TwoFactorEnabled = true,
                });
            }

            return Ok(await CompleteLoginAsync(user));
        }

        [Authorize]
        [HttpGet("sessions/list")]
        [HttpPost("sessions/list")]
        public async Task<ActionResult<List<SessionDto>>> ListSessions([FromBody] SessionListRequest? request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized();

            bool canManageAll = User.IsInRole("ADMIN")
                || User.Claims.Any(c => c.Type == ClaimTypesEx.Permission
                    && string.Equals(c.Value, PermissionCodes.SystemSettingsManage, StringComparison.OrdinalIgnoreCase));

            bool allUsers = canManageAll && request?.AllUsers == true;
            bool includeRevoked = request?.IncludeRevoked == true;

            IQueryable<UserTokenEntity> query = _context.UserTokenEntities.AsNoTracking()
                .Include(t => t.User)
                .Where(t => !t.IsDeleted);

            if (!allUsers)
                query = query.Where(t => t.UserId == userId);
            if (!includeRevoked)
                query = query.Where(t => t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);

            List<UserTokenEntity> rows = await query.OrderByDescending(t => t.CreatedAt).Take(200).ToListAsync();
            List<SessionDto> items = rows.Select(t => new SessionDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Username = t.User?.Username,
                Platform = t.Platform,
                DeviceName = t.DeviceName,
                IpAddress = t.IpAddress,
                UserAgent = t.UserAgent,
                CreatedAt = t.CreatedAt,
                ExpiresAt = t.ExpiresAt,
                RevokedAt = t.RevokedAt,
                IsCurrent = false,
            }).ToList();

            return Ok(items);
        }

        [Authorize]
        [HttpPost("sessions/revoke")]
        public async Task<IActionResult> RevokeSession([FromBody] SessionRevokeRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized();
            if (request == null || request.Id == Guid.Empty)
                return BadRequest("Id phiên không hợp lệ.");

            bool canManageAll = User.IsInRole("ADMIN")
                || User.Claims.Any(c => c.Type == ClaimTypesEx.Permission
                    && string.Equals(c.Value, PermissionCodes.SystemSettingsManage, StringComparison.OrdinalIgnoreCase));

            UserTokenEntity? token = await _context.UserTokenEntities
                .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted);
            if (token == null) return NotFound("Không tìm thấy phiên.");
            if (token.UserId != userId && !canManageAll)
                return Forbid();

            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "RevokedByUser";
            token.UpdatedAt = DateTime.UtcNow;
            token.UpdatedBy = userId;
            _ = await _context.SaveChangesAsync(default);
            return Ok(new { message = "Đã thu hồi phiên.", id = token.Id });
        }

        private const string MobileAccessDeniedMessage =
            "Bạn không có quyền truy cập ứng dụng di động. Vui lòng liên hệ quản trị viên.";

        private bool IsMobileAuthRequest =>
            Request.Path.StartsWithSegments("/api/v1/mobile", StringComparison.OrdinalIgnoreCase);

        private static bool HasMobileAccess(UserEntity user, AuthContextDto authContext)
        {
            if (string.Equals(user.Type, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase)
                || authContext.Roles.Any(r => string.Equals(r, RoleCodes.Admin, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return authContext.Permissions.Any(p =>
                string.Equals(p, PermissionCodes.MobileAccess, StringComparison.OrdinalIgnoreCase));
        }

        private ActionResult? ValidateMobileAccess(UserEntity user, AuthContextDto authContext)
        {
            if (!IsMobileAuthRequest)
                return null;

            if (HasMobileAccess(user, authContext))
                return null;

            _logger.LogWarning(
                "Mobile access denied for user {Username}: missing {Permission}.",
                user.Username,
                PermissionCodes.MobileAccess);
            return StatusCode(StatusCodes.Status403Forbidden, MobileAccessDeniedMessage);
        }

        private async Task<ActionResult<LoginResponse>> CompleteLoginAsync(UserEntity user)
        {
            AuthContextDto authContext = await _authContext.LoadAuthContextAsync(user.Id);
            ActionResult? mobileDenied = ValidateMobileAccess(user, authContext);
            if (mobileDenied != null)
                return mobileDenied;

            return Ok(await IssueFullLoginAsync(user, authContext));
        }

        private async Task<LoginResponse> IssueFullLoginAsync(UserEntity user, AuthContextDto authContext)
        {
            string tokenString = GenerateJwtToken(user, authContext);
            string refreshToken = GenerateRefreshToken();

            string refreshTokenHash = HashToken(refreshToken);
            string platform = Request.Path.StartsWithSegments("/api/v1/mobile") ? "MOBILE" : "WEB";
            UserTokenEntity userToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Platform = platform,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            _ = _context.UserTokenEntities.Add(userToken);

            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            _ = await _context.SaveChangesAsync(default);

            string? email = user.Email;
            string? avatarUrl = null;
            string? fullName = null;
            Guid? companyId = user.CompanyId;
            Guid? branchId = user.BranchId;

            if (user.EmployeeId.HasValue && user.EmployeeId != Guid.Empty)
            {
                var empLight = await _context.EmployeeEntities.AsNoTracking()
                    .Where(e => e.Id == user.EmployeeId.Value && !e.IsDeleted)
                    .Select(e => new
                    {
                        e.Email,
                        e.CompanyEmail,
                        e.AvatarUrl,
                        e.FullName,
                        e.FirstName,
                        e.LastName,
                        e.CompanyId,
                        e.BranchId,
                    })
                    .FirstOrDefaultAsync();

                if (empLight != null)
                {
                    companyId = empLight.CompanyId ?? companyId;
                    branchId = empLight.BranchId ?? branchId;
                    avatarUrl = empLight.AvatarUrl;
                    email = empLight.Email ?? empLight.CompanyEmail ?? email;
                    fullName = string.IsNullOrWhiteSpace(empLight.FullName)
                        ? $"{empLight.LastName} {empLight.FirstName}".Trim()
                        : empLight.FullName;
                    if (string.IsNullOrWhiteSpace(fullName)) fullName = null;
                }
            }

            if (string.IsNullOrWhiteSpace(email))
                email = $"{user.Username}@hrm.com";

            _logger.LogInformation("Login successful for user {Username}", user.Username);

            return new LoginResponse
            {
                Token = tokenString,
                RefreshToken = refreshToken,
                Username = user.Username,
                Type = user.Type,
                EmployeeId = user.EmployeeId,
                CompanyId = companyId,
                BranchId = branchId,
                Email = email,
                AvatarUrl = avatarUrl,
                FullName = fullName,
                MustChangePassword = user.MustChangePassword,
                TwoFactorEnabled = user.TwoFactorEnabled,
                Roles = authContext.Roles,
                Permissions = authContext.Permissions,
            };
        }

        private string GenerateTwoFactorTempToken(UserEntity user)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            string jwtSecret = _configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForHrmSystem2026!AwesomeDesignPleaseChangeMeInProduction";
            byte[] key = Encoding.ASCII.GetBytes(jwtSecret);
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("purpose", "2fa"),
                ]),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = _configuration["JwtSettings:Issuer"] ?? "HrmApi",
                Audience = _configuration["JwtSettings:Audience"] ?? "HrmAdmin",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private Guid? ValidateTwoFactorTempToken(string tempToken)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new();
                string jwtSecret = _configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForHrmSystem2026!AwesomeDesignPleaseChangeMeInProduction";
                TokenValidationParameters parameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"] ?? "HrmApi",
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"] ?? "HrmAdmin",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
                ClaimsPrincipal principal = tokenHandler.ValidateToken(tempToken, parameters, out _);
                string? purpose = principal.FindFirst("purpose")?.Value;
                if (!string.Equals(purpose, "2fa", StringComparison.Ordinal))
                    return null;
                string? id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(id, out Guid userId) ? userId : null;
            }
            catch
            {
                return null;
            }
        }

        private SsoStatusResponse BuildSsoStatus()
        {
            return new SsoStatusResponse
            {
                Google = ReadProviderStatus("Google"),
                Microsoft = ReadProviderStatus("Microsoft"),
            };
        }

        private SsoProviderStatusDto ReadProviderStatus(string name)
        {
            string section = $"Authentication:{name}";
            bool enabled = bool.TryParse(_configuration[$"{section}:Enabled"], out bool en) && en;
            string? clientId = _configuration[$"{section}:ClientId"];
            bool configured = !string.IsNullOrWhiteSpace(clientId);
            string? masked = null;
            if (configured && clientId!.Length > 8)
                masked = clientId[..4] + "…" + clientId[^4..];
            else if (configured)
                masked = "****";
            return new SsoProviderStatusDto
            {
                Enabled = enabled,
                Configured = configured,
                ClientIdMasked = masked,
            };
        }

        private async Task<(string Subject, string? Email)> ExchangeOidcCodeAsync(
            string provider, string clientId, string clientSecret, string redirectUri, string code)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string tokenUrl = provider == "google"
                ? "https://oauth2.googleapis.com/token"
                : $"https://login.microsoftonline.com/{_configuration["Authentication:Microsoft:TenantId"] ?? "common"}/oauth2/v2.0/token";

            using FormUrlEncodedContent form = new(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code",
            });
            HttpResponseMessage resp = await client.PostAsync(tokenUrl, form);
            string body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException(body);

            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(body);
            string? idToken = doc.RootElement.TryGetProperty("id_token", out var idEl) ? idEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(idToken))
                throw new InvalidOperationException("id_token missing from token response.");

            JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
            string? sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            string? email = jwt.Claims.FirstOrDefault(c => c.Type is "email" or "preferred_username")?.Value;
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidOperationException("sub claim missing.");
            return (sub, email);
        }

        private async Task<string> EnsureUniqueUsernameAsync(string baseUsername)
        {
            string candidate = baseUsername;
            int i = 1;
            while (await _context.UserEntities.AnyAsync(u => u.Username == candidate && !u.IsDeleted))
            {
                candidate = $"{baseUsername}{i++}";
            }
            return candidate;
        }

        private static string? NormalizeProvider(string? provider)
        {
            if (string.IsNullOrWhiteSpace(provider)) return null;
            string p = provider.Trim().ToLowerInvariant();
            return p is "google" or "microsoft" ? p : null;
        }

        private static string Capitalize(string s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

        private bool TryGetUserId(out Guid userId)
        {
            string? userIdStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
            return Guid.TryParse(userIdStr, out userId);
        }

        private string GenerateJwtToken(UserEntity user, AuthContextDto authContext)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            string jwtSecret = _configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForHrmSystem2026!AwesomeDesignPleaseChangeMeInProduction";
            byte[] key = Encoding.ASCII.GetBytes(jwtSecret);

            List<Claim> claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypesEx.UserType, user.Type ?? string.Empty),
                new Claim("UserCode", user.Username),
                new Claim("CompanyId", user.CompanyId?.ToString() ?? string.Empty),
                new Claim("BranchId", user.BranchId?.ToString() ?? string.Empty),
                new Claim("EmployeeId", user.EmployeeId?.ToString() ?? string.Empty)
            ];

            foreach (string role in authContext.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (!string.IsNullOrWhiteSpace(user.Type)
                && !authContext.Roles.Any(r => string.Equals(r, user.Type, StringComparison.OrdinalIgnoreCase)))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Type));
            }

            // Wave B5: slim JWT — không nhồi permission claims (AuthZ dùng IPermissionCache).
            // Giữ perm_ver để client/cache biết khi quyền đổi.
            string permVer = ComputePermissionVersion(authContext);
            claims.Add(new Claim("perm_ver", permVer));

            double expiryInMinutes = double.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "720");
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
                Issuer = _configuration["JwtSettings:Issuer"] ?? "HrmApi",
                Audience = _configuration["JwtSettings:Audience"] ?? "HrmAdmin",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string ComputePermissionVersion(AuthContextDto authContext)
        {
            string payload = string.Join('|',
                authContext.Roles.Concat(authContext.Permissions)
                    .Select(x => x.Trim().ToUpperInvariant())
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(x => x, StringComparer.Ordinal));
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash.AsSpan(0, 8));
        }

        private string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashToken(string token)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Auth;
using HrmApi.Domain.Entities.Employee;
using HrmApi.Domain.Entities.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    public abstract class AuthController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher<UserEntity> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(
            IApplicationDbContext context,
            IPasswordHasher<UserEntity> passwordHasher,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _emailService = emailService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            UserEntity? user = await _context.UserEntities
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            EmployeeEntity? employee = user.Employee;

            return Ok(new
            {
                user.Id,
                user.Username,
                Email = user.Email ?? $"{user.Username}@hrm.com",
                user.PhoneNumber,
                user.Type,
                user.CompanyId,
                user.BranchId,
                user.EmployeeId,
                Employee = employee != null && !employee.IsDeleted ? new
                {
                    employee.Id,
                    employee.Code,
                    employee.FirstName,
                    employee.LastName,
                    employee.FullName,
                    employee.Gender,
                    employee.Phone,
                    employee.Email,
                    employee.CompanyEmail,
                    employee.DayOfBirth,
                    employee.BankAccountNumber,
                    employee.BankName
                } : null
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            System.Console.WriteLine($"[API Auth] Login attempt received for username: '{request?.Username}'");
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                System.Console.WriteLine("[API Auth] Login rejected: Username or password null/empty.");
                return BadRequest("Tài khoản và mật khẩu không được để trống.");
            }

            UserEntity? user = await _context.UserEntities
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower() && !u.IsDeleted);

            if (user == null)
            {
                System.Console.WriteLine($"[API Auth] Login rejected: User '{request.Username}' not found.");
                return BadRequest("Tài khoản hoặc mật khẩu không chính xác.");
            }

            if (!user.IsActive)
            {
                System.Console.WriteLine($"[API Auth] Login rejected: User '{user.Username}' is inactive.");
                return BadRequest("Tài khoản đang bị khóa hoặc ngưng hoạt động.");
            }

            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            {
                double lockTimeRemaining = Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
                System.Console.WriteLine($"[API Auth] Login rejected: User '{user.Username}' is locked for another {lockTimeRemaining} minutes.");
                return BadRequest($"Tài khoản đang bị khóa tạm thời. Vui lòng thử lại sau {lockTimeRemaining} phút.");
            }

            string passwordHash = user.PasswordHash ?? string.Empty;
            PasswordVerificationResult passwordResult = _passwordHasher.VerifyHashedPassword(user, passwordHash, request.Password ?? string.Empty);
            System.Console.WriteLine($"[API Auth] Password verify for '{user.Username}': result={passwordResult}, hashLen={passwordHash.Length}, pwdLen={(request.Password ?? string.Empty).Length}");
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                user.FailedLoginAttempts++;
                System.Console.WriteLine($"[API Auth] Login rejected: Incorrect password for user '{user.Username}'. Failed attempts: {user.FailedLoginAttempts}");
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLoginAttempts = 0;
                    _ = await _context.SaveChangesAsync(default);
                    System.Console.WriteLine($"[API Auth] User '{user.Username}' has been locked due to too many failed attempts.");
                    return BadRequest("Tài khoản của bạn đã bị khóa tạm thời 15 phút do nhập sai mật khẩu quá 5 lần.");
                }

                _ = await _context.SaveChangesAsync(default);
                int attemptsLeft = 5 - user.FailedLoginAttempts;
                return BadRequest($"Tài khoản hoặc mật khẩu không chính xác. Bạn còn {attemptsLeft} lần thử.");
            }

            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;

            string tokenString = GenerateJwtToken(user);
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

            System.Console.WriteLine($"[API Auth] Login successful for user: '{user.Username}'. Token generated.");

            return Ok(new LoginResponse
            {
                Token = tokenString,
                RefreshToken = refreshToken,
                Username = user.Username,
                Type = user.Type,
                EmployeeId = user.EmployeeId,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId,
                MustChangePassword = user.MustChangePassword
            });
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

            string newAccessToken = GenerateJwtToken(user);
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
                MustChangePassword = user.MustChangePassword
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

            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
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

            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
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

            _ = await _context.SaveChangesAsync(default);

            return Ok(new { message = "Cập nhật thông tin thành công." });
        }

        private string GenerateJwtToken(UserEntity user)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            string jwtSecret = _configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForHrmSystem2026!AwesomeDesignPleaseChangeMeInProduction";
            byte[] key = Encoding.ASCII.GetBytes(jwtSecret);

            Claim[] claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Type),
                new Claim("UserCode", user.Username),
                new Claim("CompanyId", user.CompanyId?.ToString() ?? string.Empty),
                new Claim("BranchId", user.BranchId?.ToString() ?? string.Empty),
                new Claim("EmployeeId", user.EmployeeId?.ToString() ?? string.Empty)
            };

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

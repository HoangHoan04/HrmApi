using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs;
using HrmApi.Domain.Entities.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HrmApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            return Ok(new
            {
                user.Id,
                user.Username,
                Email = user.Email ?? $"{user.Username}@hrm.com",
                user.PhoneNumber,
                user.Type,
                user.CompanyId,
                user.BranchId
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Tài khoản và mật khẩu không được để trống.");
            }

            var user = await _context.UserEntities
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower() && !u.IsDeleted);

            if (user == null)
            {
                return BadRequest("Tài khoản hoặc mật khẩu không chính xác.");
            }

            if (!user.IsActive)
            {
                return BadRequest("Tài khoản đang bị khóa hoặc ngưng hoạt động.");
            }

            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            {
                var lockTimeRemaining = Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
                return BadRequest($"Tài khoản đang bị khóa tạm thời. Vui lòng thử lại sau {lockTimeRemaining} phút.");
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLoginAttempts = 0;
                    await _context.SaveChangesAsync(default);
                    return BadRequest("Tài khoản của bạn đã bị khóa tạm thời 15 phút do nhập sai mật khẩu quá 5 lần.");
                }

                await _context.SaveChangesAsync(default);
                var attemptsLeft = 5 - user.FailedLoginAttempts;
                return BadRequest($"Tài khoản hoặc mật khẩu không chính xác. Bạn còn {attemptsLeft} lần thử.");
            }

            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;

            var tokenString = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenHash = HashToken(refreshToken);
            var userToken = new UserTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Platform = "WEB",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            _context.UserTokenEntities.Add(userToken);

            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _context.SaveChangesAsync(default);

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

            var refreshTokenHash = HashToken(request.RefreshToken);
            var tokenEntity = await _context.UserTokenEntities
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.RefreshTokenHash == refreshTokenHash && !t.IsDeleted);

            if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow || tokenEntity.RevokedAt.HasValue)
            {
                return Unauthorized("Phiên đăng nhập đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.");
            }

            var user = tokenEntity.User;
            if (user == null || !user.IsActive || (user.IsLocked && user.LockedUntil > DateTime.UtcNow))
            {
                return Unauthorized("Tài khoản đang bị khóa hoặc ngưng hoạt động.");
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.RevokedReason = "Rotated";

            var newRefreshTokenHash = HashToken(newRefreshToken);
            var newUserToken = new UserTokenEntity
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

            _context.UserTokenEntities.Add(newUserToken);
            await _context.SaveChangesAsync(default);

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

            var user = await _context.UserEntities
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted);

            if (user == null)
            {
                return BadRequest("Email không tồn tại trong hệ thống.");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            user.ResetPasswordOtp = otp;
            user.ResetPasswordOtpExpiresAt = DateTime.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync(default);

            var emailBody = $@"
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

            var user = await _context.UserEntities
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

            await _context.SaveChangesAsync(default);

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

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return BadRequest("Tài khoản không tồn tại.");
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return BadRequest("Mật khẩu cũ không chính xác.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.MustChangePassword = false;
            user.PasswordChangedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(default);

            return Ok(new { message = "Thay đổi mật khẩu thành công." });
        }

        private string GenerateJwtToken(UserEntity user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForHrmSystem2026!AwesomeDesignPleaseChangeMeInProduction";
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Type),
                new Claim("UserCode", user.Username),
                new Claim("CompanyId", user.CompanyId?.ToString() ?? string.Empty),
                new Claim("BranchId", user.BranchId?.ToString() ?? string.Empty)
            };

            var expiryInMinutes = double.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "720");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
                Issuer = _configuration["JwtSettings:Issuer"] ?? "HrmApi",
                Audience = _configuration["JwtSettings:Audience"] ?? "HrmAdmin",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}

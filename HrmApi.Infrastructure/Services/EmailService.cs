using HrmApi.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HrmApi.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var host = _configuration["SmtpSettings:Host"] ?? Environment.GetEnvironmentVariable("SMTP_HOST");
            var portStr = _configuration["SmtpSettings:Port"] ?? Environment.GetEnvironmentVariable("SMTP_PORT");
            var username = _configuration["SmtpSettings:Username"] ?? Environment.GetEnvironmentVariable("SMTP_USERNAME");
            var password = _configuration["SmtpSettings:Password"] ?? Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var enableSslStr = _configuration["SmtpSettings:EnableSsl"] ?? Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL");
            var fromEmail = _configuration["SmtpSettings:FromEmail"] ?? Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "no-reply@hrm.com";

            _logger.LogInformation("==================================================");
            _logger.LogInformation($"GỬI EMAIL TỚI: {to}");
            _logger.LogInformation($"TIÊU ĐỀ: {subject}");
            _logger.LogInformation($"NỘI DUNG:\n{body}");
            _logger.LogInformation("==================================================");

            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogWarning("SMTP Host chưa được cấu hình. Chỉ ghi log email ra console.");
                return;
            }

            try
            {
                int port = int.TryParse(portStr, out var p) ? p : 587;
                bool enableSsl = !bool.TryParse(enableSslStr, out var ssl) || ssl;

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.UseDefaultCredentials = false;
                    if (!string.IsNullOrEmpty(username))
                    {
                        client.Credentials = new NetworkCredential(username, password);
                    }

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(to);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Gửi email thành công tới: {to}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi email tới {to}");
            }
        }
    }
}

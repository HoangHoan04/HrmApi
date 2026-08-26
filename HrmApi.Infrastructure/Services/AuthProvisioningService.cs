using HrmApi.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace HrmApi.Infrastructure.Services;

/// <summary>
/// Gọi AuthApi để tạo tài khoản khi HRM thêm nhân viên mới.
/// </summary>
public class AuthProvisioningService : IAuthProvisioningService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthProvisioningService> _logger;

    public AuthProvisioningService(
        HttpClient http,
        IConfiguration configuration,
        ILogger<AuthProvisioningService> logger)
    {
        _http = http;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Guid> ProvisionEmployeeAccountAsync(
        string email,
        string fullName,
        string? phone,
        Guid? companyId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("[AuthProvisioning] Email trống, bỏ qua provisioning.");
            return Guid.Empty;
        }

        var baseUrl = _configuration["AuthService:BaseUrl"]?.TrimEnd('/');
        var apiKey = _configuration["AuthService:InternalApiKey"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("[AuthProvisioning] AuthService:BaseUrl chưa được cấu hình.");
            return Guid.Empty;
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/internal/provision-employee")
            {
                Content = JsonContent.Create(new
                {
                    email,
                    fullName,
                    phone,
                    companyId
                })
            };

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Add("X-Internal-Api-Key", apiKey);
            }

            var response = await _http.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("[AuthProvisioning] AuthApi trả về lỗi {StatusCode}: {Body}",
                    response.StatusCode, body);
                return Guid.Empty;
            }

            var result = await response.Content.ReadFromJsonAsync<ProvisionResponse>(cancellationToken: cancellationToken);
            if (result?.UserId is { } userId && userId != Guid.Empty)
            {
                _logger.LogInformation("[AuthProvisioning] {Action} tài khoản Auth cho {Email}, UserId = {UserId}",
                    result.IsNew ? "Tạo mới" : "Đã tồn tại", email, userId);
                return userId;
            }

            return Guid.Empty;
        }
        catch (Exception ex)
        {
            // Không để lỗi Auth block việc tạo nhân viên
            _logger.LogError(ex, "[AuthProvisioning] Lỗi khi gọi AuthApi provision cho {Email}", email);
            return Guid.Empty;
        }
    }

    private class ProvisionResponse
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }
        [JsonPropertyName("isNew")]
        public bool IsNew { get; set; }
    }
}

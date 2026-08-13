namespace HrmApi.Application.DTOs.Settings
{
    public class SettingsIdRequest
    {
        public Guid Id { get; set; }
    }

    public class SettingsPagedQuery
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
    }

    // ── Report Schedule ──────────────────────────────────────
    public class ReportScheduleDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string CronHint { get; set; } = string.Empty;
        public string EmailTo { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastRunAt { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ReportScheduleCommandFields
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ReportType { get; set; }
        public string? CronHint { get; set; }
        public string? EmailTo { get; set; }
        public bool? IsActive { get; set; }
        public string? Note { get; set; }
    }

    // ── Compliance ───────────────────────────────────────────
    public class ComplianceSummaryRequest
    {
        public int? WithinDays { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class ComplianceSummaryDto
    {
        public int ExpiringContractCount { get; set; }
        public int ExpiringFileCount { get; set; }
        public int PendingTransferCount { get; set; }
        public int WithinDays { get; set; }
    }

    // ── Legal Rate Config ────────────────────────────────────
    public class LegalRateConfigDto
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public decimal SocialInsuranceEmployeeRate { get; set; }
        public decimal SocialInsuranceEmployerRate { get; set; }
        public decimal HealthInsuranceRate { get; set; }
        public decimal UnemploymentRate { get; set; }
        public decimal PersonalDeduction { get; set; }
        public decimal DependentDeduction { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class LegalRateConfigCommandFields
    {
        public int? Year { get; set; }
        public decimal? SocialInsuranceEmployeeRate { get; set; }
        public decimal? SocialInsuranceEmployerRate { get; set; }
        public decimal? HealthInsuranceRate { get; set; }
        public decimal? UnemploymentRate { get; set; }
        public decimal? PersonalDeduction { get; set; }
        public decimal? DependentDeduction { get; set; }
        public string? Note { get; set; }
    }

    // ── Notification Template ────────────────────────────────
    public class NotificationTemplateDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class NotificationTemplateCommandFields
    {
        public string? Code { get; set; }
        public string? Channel { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public bool? IsActive { get; set; }
        public string? Note { get; set; }
    }

    // ── API Client Key ───────────────────────────────────────
    public class ApiClientKeyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KeyPrefix { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ApiClientKeyCreateResultDto : ApiClientKeyDto
    {
        /// <summary>Plaintext key — only returned once on create.</summary>
        public string PlaintextKey { get; set; } = string.Empty;
    }

    public class ApiClientKeyCommandFields
    {
        public string? Name { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Note { get; set; }
    }

    // ── Webhook Subscription ─────────────────────────────────
    public class WebhookSubscriptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string EventTypes { get; set; } = string.Empty;
        public string? Secret { get; set; }
        public bool IsActive { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class WebhookSubscriptionCommandFields
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? EventTypes { get; set; }
        public string? Secret { get; set; }
        public bool? IsActive { get; set; }
        public string? Note { get; set; }
    }

    // ── System Retention ─────────────────────────────────────
    public class SystemRetentionConfigDto
    {
        public Guid Id { get; set; }
        public int SoftDeleteRetentionDays { get; set; }
        public bool IsPurgeEnabled { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SystemRetentionConfigCommandFields
    {
        public int? SoftDeleteRetentionDays { get; set; }
        public bool? IsPurgeEnabled { get; set; }
        public string? Note { get; set; }
    }

    // ── IP Allowlist ─────────────────────────────────────────
    public class IpAllowlistEntryDto
    {
        public Guid Id { get; set; }
        public string CidrOrIp { get; set; } = string.Empty;
        public string? Note { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class IpAllowlistCommandFields
    {
        public string? CidrOrIp { get; set; }
        public string? Note { get; set; }
        public bool? IsActive { get; set; }
    }

    // ── SMS Gateway ──────────────────────────────────────────
    public class SmsGatewayConfigDto
    {
        public Guid Id { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string? SenderId { get; set; }
        public bool IsActive { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SmsGatewayConfigCommandFields
    {
        public string? Provider { get; set; }
        public string? ApiUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? SenderId { get; set; }
        public bool? IsActive { get; set; }
        public string? Note { get; set; }
    }

    public class SmsSendTestRequest
    {
        public Guid? ConfigId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Message { get; set; }
    }

    // ── Zalo OA ──────────────────────────────────────────────
    public class ZaloOaConfigDto
    {
        public Guid Id { get; set; }
        public string OaId { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool IsActive { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ZaloOaConfigCommandFields
    {
        public string? OaId { get; set; }
        public string? AppId { get; set; }
        public string? SecretKey { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool? IsActive { get; set; }
        public string? Note { get; set; }
    }

    public class ZaloSendTestRequest
    {
        public Guid? ConfigId { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
    }

    // ── Integration status ───────────────────────────────────
    public class IntegrationAdapterStatusDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public bool Configured { get; set; }
        public string? Detail { get; set; }
    }

    public class IntegrationStatusResultDto
    {
        public List<IntegrationAdapterStatusDto> Adapters { get; set; } = [];
        public DateTime CheckedAt { get; set; }
    }
}

using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.DTOs.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.Integrations
{
    public class GetIntegrationStatusQuery : IRequest<IntegrationStatusResultDto> { }

    public class GetIntegrationStatusQueryHandler : IRequestHandler<GetIntegrationStatusQuery, IntegrationStatusResultDto>
    {
        private readonly IApplicationDbContext _context;
        public GetIntegrationStatusQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<IntegrationStatusResultDto> Handle(GetIntegrationStatusQuery request, CancellationToken cancellationToken)
        {
            bool smsConfigured = await _context.SmsGatewayConfigEntities.AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.IsActive, cancellationToken);
            bool zaloConfigured = await _context.ZaloOaConfigEntities.AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.IsActive, cancellationToken);
            int webhookCount = await _context.WebhookSubscriptionEntities.AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.IsActive, cancellationToken);
            int apiKeyCount = await _context.ApiClientKeyEntities.AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.IsActive, cancellationToken);

            return new IntegrationStatusResultDto
            {
                CheckedAt = DateTime.UtcNow,
                Adapters =
                [
                    new() { Code = "punch_csv", Name = "Import punch CSV", Enabled = true, Configured = true, Detail = "POST /api/v1/timekeeping-punch/import-csv" },
                    new() { Code = "bank_payroll", Name = "Bank payroll file", Enabled = true, Configured = true, Detail = "POST /api/v1/salary/export-bank-file" },
                    new() { Code = "bhxh_export", Name = "BHXH export", Enabled = true, Configured = true, Detail = "POST /api/v1/salary/export-bhxh" },
                    new() { Code = "accounting_export", Name = "Accounting journal export", Enabled = true, Configured = true, Detail = "POST /api/v1/salary/export-accounting" },
                    new() { Code = "sms_gateway", Name = "SMS gateway", Enabled = true, Configured = smsConfigured, Detail = smsConfigured ? "Active config found" : "Chưa cấu hình SMS active" },
                    new() { Code = "zalo_oa", Name = "Zalo OA", Enabled = true, Configured = zaloConfigured, Detail = zaloConfigured ? "Active config found" : "Chưa cấu hình Zalo OA active" },
                    new() { Code = "webhook_delivery", Name = "Webhook delivery", Enabled = true, Configured = webhookCount > 0, Detail = $"{webhookCount} subscription(s) active" },
                    new() { Code = "api_keys", Name = "API client keys", Enabled = true, Configured = apiKeyCount > 0, Detail = $"{apiKeyCount} key(s) active" },
                ],
            };
        }
    }

    public class RefreshIntegrationStatusCommand : IRequest<IntegrationStatusResultDto> { }

    public class RefreshIntegrationStatusCommandHandler : IRequestHandler<RefreshIntegrationStatusCommand, IntegrationStatusResultDto>
    {
        private readonly IMediator _mediator;
        public RefreshIntegrationStatusCommandHandler(IMediator mediator) => _mediator = mediator;

        public Task<IntegrationStatusResultDto> Handle(RefreshIntegrationStatusCommand request, CancellationToken cancellationToken)
            => _mediator.Send(new GetIntegrationStatusQuery(), cancellationToken);
    }
}

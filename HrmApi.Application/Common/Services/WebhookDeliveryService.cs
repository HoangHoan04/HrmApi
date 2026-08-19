using HrmApi.Application.Common.Interfaces;
using HrmApi.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HrmApi.Application.Common.Services
{
    public class WebhookDeliveryService : IWebhookDeliveryService
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WebhookDeliveryService> _logger;

        public WebhookDeliveryService(
            IApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<WebhookDeliveryService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task PublishAsync(string eventType, object payload, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventType))
            {
                return;
            }

            List<WebhookSubscriptionEntity> subs;
            try
            {
                subs = await _context.WebhookSubscriptionEntities.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.IsActive)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook delivery: failed to load subscriptions for {EventType}", eventType);
                return;
            }

            List<WebhookSubscriptionEntity> targets = subs
                .Where(x => MatchesEvent(x.EventTypes, eventType))
                .ToList();
            if (targets.Count == 0)
            {
                return;
            }

            var body = new
            {
                eventType,
                occurredAt = DateTime.UtcNow,
                data = payload,
            };

            HttpClient client = _httpClientFactory.CreateClient(nameof(WebhookDeliveryService));
            foreach (WebhookSubscriptionEntity sub in targets)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using HttpRequestMessage request = new(HttpMethod.Post, sub.Url)
                        {
                            Content = JsonContent.Create(body),
                        };
                        if (!string.IsNullOrWhiteSpace(sub.Secret))
                        {
                            _ = request.Headers.TryAddWithoutValidation("X-Webhook-Secret", sub.Secret);
                        }

                        using HttpResponseMessage response = await client.SendAsync(request);
                        if (!response.IsSuccessStatusCode)
                        {
                            string text = await response.Content.ReadAsStringAsync();
                            _logger.LogWarning(
                                "Webhook delivery failed: {Name} {Url} → {Status} {Body}",
                                sub.Name, sub.Url, (int)response.StatusCode, text);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Webhook delivery error: {Name} {Url}", sub.Name, sub.Url);
                    }
                }, CancellationToken.None);
            }

            await Task.CompletedTask;
        }

        private static bool MatchesEvent(string eventTypes, string eventType)
        {
            return !string.IsNullOrWhiteSpace(eventTypes) && eventTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(x => string.Equals(x, eventType, StringComparison.OrdinalIgnoreCase)
                    || x == "*"
                    || string.Equals(x, "ALL", StringComparison.OrdinalIgnoreCase));
        }
    }
}

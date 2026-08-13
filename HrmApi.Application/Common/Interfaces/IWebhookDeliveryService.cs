using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Application.Common.Interfaces
{
    public interface IWebhookDeliveryService
    {
        Task PublishAsync(string eventType, object payload, CancellationToken cancellationToken = default);
    }

    public static class WebhookEventTypes
    {
        public const string PayrollPeriodFinalized = "PAYROLL.PERIOD_FINALIZED";
    }
}

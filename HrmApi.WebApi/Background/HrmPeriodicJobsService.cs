using HrmApi.Application.Features.Contracts.Commands;
using HrmApi.Application.Features.Settings;
using HrmApi.Application.Features.TransferEmployees.Commands;
using MediatR;

namespace HrmApi.WebApi.Background
{

    public class HrmPeriodicJobsService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HrmPeriodicJobsService> _logger;

        public HrmPeriodicJobsService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<HrmPeriodicJobsService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TimeSpan interval = GetInterval();
            _logger.LogInformation("HrmPeriodicJobsService started. Interval={Interval}", interval);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunTickAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "HrmPeriodicJobsService tick failed.");
                }

                try
                {
                    await Task.Delay(GetInterval(), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task RunTickAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            int withinDays = _configuration.GetValue("PeriodicJobs:ContractExpiryWithinDays", 30);

            MarkExpiringContractsResult expiry = await mediator.Send(
                new MarkExpiringContractsCommand { WithinDays = withinDays }, cancellationToken);
            _logger.LogInformation(
                "ContractExpiryJob done: marked={Marked}, renewals={Renewals}",
                expiry.MarkedCount, expiry.ReviewRenewalsCreated);

            ApplyDueTransfersResult transfers = await mediator.Send(new ApplyDueTransfersCommand(), cancellationToken);
            _logger.LogInformation(
                "TransferApplyDueJob done: applied={Applied}, failed={Failed}",
                transfers.AppliedCount, transfers.FailedCount);

            SoftDeleteRetentionJobResult retention = await mediator.Send(new SoftDeleteRetentionJobCommand(), cancellationToken);
            _logger.LogInformation("SoftDeleteRetentionJob: {Message}", retention.Message);

            int reports = await mediator.Send(new RunDueReportSchedulesCommand(), cancellationToken);
            _logger.LogInformation("RunDueReportSchedules done: ran={Count}", reports);
        }

        private TimeSpan GetInterval()
        {
            int hours = _configuration.GetValue("PeriodicJobs:IntervalHours", 1);
            if (hours < 1) hours = 1;
            return TimeSpan.FromHours(hours);
        }
    }
}

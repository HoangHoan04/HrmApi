using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HrmApi.Infrastructure.Persistence
{
    public static class DatabaseMigrationExtensions
    {
        public static async Task MigrateWithBootstrapAsync(
            this ApplicationDbContext context,
            IConfiguration configuration,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            await DatabaseBootstrap.EnsureUtf8DatabaseAsync(configuration, logger, cancellationToken);
            await context.Database.MigrateAsync(cancellationToken);
        }
    }
}

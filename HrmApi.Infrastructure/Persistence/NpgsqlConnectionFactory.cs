using Npgsql;

namespace HrmApi.Infrastructure.Persistence
{
    internal static class NpgsqlConnectionFactory
    {
        public static NpgsqlConnectionStringBuilder Build(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Encoding = "UTF8",
                Pooling = true,
                MinPoolSize = 0,
                MaxPoolSize = 100,
                ConnectionIdleLifetime = 60,
                ConnectionPruningInterval = 30,
                KeepAlive = 30,
                Timeout = 30,
                CommandTimeout = 30,
            };

            if (builder.SslMode == SslMode.Require)
            {
                builder.SslMode = SslMode.Prefer;
            }

            return builder;
        }

        public static string ToAdminConnectionString(NpgsqlConnectionStringBuilder builder)
        {
            var adminBuilder = new NpgsqlConnectionStringBuilder(builder.ConnectionString)
            {
                Database = "postgres",
            };
            return adminBuilder.ConnectionString;
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using System.Net.Sockets;

namespace HrmApi.Infrastructure.Persistence
{
    public static class DatabaseBootstrap
    {
        public const int Utf8EncodingId = 6;
        private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(2);
        private const int DefaultMaxAttempts = 30;

        public static Task EnsureUtf8DatabaseAsync(
            IConfiguration configuration,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            return EnsureUtf8DatabaseAsync(
                configuration.GetConnectionString("DefaultConnection"),
                logger,
                cancellationToken);
        }

        public static async Task EnsureUtf8DatabaseAsync(
            string? connectionString,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            logger ??= NullLogger.Instance;
            var builder = NpgsqlConnectionFactory.Build(connectionString);
            var databaseName = builder.Database;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("Database name is missing in the connection string.");
            }

            await WaitForPostgreSqlReadyAsync(
                NpgsqlConnectionFactory.ToAdminConnectionString(builder),
                logger,
                cancellationToken);

            builder.Database = "postgres";

            await using var connection = new NpgsqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await EnsureClusterSupportsUtf8Async(connection, logger, cancellationToken);

            var encodingId = await GetDatabaseEncodingIdAsync(connection, databaseName, cancellationToken);
            if (encodingId is null)
            {
                logger.LogInformation(
                    "Database {DatabaseName} does not exist. Creating with UTF8 encoding...",
                    databaseName);

                await CreateUtf8DatabaseAsync(connection, databaseName, cancellationToken);
                encodingId = await GetDatabaseEncodingIdAsync(connection, databaseName, cancellationToken);
            }

            if (encodingId != Utf8EncodingId)
            {
                var encodingName = await GetDatabaseEncodingNameAsync(connection, databaseName, cancellationToken);
                throw new InvalidOperationException(
                    BuildWrongEncodingMessage(databaseName, encodingName, encodingId));
            }

            logger.LogInformation(
                "Database {DatabaseName} is ready with UTF8 encoding.",
                databaseName);
        }

        private static async Task EnsureClusterSupportsUtf8Async(
            NpgsqlConnection connection,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var serverEncoding = await ExecuteScalarStringAsync(
                connection,
                "SHOW server_encoding",
                cancellationToken);

            var utf8EncodingName = await ExecuteScalarStringAsync(
                connection,
                $"SELECT pg_encoding_to_char({Utf8EncodingId})",
                cancellationToken);

            if (!string.Equals(utf8EncodingName, "UTF8", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "PostgreSQL cluster does not support UTF8 encoding. " +
                    "Reinstall PostgreSQL with a UTF8 locale (for example English, World) " +
                    "or initialize the data directory with UTF8 before running migrations.");
            }

            if (!string.Equals(serverEncoding, "UTF8", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "PostgreSQL server_encoding is {ServerEncoding}. New databases will still be created as UTF8.",
                    serverEncoding);
            }
        }

        private static async Task CreateUtf8DatabaseAsync(
            NpgsqlConnection connection,
            string databaseName,
            CancellationToken cancellationToken)
        {
            var quotedDatabaseName = QuoteIdentifier(databaseName);

            await using var createCommand = new NpgsqlCommand(
                $"""
                 CREATE DATABASE {quotedDatabaseName}
                 WITH ENCODING 'UTF8'
                      LC_COLLATE='C'
                      LC_CTYPE='C'
                      TEMPLATE template0
                 """,
                connection);
            await createCommand.ExecuteNonQueryAsync(cancellationToken);

            var encodingId = await GetDatabaseEncodingIdAsync(connection, databaseName, cancellationToken);
            if (encodingId != Utf8EncodingId)
            {
                var encodingName = await GetDatabaseEncodingNameAsync(connection, databaseName, cancellationToken);
                throw new InvalidOperationException(
                    "PostgreSQL created the database but it is not UTF8. " +
                    $"Actual encoding: {encodingName} (id={encodingId}). " +
                    "The PostgreSQL cluster must be reinstalled or reinitialized with UTF8 support.");
            }
        }

        private static async Task<int?> GetDatabaseEncodingIdAsync(
            NpgsqlConnection connection,
            string databaseName,
            CancellationToken cancellationToken)
        {
            await using var command = new NpgsqlCommand(
                """
                SELECT encoding
                FROM pg_database
                WHERE datname = @databaseName
                """,
                connection);
            command.Parameters.AddWithValue("databaseName", databaseName);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is null or DBNull ? null : Convert.ToInt32(result);
        }

        private static async Task<string?> GetDatabaseEncodingNameAsync(
            NpgsqlConnection connection,
            string databaseName,
            CancellationToken cancellationToken)
        {
            await using var command = new NpgsqlCommand(
                """
                SELECT pg_encoding_to_char(encoding)
                FROM pg_database
                WHERE datname = @databaseName
                """,
                connection);
            command.Parameters.AddWithValue("databaseName", databaseName);

            return await command.ExecuteScalarAsync(cancellationToken) as string;
        }

        private static async Task<string?> ExecuteScalarStringAsync(
            NpgsqlConnection connection,
            string sql,
            CancellationToken cancellationToken)
        {
            await using var command = new NpgsqlCommand(sql, connection);
            return (await command.ExecuteScalarAsync(cancellationToken))?.ToString();
        }

        private static string QuoteIdentifier(string identifier) =>
            "\"" + identifier.Replace("\"", "\"\"") + "\"";

        private static string BuildWrongEncodingMessage(
            string databaseName,
            string? encodingName,
            int? encodingId)
        {
            var quotedDatabaseName = QuoteIdentifier(databaseName);

            return
                $"Database '{databaseName}' is using encoding '{encodingName}' (id={encodingId}), but UTF8 (id={Utf8EncodingId}) is required. " +
                "Drop the database and run migrations again so it can be recreated with UTF8:\n" +
                $"DROP DATABASE IF EXISTS {quotedDatabaseName} WITH (FORCE);";
        }

        public static async Task WaitForPostgreSqlReadyAsync(
            string connectionString,
            ILogger logger,
            CancellationToken cancellationToken = default,
            int maxAttempts = DefaultMaxAttempts,
            TimeSpan? retryDelay = null)
        {
            var delay = retryDelay ?? DefaultRetryDelay;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await using var connection = new NpgsqlConnection(connectionString);
                    await connection.OpenAsync(cancellationToken);

                    await using var command = new NpgsqlCommand("SELECT 1", connection);
                    await command.ExecuteScalarAsync(cancellationToken);

                    if (attempt > 1)
                    {
                        logger.LogInformation("PostgreSQL is ready after {Attempt} attempts.", attempt);
                    }

                    return;
                }
                catch (Exception ex) when (IsTransientStartupError(ex) && attempt < maxAttempts)
                {
                    logger.LogWarning(
                        ex,
                        "PostgreSQL is not ready yet (attempt {Attempt}/{MaxAttempts}). Retrying in {DelaySeconds}s...",
                        attempt,
                        maxAttempts,
                        delay.TotalSeconds);

                    await Task.Delay(delay, cancellationToken);
                }
            }

            throw new InvalidOperationException(
                "PostgreSQL is not ready to accept connections. " +
                "Please ensure the PostgreSQL service is running and not stuck in recovery mode, then restart the API.");
        }

        private static bool IsTransientStartupError(Exception exception)
        {
            for (var current = exception; current is not null; current = current.InnerException)
            {
                if (current is NpgsqlException or IOException or SocketException)
                {
                    return true;
                }

                if (current.Message.Contains("recovery mode", StringComparison.OrdinalIgnoreCase)
                    || current.Message.Contains("starting up", StringComparison.OrdinalIgnoreCase)
                    || current.Message.Contains("forcibly closed", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

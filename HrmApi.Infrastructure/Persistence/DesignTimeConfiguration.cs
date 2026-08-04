using Microsoft.Extensions.Configuration;

namespace HrmApi.Infrastructure.Persistence
{
    internal static class DesignTimeConfiguration
    {
        public static IConfiguration Build()
        {
            var webApiPath = ResolveWebApiPath();
            var envPath = Path.Combine(webApiPath, ".env");

            if (File.Exists(envPath))
            {
                foreach (var line in File.ReadAllLines(envPath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                    }
                }
            }

            return new ConfigurationBuilder()
                .SetBasePath(webApiPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();
        }

        public static string ResolveWebApiPath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var candidates = new[]
            {
                currentDirectory,
                Path.Combine(currentDirectory, "HrmApi.WebApi"),
                Path.GetFullPath(Path.Combine(currentDirectory, "..", "HrmApi.WebApi")),
                Path.GetFullPath(Path.Combine(currentDirectory, "..", "..", "HrmApi.WebApi")),
            };

            foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (File.Exists(Path.Combine(candidate, "appsettings.json")))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException(
                "Could not find HrmApi.WebApi/appsettings.json. " +
                "Run dotnet ef from the HrmApi folder or specify --startup-project HrmApi.WebApi.");
        }
    }
}

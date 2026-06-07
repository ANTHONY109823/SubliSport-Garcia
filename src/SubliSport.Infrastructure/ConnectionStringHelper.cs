using Microsoft.Extensions.Configuration;

namespace SubliSport.Infrastructure;

public static class ConnectionStringHelper
{
    public static string Resolve(IConfiguration configuration)
    {
        // Railway inyecta DATABASE_URL — debe tener prioridad sobre appsettings.json
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return ParseDatabaseUrl(databaseUrl);
        }

        var fromPgVars = BuildFromRailwayPgVars();
        if (fromPgVars is not null)
        {
            return fromPgVars;
        }

        var fromConfig = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromConfig))
        {
            return fromConfig;
        }

        throw new InvalidOperationException(
            "No se encontró conexión a PostgreSQL. En Railway: referencia DATABASE_URL desde PostgreSQL. " +
            "Local: configure ConnectionStrings__DefaultConnection o use docker compose.");
    }

    public static string ParseDatabaseUrl(string databaseUrl)
    {
        var normalized = databaseUrl
            .Replace("postgres://", "postgresql://", StringComparison.OrdinalIgnoreCase);

        var uri = new Uri(normalized);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }

    private static string? BuildFromRailwayPgVars()
    {
        var host = Environment.GetEnvironmentVariable("PGHOST");
        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }

        var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var user = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? string.Empty;
        var database = Environment.GetEnvironmentVariable("PGDATABASE") ?? "railway";

        return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}

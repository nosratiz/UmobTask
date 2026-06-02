namespace GbfsQuiz.Infrastructure.Persistence;

/// <summary>
/// Normalises a connection string to Npgsql's key/value form. Managed hosts
/// (Render, Railway, Heroku, Fly) expose Postgres as a <c>postgres://</c> URI, which
/// Npgsql does not parse natively — this converts it so the same config works locally
/// and in the cloud.
/// </summary>
public static class PostgresConnectionString
{
    public static string Normalize(string connectionString)
    {
        if (!IsUri(connectionString))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var parts = uri.UserInfo.Split(':', 2);
        var database = uri.AbsolutePath.Trim('/');
        var sslMode = uri.Query.Contains("sslmode=require", StringComparison.OrdinalIgnoreCase)
            ? "SSL Mode=Require;Trust Server Certificate=true;"
            : string.Empty;

        return $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};" +
               $"Username={Uri.UnescapeDataString(parts[0])};" +
               $"Password={Uri.UnescapeDataString(parts.ElementAtOrDefault(1) ?? string.Empty)};" +
               $"Database={database};{sslMode}";
    }

    private static bool IsUri(string value) =>
        value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);
}

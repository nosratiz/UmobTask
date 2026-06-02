using GbfsQuiz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GbfsQuiz.Web.Common.Startup;

/// <summary>Applies pending EF Core migrations at startup for a friction-free demo run.</summary>
public static class DatabaseStartupExtensions
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = app.Logger;

        try
        {
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not apply database migrations. Is PostgreSQL running?");
        }
    }
}

using EmotionService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmotionService.Infrastructure.Persistence.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var logger    = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            var pending = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

            if (pending.Count == 0)
            {
                logger.LogInformation("✅ Database is up to date. No pending migrations.");
                return;
            }

            logger.LogInformation("⏳ Applying {Count} pending migration(s): {Names}",
                pending.Count, string.Join(", ", pending));

            await dbContext.Database.MigrateAsync();

            logger.LogInformation("✅ Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to apply migrations.");
            throw;
        }
    }
}

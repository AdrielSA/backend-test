using BackendTest.Infrastructure.Data;
using BackendTest.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;

namespace BackendTest.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> InitializeDatabaseAsync(
        this WebApplication app,
        bool runSeeder = false)
    {
        await app.MigrateDatabaseAsync();

        if (runSeeder)
        {
            await app.SeedDatabaseAsync();
        }

        return app;
    }

    private static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogInformation("Aplicando migraciones pendientes...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al aplicar migraciones");
            throw;
        }

        return app;
    }

    private static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app, bool force = false)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (!environment.IsDevelopment() && !force)
        {
            logger.LogWarning("El seeder solo puede ejecutarse en desarrollo. Use 'force: true' para override.");
            return app;
        }

        try
        {
            var seeder = new DatabaseSeeder(context, logger);
            await seeder.SeedAsync();
            logger.LogInformation("Seed completado exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al ejecutar el seeder");
            throw;
        }

        return app;
    }
}

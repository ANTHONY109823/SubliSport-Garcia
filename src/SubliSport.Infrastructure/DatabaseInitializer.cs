using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Infrastructure.Data;

namespace SubliSport.Infrastructure;

public static class DatabaseInitializer
{
    private const int MaxRetries = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await MigrateWithRetryAsync(context, logger);

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await EnsureSuperAdminAsync(userManager, configuration, logger);
    }

    private static async Task EnsureSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var superAdminEmail = configuration["Seed:SuperAdminEmail"]?.Trim();
        var superAdminPassword = configuration["Seed:SuperAdminPassword"];

        if (string.IsNullOrWhiteSpace(superAdminEmail) || string.IsNullOrWhiteSpace(superAdminPassword))
        {
            logger.LogWarning("SuperAdmin seed skipped: configure Seed:SuperAdminEmail y Seed:SuperAdminPassword.");
            return;
        }

        var existing = await userManager.FindByEmailAsync(superAdminEmail);
        if (existing is null)
        {
            var user = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                EmailConfirmed = true,
                FullName = "Super Administrador",
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, superAdminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo crear SuperAdmin: {errors}");
            }

            await userManager.AddToRoleAsync(user, AppRoles.SuperAdmin);
            logger.LogInformation("SuperAdmin creado: {Email}", superAdminEmail);
            return;
        }

        if (!await userManager.IsInRoleAsync(existing, AppRoles.SuperAdmin))
        {
            await userManager.AddToRoleAsync(existing, AppRoles.SuperAdmin);
        }

        existing.IsActive = true;
        await userManager.UpdateAsync(existing);

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(existing);
        var resetResult = await userManager.ResetPasswordAsync(existing, resetToken, superAdminPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            logger.LogWarning("No se pudo sincronizar clave SuperAdmin: {Errors}", errors);
            return;
        }

        logger.LogInformation("SuperAdmin sincronizado: {Email}", superAdminEmail);
    }

    private static async Task MigrateWithRetryAsync(AppDbContext context, ILogger logger)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Migraciones aplicadas correctamente.");
                return;
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                logger.LogWarning(ex,
                    "Intento {Attempt}/{Max} fallido al conectar con PostgreSQL. Reintentando en {Delay}s...",
                    attempt, MaxRetries, RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay);
            }
        }

        await context.Database.MigrateAsync();
    }
}

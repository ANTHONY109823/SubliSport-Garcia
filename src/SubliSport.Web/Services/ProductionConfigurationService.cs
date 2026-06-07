using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Production;
using SubliSport.Infrastructure.Data;

namespace SubliSport.Web.Services;

public class ProductionConfigurationService(AppDbContext db, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<ProductionSettingsData> GetSettingsAsync()
    {
        var row = await db.ProductionConfigurations.AsNoTracking().FirstOrDefaultAsync(p => p.Id == 1);
        if (row is null || string.IsNullOrWhiteSpace(row.JsonData))
        {
            return CreateDefaultWithFallback();
        }

        try
        {
            return JsonSerializer.Deserialize<ProductionSettingsData>(row.JsonData, JsonOptions)
                   ?? CreateDefaultWithFallback();
        }
        catch
        {
            return CreateDefaultWithFallback();
        }
    }

    public async Task SaveSettingsAsync(ProductionSettingsData settings, string userId)
    {
        var normalized = Normalize(settings);
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        var row = await db.ProductionConfigurations.FirstOrDefaultAsync(p => p.Id == 1);
        if (row is null)
        {
            db.ProductionConfigurations.Add(new ProductionConfiguration
            {
                Id = 1,
                JsonData = json,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = userId
            });
        }
        else
        {
            row.JsonData = json;
            row.UpdatedAt = DateTime.UtcNow;
            row.UpdatedByUserId = userId;
        }

        await db.SaveChangesAsync();
    }

    public async Task EnsureSeedAsync()
    {
        if (await db.ProductionConfigurations.AnyAsync(p => p.Id == 1))
        {
            return;
        }

        var defaults = CreateDefaultWithFallback();
        db.ProductionConfigurations.Add(new ProductionConfiguration
        {
            Id = 1,
            JsonData = JsonSerializer.Serialize(defaults, JsonOptions),
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private ProductionSettingsData CreateDefaultWithFallback()
    {
        var defaults = ProductionSettingsData.CreateDefault();
        var fromConfig = configuration["Production:WhatsAppNumber"];
        if (!string.IsNullOrWhiteSpace(fromConfig))
        {
            defaults.FallbackPhone = fromConfig.Trim();
        }

        return defaults;
    }

    private static ProductionSettingsData Normalize(ProductionSettingsData settings)
    {
        settings.GroupName = string.IsNullOrWhiteSpace(settings.GroupName)
            ? ProductionSettingsData.CreateDefault().GroupName
            : settings.GroupName.Trim();

        settings.GroupInviteUrl = settings.GroupInviteUrl?.Trim() ?? string.Empty;
        settings.FallbackPhone = string.IsNullOrWhiteSpace(settings.FallbackPhone)
            ? null
            : settings.FallbackPhone.Trim();
        settings.Notes = string.IsNullOrWhiteSpace(settings.Notes) ? null : settings.Notes.Trim();
        return settings;
    }
}

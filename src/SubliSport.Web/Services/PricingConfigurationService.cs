using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Pricing;
using SubliSport.Infrastructure.Data;

namespace SubliSport.Web.Services;

public class PricingConfigurationService(AppDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<PricingSettingsData> GetSettingsAsync()
    {
        var row = await db.PricingConfigurations.AsNoTracking().FirstOrDefaultAsync(p => p.Id == 1);
        if (row is null || string.IsNullOrWhiteSpace(row.JsonData))
        {
            return PricingSettingsData.CreateDefault();
        }

        try
        {
            return JsonSerializer.Deserialize<PricingSettingsData>(row.JsonData, JsonOptions)
                   ?? PricingSettingsData.CreateDefault();
        }
        catch
        {
            return PricingSettingsData.CreateDefault();
        }
    }

    public async Task SaveSettingsAsync(PricingSettingsData settings, string userId)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        var row = await db.PricingConfigurations.FirstOrDefaultAsync(p => p.Id == 1);
        if (row is null)
        {
            db.PricingConfigurations.Add(new PricingConfiguration
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
        if (await db.PricingConfigurations.AnyAsync(p => p.Id == 1))
        {
            return;
        }

        var defaults = PricingSettingsData.CreateDefault();
        db.PricingConfigurations.Add(new PricingConfiguration
        {
            Id = 1,
            JsonData = JsonSerializer.Serialize(defaults, JsonOptions),
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

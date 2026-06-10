using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Landing;
using SubliSport.Infrastructure.Data;

namespace SubliSport.Web.Services;

public class LandingConfigurationService(AppDbContext db, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<LandingSettingsData> GetSettingsAsync()
    {
        try
        {
            var row = await db.LandingConfigurations.AsNoTracking().FirstOrDefaultAsync(p => p.Id == 1);
            if (row is null || string.IsNullOrWhiteSpace(row.JsonData))
            {
                return CreateDefaultWithFallback();
            }

            return JsonSerializer.Deserialize<LandingSettingsData>(row.JsonData, JsonOptions)
                   ?? CreateDefaultWithFallback();
        }
        catch
        {
            return CreateDefaultWithFallback();
        }
    }

    public async Task SaveSettingsAsync(LandingSettingsData settings, string userId)
    {
        var normalized = Normalize(settings);
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        var row = await db.LandingConfigurations.FirstOrDefaultAsync(p => p.Id == 1);
        if (row is null)
        {
            db.LandingConfigurations.Add(new LandingConfiguration
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
        if (await db.LandingConfigurations.AnyAsync(p => p.Id == 1))
        {
            return;
        }

        var defaults = CreateDefaultWithFallback();
        db.LandingConfigurations.Add(new LandingConfiguration
        {
            Id = 1,
            JsonData = JsonSerializer.Serialize(defaults, JsonOptions),
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private LandingSettingsData CreateDefaultWithFallback()
    {
        var defaults = LandingSettingsData.CreateDefault();
        var fromConfig = configuration["Production:WhatsAppNumber"];
        if (!string.IsNullOrWhiteSpace(fromConfig))
        {
            defaults.Quote.WhatsAppPhone = fromConfig.Trim();
        }

        return defaults;
    }

    private static LandingSettingsData Normalize(LandingSettingsData settings)
    {
        settings.Catalog = settings.Catalog
            .Where(c => !string.IsNullOrWhiteSpace(c.ImageUrl) && !string.IsNullOrWhiteSpace(c.Title))
            .Select((c, i) =>
            {
                c.ImageUrl = c.ImageUrl.Trim();
                c.Title = c.Title.Trim();
                c.Subtitle = (c.Subtitle ?? string.Empty).Trim();
                c.SortOrder = i + 1;
                return c;
            })
            .ToList();

        settings.Quote.WhatsAppPhone = string.IsNullOrWhiteSpace(settings.Quote.WhatsAppPhone)
            ? LandingSettingsData.CreateDefault().Quote.WhatsAppPhone
            : settings.Quote.WhatsAppPhone.Trim();

        settings.Quote.ResponseNote = string.IsNullOrWhiteSpace(settings.Quote.ResponseNote)
            ? LandingSettingsData.CreateDefault().Quote.ResponseNote
            : settings.Quote.ResponseNote.Trim();

        settings.Quote.QuantityPlaceholder = (settings.Quote.QuantityPlaceholder ?? string.Empty).Trim();
        settings.Quote.NamePlaceholder = (settings.Quote.NamePlaceholder ?? string.Empty).Trim();
        settings.Quote.ExtraPlaceholder = (settings.Quote.ExtraPlaceholder ?? string.Empty).Trim();

        settings.Quote.Garments = settings.Quote.Garments
            .Where(g => !string.IsNullOrWhiteSpace(g.Label) && !string.IsNullOrWhiteSpace(g.Value))
            .Select(g =>
            {
                g.Label = g.Label.Trim();
                g.Value = g.Value.Trim();
                g.IconClass = string.IsNullOrWhiteSpace(g.IconClass) ? "fas fa-tshirt" : g.IconClass.Trim();
                g.IsMixed = g.IsMixed || g.Value.Equals("Mixta", StringComparison.OrdinalIgnoreCase);
                return g;
            })
            .ToList();

        if (settings.Quote.Garments.Count == 0)
        {
            settings.Quote.Garments = LandingSettingsData.CreateDefault().Quote.Garments;
        }

        if (!settings.Quote.Garments.Any(g => g.IsMixed))
        {
            settings.Quote.Garments.Add(new LandingGarmentOption
            {
                Label = "Ambos tipos",
                Value = "Mixta",
                IconClass = "fas fa-layer-group",
                IsMixed = true
            });
        }

        settings.Quote.Sports = settings.Quote.Sports
            .Where(s => !string.IsNullOrWhiteSpace(s.Label) && !string.IsNullOrWhiteSpace(s.Value))
            .Select(s =>
            {
                s.Label = s.Label.Trim();
                s.Value = s.Value.Trim();
                return s;
            })
            .ToList();

        if (settings.Quote.Sports.Count == 0)
        {
            settings.Quote.Sports = LandingSettingsData.CreateDefault().Quote.Sports;
        }

        settings.Quote.Sizes = settings.Quote.Sizes
            .Where(s => !string.IsNullOrWhiteSpace(s.Label) && !string.IsNullOrWhiteSpace(s.Value))
            .Select(s =>
            {
                s.Label = s.Label.Trim();
                s.Value = s.Value.Trim();
                return s;
            })
            .ToList();

        if (settings.Quote.Sizes.Count == 0)
        {
            settings.Quote.Sizes = LandingSettingsData.CreateDefault().Quote.Sizes;
        }

        return settings;
    }
}

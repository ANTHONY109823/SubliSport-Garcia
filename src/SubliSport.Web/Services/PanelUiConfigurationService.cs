using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Ui;
using SubliSport.Infrastructure.Data;

namespace SubliSport.Web.Services;

public class PanelUiConfigurationService(AppDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<PanelUiSettingsData> GetSettingsAsync()
    {
        try
        {
            var row = await db.PanelUiConfigurations.AsNoTracking().FirstOrDefaultAsync(p => p.Id == 1);
            if (row is null || string.IsNullOrWhiteSpace(row.JsonData))
            {
                return PanelUiSettingsData.CreateDefault();
            }

            return JsonSerializer.Deserialize<PanelUiSettingsData>(row.JsonData, JsonOptions)
                   ?? PanelUiSettingsData.CreateDefault();
        }
        catch
        {
            return PanelUiSettingsData.CreateDefault();
        }
    }

    public async Task SaveSettingsAsync(PanelUiSettingsData settings, string userId)
    {
        var normalized = Normalize(settings);
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        var row = await db.PanelUiConfigurations.FirstOrDefaultAsync(p => p.Id == 1);
        if (row is null)
        {
            db.PanelUiConfigurations.Add(new PanelUiConfiguration
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
        if (await db.PanelUiConfigurations.AnyAsync(p => p.Id == 1))
        {
            return;
        }

        var defaults = PanelUiSettingsData.CreateDefault();
        db.PanelUiConfigurations.Add(new PanelUiConfiguration
        {
            Id = 1,
            JsonData = JsonSerializer.Serialize(defaults, JsonOptions),
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static PanelUiSettingsData Normalize(PanelUiSettingsData settings)
    {
        var defaults = PanelUiSettingsData.CreateDefault();
        settings.AdminCreateOrder = NormalizeSection(settings.AdminCreateOrder, defaults.AdminCreateOrder, includeLists: true);
        settings.AdminOrderDetail = NormalizeSection(settings.AdminOrderDetail, defaults.AdminOrderDetail);
        settings.DesignerOrderDetail = NormalizeSection(settings.DesignerOrderDetail, defaults.DesignerOrderDetail);
        settings.ProductionOrderDetail = NormalizeSection(settings.ProductionOrderDetail, defaults.ProductionOrderDetail);
        return settings;
    }

    private static PanelSectionSettings NormalizeSection(
        PanelSectionSettings section,
        PanelSectionSettings defaults,
        bool includeLists = false)
    {
        section.Fields = section.Fields
            .Where(f => !string.IsNullOrWhiteSpace(f.Key))
            .Select(f => new UiFieldSetting
            {
                Key = f.Key.Trim(),
                Label = string.IsNullOrWhiteSpace(f.Label) ? f.Key.Trim() : f.Label.Trim(),
                Visible = f.Visible
            })
            .ToList();

        if (section.Fields.Count == 0)
        {
            section.Fields = defaults.Fields;
        }

        if (includeLists)
        {
            section.GarmentTypes = section.GarmentTypes
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Select(g => g.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            section.Sports = section.Sports
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (section.GarmentTypes.Count == 0)
            {
                section.GarmentTypes = defaults.GarmentTypes;
            }

            if (section.Sports.Count == 0)
            {
                section.Sports = defaults.Sports;
            }
        }

        return section;
    }
}

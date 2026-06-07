namespace SubliSport.Domain.Production;

public class ProductionSettingsData
{
    public string GroupName { get; set; } = "Grupo Producción SubliSport";

    /// <summary>Enlace de invitación del grupo (https://chat.whatsapp.com/...).</summary>
    public string GroupInviteUrl { get; set; } = string.Empty;

    /// <summary>Teléfono de respaldo si aún no hay enlace de grupo configurado.</summary>
    public string? FallbackPhone { get; set; } = "51960840874";

    public string? Notes { get; set; }

    public static ProductionSettingsData CreateDefault() => new()
    {
        GroupName = "Grupo Producción SubliSport",
        FallbackPhone = "51960840874"
    };
}

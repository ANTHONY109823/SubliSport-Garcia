namespace SubliSport.Domain.Landing;

public static class LandingCompanyInfo
{
    public const string BusinessName = "SUBLISPORT GARCIA";
    public const string LegalName = "LIZARDO EPIFANIO GARCIA CCAYO";
    public const string Ruc = "10431866892";
    public const string Address = "Galería Llancama Int. 320 y 311, Av. Aviación con Isabel La Católica, La Victoria, Lima";
}

public static class LandingFabricCatalog
{
    public static readonly IReadOnlyList<(string Key, string Label)> Fabrics =
    [
        ("dry_fit", "DRY FIT"),
        ("win_fresch", "WIN FRESCH"),
        ("poly_exagonal", "POLY EXAGONAL"),
        ("puma", "PUMA"),
        ("gota", "GOTA"),
        ("sig_sag", "SIG SAG"),
        ("marathon", "MARATON"),
        ("micro_nike", "MICRO NIKE"),
        ("labrado_brillo", "LABRADO CON BRILLO")
    ];

    /// <summary>Claves antiguas (combinadas) → clave actual para pedidos ya registrados.</summary>
    public static readonly IReadOnlyDictionary<string, string> LegacyKeyMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["gota_sig_sag"] = "gota",
            ["marathon_micro"] = "marathon"
        };

    public static string NormalizeKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return Fabrics[0].Key;
        var k = key.Trim().ToLowerInvariant();
        if (LegacyKeyMap.TryGetValue(k, out var mapped)) return mapped;
        return Fabrics.Any(f => f.Key.Equals(k, StringComparison.OrdinalIgnoreCase)) ? k : Fabrics[0].Key;
    }

    public static string GetLabel(string key) =>
        Fabrics.FirstOrDefault(f => f.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Label
        ?? Fabrics[0].Label;
}

public record LandingQuoteLineItem(
    string Description,
    string Size,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record LandingQuoteResult(
    decimal Subtotal,
    decimal EmbroideryInsigniaTotal,
    decimal EmbroideryBrandTotal,
    decimal EmbroideryShortTotal,
    decimal Total,
    string FabricLabel,
    string GarmentCategory,
    List<LandingQuoteLineItem> Lines,
    string AdminSuggestionText,
    string ClientRequestText,
    string ClientProformaDraft);

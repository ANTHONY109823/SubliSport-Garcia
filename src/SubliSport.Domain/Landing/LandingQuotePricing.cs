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
        ("dry_fit", "DRY FIT / WIN FRESCH"),
        ("poly_exagonal", "POLY EXAGONAL"),
        ("puma", "PUMA"),
        ("gota_sig_sag", "GOTA / SIG SAG"),
        ("marathon_micro", "MARATON / MICRO NIKE"),
        ("labrado_brillo", "LABRADO CON BRILLO")
    ];
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
    string ProformaText,
    string WhatsAppSummary);

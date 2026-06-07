namespace SubliSport.Domain.Pricing;

public class FabricTypeConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Divisor { get; set; } = 3.6m;
    public decimal PricePerKg { get; set; } = 17m;
    public int Gm2 { get; set; } = 200;
}

public class ExtraMeterCostConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerMeter { get; set; }
}

public class GarmentPriceConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class PricingSettingsData
{
    public List<FabricTypeConfig> Fabrics { get; set; } = [];
    public decimal LaserPricePerMeter { get; set; } = 1.50m;
    public decimal PrintPressPricePerMeter { get; set; } = 8.00m;
    public List<ExtraMeterCostConfig> ExtraMeterCosts { get; set; } = [];
    public List<GarmentPriceConfig> Garments { get; set; } = [];
    public List<GarmentPriceConfig> Socks { get; set; } = [];

    public static PricingSettingsData CreateDefault() => new()
    {
        Fabrics =
        [
            new FabricTypeConfig { Id = 1, Name = "Marathón", Divisor = 3.6m, PricePerKg = 17m, Gm2 = 220 },
            new FabricTypeConfig { Id = 2, Name = "Dry", Divisor = 3.6m, PricePerKg = 18m, Gm2 = 180 },
            new FabricTypeConfig { Id = 3, Name = "Win", Divisor = 3.7m, PricePerKg = 17m, Gm2 = 200 },
            new FabricTypeConfig { Id = 4, Name = "RIP", Divisor = 3.6m, PricePerKg = 26m, Gm2 = 200 }
        ],
        LaserPricePerMeter = 1.50m,
        PrintPressPricePerMeter = 8.00m,
        Garments =
        [
            new GarmentPriceConfig { Id = 1, Name = "Polo / Camiseta", Price = 3m },
            new GarmentPriceConfig { Id = 2, Name = "Short", Price = 5m },
            new GarmentPriceConfig { Id = 3, Name = "Casaca / Buzo", Price = 12m },
            new GarmentPriceConfig { Id = 4, Name = "Pantalón largo", Price = 10m }
        ],
        Socks = [new GarmentPriceConfig { Id = 1, Name = "Medias", Price = 6.5m }]
    };
}

public record OrderPricingInput(
    int FabricTypeId,
    decimal FabricMeters,
    int? FabricTypeRipId,
    decimal FabricMetersRip,
    bool IncludesConfection,
    bool ServiceOnlyPrintPress,
    bool IsFullSet,
    int Quantity);

public record OrderPricingResult(
    decimal FabricCost,
    decimal FabricRipCost,
    decimal LaserCost,
    decimal PrintPressCost,
    decimal ExtraMeterCost,
    decimal ConfectionCost,
    decimal SuggestedTotal,
    decimal KilosMain,
    decimal KilosRip,
    string FabricName,
    string? FabricRipName);

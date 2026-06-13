namespace SubliSport.Web.Helpers;

using SubliSport.Domain.Enums;
using SubliSport.Domain.Landing;

public static class OrderFormOptions
{
    public const string DefaultSport = "Fútbol";

    public static readonly (string Value, string Label)[] GarmentChips =
    [
        ("Conjunto completo (camiseta + short + medias)", "Conjunto completo"),
        ("Solo camiseta", "Solo camiseta"),
        ("Short deportivo", "Short deportivo"),
        ("Mixta", "Ambos tipos")
    ];

    public static readonly string[] MixedGarmentItemTypes =
    [
        "Conjunto completo",
        "Polo / Camiseta sola",
        "Short / Pantaloneta",
        "Medias",
        "Otro"
    ];

    public static readonly (string Value, string Label)[] SizeRanges =
    [
        ("XS / S", "XS / S"),
        ("M / L", "M / L"),
        ("XL / XXL", "XL / XXL"),
        ("Tallas mixtas", "Tallas mixtas")
    ];

    public static readonly (string Key, string Label)[] Fabrics = LandingFabricCatalog.Fabrics
        .Select(f => (f.Key, f.Label))
        .ToArray();

    public static readonly (OrderPriority Value, string Label)[] PriorityQuick =
    [
        (OrderPriority.Normal, "Normal"),
        (OrderPriority.Alta, "Alta"),
        (OrderPriority.Urgente, "Urgente")
    ];
}

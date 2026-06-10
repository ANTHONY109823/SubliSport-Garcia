namespace SubliSport.Web.Helpers;

using SubliSport.Domain.Enums;

public static class OrderFormOptions
{
    public static readonly string[] GarmentTypes =
    [
        "Conjunto completo",
        "Camiseta",
        "Short / Pantaloneta",
        "Medias",
        "Mixta",
        "Otro"
    ];

    public static readonly string[] MixedGarmentItemTypes =
    [
        "Conjunto completo",
        "Polo / Camiseta sola",
        "Short / Pantaloneta",
        "Short falda",
        "Medias",
        "Otro"
    ];

    public static readonly string[] Sports =
    [
        "Fútbol",
        "Vóley",
        "Básquet",
        "Atletismo",
        "Otro"
    ];

    public static readonly (OrderPriority Value, string Label)[] PriorityQuick =
    [
        (OrderPriority.Normal, "Normal"),
        (OrderPriority.Alta, "Alta"),
        (OrderPriority.Urgente, "Urgente")
    ];
}

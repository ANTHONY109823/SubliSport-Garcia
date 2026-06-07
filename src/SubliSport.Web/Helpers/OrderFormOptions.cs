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

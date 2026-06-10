using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public static class GiftOptionHelper
{
    public static readonly (GiftOption Value, string Label)[] Options =
    [
        (GiftOption.None, "Sin obsequio"),
        (GiftOption.Pelota, "Pelota"),
        (GiftOption.Banderola, "Banderola"),
        (GiftOption.CamisetaExtra, "Camiseta extra")
    ];

    public static string GetLabel(GiftOption option) =>
        Options.FirstOrDefault(o => o.Value == option).Label ?? "—";
}

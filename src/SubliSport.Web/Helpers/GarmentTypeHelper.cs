namespace SubliSport.Web.Helpers;

public static class GarmentTypeHelper
{
    public sealed record GarmentOption(string Value, string Label);

    public static readonly GarmentOption[] AdminCreateOptions =
    [
        new("Conjunto completo (camiseta + short + medias)", "Conjunto completo (camiseta + short + medias)"),
        new("Solo camiseta", "Solo camiseta"),
        new("Short deportivo", "Short deportivo"),
        new("Mixta", "Ambos tipos"),
        new("Otro", "Otro")
    ];

    public static bool IsMixed(string? value) =>
        value?.Equals("Mixta", StringComparison.OrdinalIgnoreCase) == true;

    public static string GetLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "—";
        }

        if (IsMixed(value))
        {
            return "Ambos tipos";
        }

        return AdminCreateOptions
            .FirstOrDefault(o => o.Value.Equals(value, StringComparison.OrdinalIgnoreCase))?
            .Label ?? value;
    }
}

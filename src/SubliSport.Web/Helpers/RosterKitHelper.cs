namespace SubliSport.Web.Helpers;

public static class RosterKitHelper
{
    public const string Conjunto = "conjunto";
    public const string Camiseta = "camiseta";

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Conjunto;
        }

        var v = value.Trim().ToLowerInvariant()
            .Replace("ó", "o")
            .Replace("á", "a");

        if (v.Contains("camiseta") || v.Contains("polo") || v.Contains("sola") || v == "camiseta")
        {
            return Camiseta;
        }

        return Conjunto;
    }

    public static string GetDisplayLabel(string? kit) =>
        Normalize(kit) == Camiseta ? "Camiseta sola" : "Conjunto completo";

    public static string ResolveCategory(string? kit) =>
        Normalize(kit) == Camiseta ? "camiseta" : "conjunto";
}

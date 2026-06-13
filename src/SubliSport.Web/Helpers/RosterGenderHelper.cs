namespace SubliSport.Web.Helpers;

public static class RosterGenderHelper
{
    public const string Varon = "Varon";
    public const string Femenino = "Femenino";

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Varon;
        }

        var v = value.Trim().ToLowerInvariant()
            .Replace("ó", "o")
            .Replace("á", "a")
            .Replace("é", "e");

        return v is "f" or "femenino" or "mujer" or "fem" ? Femenino : Varon;
    }

    public static string GetCutLabel(string? gender) =>
        Normalize(gender) == Femenino ? "CORTE PRINCESA" : "VARON";

    public static bool IsFemenino(string? gender) => Normalize(gender) == Femenino;
}

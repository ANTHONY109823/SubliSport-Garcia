namespace SubliSport.Web.Helpers;

public static class PricingDisplayHelper
{
    public const decimal IgvRate = 0.18m;

    public static decimal CalculateIgv(decimal baseAmount) =>
        Math.Round(baseAmount * IgvRate, 2, MidpointRounding.AwayFromZero);

    public static decimal TotalWithIgv(decimal baseAmount, bool includesIgv) =>
        includesIgv ? baseAmount + CalculateIgv(baseAmount) : baseAmount;
}

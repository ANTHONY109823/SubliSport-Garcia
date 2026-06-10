using SubliSport.Domain.Entities;
using SubliSport.Domain.Pricing;

namespace SubliSport.Web.Services;

public static class PricingCalculatorService
{
    public static OrderPricingResult Calculate(PricingSettingsData settings, OrderPricingInput input)
    {
        var fabric = settings.Fabrics.FirstOrDefault(f => f.Id == input.FabricTypeId)
                     ?? settings.Fabrics.FirstOrDefault()
                     ?? new FabricTypeConfig();

        var fabricRip = input.FabricTypeRipId.HasValue
            ? settings.Fabrics.FirstOrDefault(f => f.Id == input.FabricTypeRipId.Value)
            : null;

        var meters = input.FabricMeters;
        var metersRip = input.FabricMetersRip;

        var kilosMain = GetKilos(fabric, meters);
        var kilosRip = fabricRip is not null ? GetKilos(fabricRip, metersRip) : 0m;

        var fabricCost = fabric.Divisor > 0 && kilosMain > 0
            ? (kilosMain / fabric.Divisor) * fabric.PricePerKg
            : 0m;

        var fabricRipCost = fabricRip is not null && fabricRip.Divisor > 0 && kilosRip > 0
            ? (kilosRip / fabricRip.Divisor) * fabricRip.PricePerKg
            : 0m;

        var laserCost = input.IncludesLaserCut ? meters * settings.LaserPricePerMeter : 0m;
        var printPressCost = meters * settings.PrintPressPricePerMeter;
        var extraCost = settings.ExtraMeterCosts.Sum(x => meters * x.PricePerMeter);

        var excludeFabric = input.ServiceOnlyPrintPress || input.ClientOwnFabric;

        decimal confectionCost = 0m;
        if (!input.ServiceOnlyPrintPress && input.IncludesConfection)
        {
            var shirt = settings.Garments.FirstOrDefault(g =>
                g.Name.Contains("Polo", StringComparison.OrdinalIgnoreCase) ||
                g.Name.Contains("Camiseta", StringComparison.OrdinalIgnoreCase))?.Price ?? 3m;
            var shortPrice = settings.Garments.FirstOrDefault(g =>
                g.Name.Contains("Short", StringComparison.OrdinalIgnoreCase))?.Price ?? 5m;
            var socks = settings.Socks.Sum(s => s.Price);

            confectionCost = shirt;
            if (input.IsFullSet)
            {
                confectionCost += shortPrice + socks;
            }

            confectionCost *= Math.Max(input.Quantity, 1);
        }

        var fabricTotal = excludeFabric ? 0m : fabricCost + fabricRipCost;
        var suggested = fabricTotal + laserCost + printPressCost + extraCost + confectionCost;

        return new OrderPricingResult(
            FabricCost: Round(fabricCost),
            FabricRipCost: Round(fabricRipCost),
            LaserCost: Round(laserCost),
            PrintPressCost: Round(printPressCost),
            ExtraMeterCost: Round(extraCost),
            ConfectionCost: Round(confectionCost),
            SuggestedTotal: Round(suggested),
            KilosMain: Round(kilosMain, 3),
            KilosRip: Round(kilosRip, 3),
            FabricName: fabric.Name,
            FabricRipName: fabricRip?.Name);
    }

    public static OrderPricingInput BuildInputFromOrder(Order order) => new(
        order.FabricTypeId ?? 1,
        order.FabricMeters ?? 0,
        order.FabricTypeRipId,
        order.FabricMetersRip ?? 0,
        order.IncludesConfection,
        order.ServiceOnlyPrintPress,
        order.ClientOwnFabric,
        order.IncludesLaserCut,
        order.GarmentType.Contains("Conjunto", StringComparison.OrdinalIgnoreCase) ||
        order.GarmentType.Equals("Mixta", StringComparison.OrdinalIgnoreCase),
        order.Quantity);

    private static decimal GetKilos(FabricTypeConfig fabric, decimal meters) =>
        meters * fabric.Gm2 / 1000m;

    private static decimal Round(decimal value, int decimals = 2) =>
        Math.Round(value, decimals, MidpointRounding.AwayFromZero);
}

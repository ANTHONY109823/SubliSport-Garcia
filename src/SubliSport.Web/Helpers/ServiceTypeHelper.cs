using SubliSport.Domain.Entities;

namespace SubliSport.Web.Helpers;

public static class ServiceTypeHelper
{
    public const string PrintPressOnly = "print_press";
    public const string WithLaserCut = "with_laser";
    public const string WithConfection = "with_confection";

    /// <summary>Orden del flujo de producción: impresión → láser → confección.</summary>
    public static readonly (string Key, string Icon, string Title, string Description)[] Options =
    [
        (PrintPressOnly, "🖨️", "Impresión + planchado", "Paso 1 — diseño, impresión y planchado de tela."),
        (WithLaserCut, "⚡", "Corte láser", "Paso 2 — corte láser de piezas."),
        (WithConfection, "✂️", "Confección", "Paso 3 — armado de polo, short y medias.")
    ];

    public static (bool PrintPress, bool LaserCut, bool Confection) FromOrder(Order order) =>
        (
            PrintPress: order.ServiceOnlyPrintPress || order.IncludesConfection,
            LaserCut: order.IncludesLaserCut,
            Confection: order.IncludesConfection
        );

    public static void ApplyServiceFlags(Order order, bool printPress, bool laserCut, bool confection)
    {
        order.IncludesLaserCut = laserCut;
        order.IncludesConfection = confection;
        order.ServiceOnlyPrintPress = printPress && !confection;
    }

    public static bool HasAnyService(bool printPress, bool laserCut, bool confection) =>
        printPress || laserCut || confection;

    public static string GetLabel(Order order)
    {
        if (order.PricingTier == Domain.Enums.ClientPricingTier.DirectRetail)
        {
            return "Venta directa (precio por prenda)";
        }

        var (printPress, laserCut, confection) = FromOrder(order);
        return BuildServiceLabel(printPress, laserCut, confection);
    }

    public static string GetShortLabel(Order order)
    {
        if (order.PricingTier == Domain.Enums.ClientPricingTier.DirectRetail)
        {
            return "Directo";
        }

        var (printPress, laserCut, confection) = FromOrder(order);
        var parts = new List<string>();
        if (printPress || confection) parts.Add("Impresión");
        if (laserCut) parts.Add("Láser");
        if (confection) parts.Add("Confección");
        return parts.Count == 0 ? "Sin servicio" : string.Join(" + ", parts);
    }

    public static string BuildServiceLabel(bool printPress, bool laserCut, bool confection)
    {
        var steps = new List<string>();
        if (printPress || confection) steps.Add("impresión + planchado");
        if (laserCut) steps.Add("corte láser");
        if (confection) steps.Add("confección");
        return steps.Count == 0
            ? "Servicio sin etapas seleccionadas"
            : $"Servicio: diseño + {string.Join(" + ", steps)}";
    }
}

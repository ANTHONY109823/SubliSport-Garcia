using SubliSport.Domain.Entities;

namespace SubliSport.Web.Helpers;

public static class ServiceTypeHelper
{
    public const string PrintPressOnly = "print_press";
    public const string WithConfection = "with_confection";

    public static string GetLabel(Order order)
    {
        if (order.PricingTier == Domain.Enums.ClientPricingTier.DirectRetail)
        {
            return "Venta directa (precio por prenda)";
        }

        if (order.ServiceOnlyPrintPress || !order.IncludesConfection)
        {
            return "Servicio: diseño + impresión + planchado";
        }

        return "Servicio: diseño + impresión + planchado + confección";
    }

    public static string GetShortLabel(Order order)
    {
        if (order.PricingTier == Domain.Enums.ClientPricingTier.DirectRetail)
        {
            return "Directo";
        }

        return order.IncludesConfection && !order.ServiceOnlyPrintPress
            ? "Con confección"
            : "Solo impresión";
    }

    public static void ApplyServiceType(Order order, string serviceType)
    {
        if (serviceType == PrintPressOnly)
        {
            order.ServiceOnlyPrintPress = true;
            order.IncludesConfection = false;
        }
        else
        {
            order.ServiceOnlyPrintPress = false;
            order.IncludesConfection = true;
        }
    }
}

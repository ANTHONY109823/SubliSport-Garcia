using SubliSport.Domain.Entities;

namespace SubliSport.Web.Helpers;

public static class ServiceTypeHelper
{
    public const string PrintPressOnly = "print_press";
    public const string WithConfection = "with_confection";
    public const string WithLaserCut = "with_laser";

    public static readonly (string Key, string Icon, string Title, string Description)[] Options =
    [
        (PrintPressOnly, "🖨️", "Solo impresión + planchado", "Sin confección — tela del taller o del cliente."),
        (WithConfection, "✂️", "Con confección", "Incluye armado de polo, short y medias según pedido."),
        (WithLaserCut, "⚡", "Corte láser", "Impresión + planchado + corte láser — sin confección.")
    ];

    public static string GetLabel(Order order)
    {
        if (order.PricingTier == Domain.Enums.ClientPricingTier.DirectRetail)
        {
            return "Venta directa (precio por prenda)";
        }

        return ResolveFromOrder(order) switch
        {
            WithConfection => "Servicio: diseño + impresión + planchado + confección",
            WithLaserCut => "Servicio: diseño + impresión + planchado + corte láser",
            _ => "Servicio: diseño + impresión + planchado"
        };
    }

    public static string GetShortLabel(Order order)
    {
        if (order.PricingTier == Domain.Enums.ClientPricingTier.DirectRetail)
        {
            return "Directo";
        }

        return ResolveFromOrder(order) switch
        {
            WithConfection => "Con confección",
            WithLaserCut => "Corte láser",
            _ => "Solo impresión"
        };
    }

    public static string ResolveFromOrder(Order order)
    {
        if (order.IncludesConfection && !order.ServiceOnlyPrintPress)
        {
            return WithConfection;
        }

        if (order.IncludesLaserCut)
        {
            return WithLaserCut;
        }

        return PrintPressOnly;
    }

    public static void ApplyServiceType(Order order, string serviceType)
    {
        switch (serviceType)
        {
            case WithConfection:
                order.ServiceOnlyPrintPress = false;
                order.IncludesConfection = true;
                order.IncludesLaserCut = false;
                break;
            case WithLaserCut:
                order.ServiceOnlyPrintPress = true;
                order.IncludesConfection = false;
                order.IncludesLaserCut = true;
                break;
            default:
                order.ServiceOnlyPrintPress = true;
                order.IncludesConfection = false;
                order.IncludesLaserCut = false;
                break;
        }
    }
}

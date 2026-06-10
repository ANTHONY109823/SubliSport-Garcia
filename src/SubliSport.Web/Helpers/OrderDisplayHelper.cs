using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public static class OrderDisplayHelper
{
    public static string GetStatusLabel(Order order)
    {
        if (DesignerOrderHelper.IsPendingClientApproval(order))
        {
            return "PENDIENTE DE APROBACIÓN";
        }

        return order.Status switch
        {
            OrderStatus.AsignadoDiseno when !DesignerOrderHelper.IsAccepted(order) => "Por aceptar (diseño)",
            OrderStatus.AsignadoDiseno => "Diseño — por iniciar",
            OrderStatus.EnDiseno => "En diseño",
            _ => OrderStatusHelper.GetLabel(order.Status)
        };
    }

    public static string GetGarmentDisplay(Order order)
    {
        if (GarmentTypeHelper.IsMixed(order.GarmentType))
        {
            return $"Ambos tipos — {MixedGarmentHelper.FormatSummary(order.MixedGarmentDetails)}";
        }

        return GarmentTypeHelper.GetLabel(order.GarmentType);
    }

    public static string FormatFabricMeters(Order order)
    {
        if (!order.FabricMeters.HasValue || order.FabricMeters <= 0)
        {
            return "—";
        }

        var text = $"{order.FabricMeters.Value:N2} m";
        if (order.FabricMetersRip is > 0)
        {
            text += $" · RIP {order.FabricMetersRip.Value:N2} m";
        }

        return text;
    }
}

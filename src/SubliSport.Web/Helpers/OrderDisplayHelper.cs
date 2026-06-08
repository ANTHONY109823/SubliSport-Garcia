using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public static class OrderDisplayHelper
{
    public static string GetStatusLabel(Order order)
    {
        if (DesignerOrderHelper.IsPendingClientApproval(order))
        {
            return "Diseño — pendiente cliente";
        }

        return order.Status switch
        {
            OrderStatus.AsignadoDiseno when !DesignerOrderHelper.IsAccepted(order) => "Por aceptar (diseño)",
            OrderStatus.AsignadoDiseno => "Diseño — por iniciar",
            OrderStatus.EnDiseno => "En diseño",
            _ => OrderStatusHelper.GetLabel(order.Status)
        };
    }
}

using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public record JourneyMilestone(string Area, DateTime At);

public static class OrderJourneyHelper
{
    public static List<JourneyMilestone> BuildMilestones(Order order)
    {
        var result = new List<JourneyMilestone>();
        var history = order.StatusHistory.OrderBy(h => h.ChangedAt).ToList();

        if (history.Count == 0)
        {
            result.Add(new JourneyMilestone("Administración", order.CreatedAt));
            return result;
        }

        string? lastArea = null;
        foreach (var item in history)
        {
            var area = ResolveArea(item, order);
            if (area == lastArea)
            {
                continue;
            }

            lastArea = area;
            result.Add(new JourneyMilestone(area, item.ChangedAt));
        }

        if (DesignerOrderHelper.IsPendingClientApproval(order))
        {
            var pendingAt = order.ClientApprovalPendingAt ?? history.Last().ChangedAt;
            if (result.Count == 0 || result[^1].Area != "Esperando cliente")
            {
                if (result.Count > 0 && result[^1].At == pendingAt && result[^1].Area == "Diseño")
                {
                    result[^1] = new JourneyMilestone("Esperando cliente", pendingAt);
                }
                else
                {
                    result.Add(new JourneyMilestone("Esperando cliente", pendingAt));
                }
            }
        }

        return result;
    }

    private static string ResolveArea(OrderStatusHistory item, Order order)
    {
        if (item.Comment != null &&
            item.Comment.Contains("pendiente aprobación", StringComparison.OrdinalIgnoreCase))
        {
            return "Esperando cliente";
        }

        if (item.Comment != null &&
            item.Comment.Contains("cliente respondió", StringComparison.OrdinalIgnoreCase))
        {
            return "Diseño";
        }

        return item.ToStatus switch
        {
            OrderStatus.CotizacionRecibida or OrderStatus.EnRevision or OrderStatus.AsignadoDiseno => "Administración",
            OrderStatus.EnDiseno => "Diseño",
            OrderStatus.DisenoAprobado or OrderStatus.EnImpresion or OrderStatus.EnPlanchado
                or OrderStatus.EnConfeccion or OrderStatus.ListoEntrega => "Producción",
            OrderStatus.Entregado => "Entrega",
            OrderStatus.Cancelado => "Cancelado",
            _ => OrderStatusHelper.GetLabel(item.ToStatus)
        };
    }
}

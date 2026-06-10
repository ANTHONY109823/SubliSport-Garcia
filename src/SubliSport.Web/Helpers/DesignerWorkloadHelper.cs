using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public sealed record DesignerWorkloadInfo(int SinAceptar, int Pendientes, int TotalPendientes);

public static class DesignerWorkloadHelper
{
    public static readonly DesignerWorkloadInfo Empty = new(0, 0, 0);

    public static Dictionary<string, DesignerWorkloadInfo> BuildWorkloads(
        IEnumerable<Order> orders,
        IEnumerable<string> designerIds)
    {
        var list = orders.ToList();
        return designerIds.ToDictionary(
            id => id,
            id => ForDesigner(id, list));
    }

    public static DesignerWorkloadInfo ForDesigner(string designerId, IEnumerable<Order> orders)
    {
        var mine = orders
            .Where(o => o.AssignedDesignerId == designerId &&
                        o.Status is not OrderStatus.Entregado and not OrderStatus.Cancelado)
            .ToList();

        var sinAceptar = mine.Count(DesignerOrderHelper.CanAccept);
        var pendientes = mine.Count(o =>
            DesignerOrderHelper.CanStart(o) ||
            o.Status == OrderStatus.EnDiseno ||
            DesignerOrderHelper.IsPendingClientApproval(o));

        return new DesignerWorkloadInfo(sinAceptar, pendientes, sinAceptar + pendientes);
    }
}

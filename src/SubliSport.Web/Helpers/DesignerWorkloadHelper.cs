using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public sealed record DesignerWorkloadInfo(
    int SinAceptar,
    int PorIniciar,
    int EnDiseno,
    int AprobacionCliente)
{
    public int TotalPendientes => SinAceptar + PorIniciar + EnDiseno + AprobacionCliente;
    public int Pendientes => PorIniciar + EnDiseno + AprobacionCliente;
}

public static class DesignerWorkloadHelper
{
    public static readonly DesignerWorkloadInfo Empty = new(0, 0, 0, 0);

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
        var porIniciar = mine.Count(DesignerOrderHelper.CanStart);
        var enDiseno = mine.Count(o =>
            o.Status == OrderStatus.EnDiseno && !DesignerOrderHelper.IsPendingClientApproval(o));
        var aprobacionCliente = mine.Count(DesignerOrderHelper.IsPendingClientApproval);

        return new DesignerWorkloadInfo(sinAceptar, porIniciar, enDiseno, aprobacionCliente);
    }
}

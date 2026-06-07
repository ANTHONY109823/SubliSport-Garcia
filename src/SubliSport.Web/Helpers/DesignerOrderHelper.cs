using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public static class DesignerOrderHelper
{
    public const string AcceptedComment = "Pedido aceptado por diseñador";
    public const string StartedComment = "Trabajo iniciado en diseño";
    public const string FinishedComment = "Diseño finalizado — enviado a impresión/producción";

    public static bool IsAccepted(Order order) =>
        order.StatusHistory.Any(h =>
            h.Comment != null &&
            h.Comment.Contains("aceptado", StringComparison.OrdinalIgnoreCase));

    public static bool CanAccept(Order order) =>
        order.Status == OrderStatus.AsignadoDiseno && !IsAccepted(order);

    public static bool CanStart(Order order) =>
        order.Status == OrderStatus.AsignadoDiseno && IsAccepted(order);

    public static bool CanFinish(Order order) =>
        order.Status == OrderStatus.EnDiseno;

    public static bool CanReturn(Order order) =>
        order.Status is OrderStatus.AsignadoDiseno or OrderStatus.EnDiseno;

    public static bool IsDesignPhaseComplete(Order order) =>
        order.Status is OrderStatus.DisenoAprobado or OrderStatus.EnImpresion or OrderStatus.EnPlanchado
            or OrderStatus.EnConfeccion or OrderStatus.ListoEntrega or OrderStatus.Entregado;

    public static bool IsPending(Order order) =>
        order.Status is OrderStatus.AsignadoDiseno or OrderStatus.EnDiseno;

    public static string GetDesignerStageLabel(Order order) => order.Status switch
    {
        OrderStatus.AsignadoDiseno when !IsAccepted(order) => "Nuevo — por aceptar",
        OrderStatus.AsignadoDiseno => "Aceptado — por iniciar",
        OrderStatus.EnDiseno => "En diseño",
        OrderStatus.DisenoAprobado => "Enviado a producción",
        _ when IsDesignPhaseComplete(order) => "Culminado",
        _ => OrderStatusHelper.GetLabel(order.Status)
    };
}

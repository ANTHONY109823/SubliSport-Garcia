using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public static class ProduccionOrderHelper
{
    public const string AcceptedComment = "Pedido aceptado por producción";
    public const string StartedComment = "Trabajo iniciado en producción";
    public const string ReadyPickupComment = "Trabajo finalizado — pendiente por recoger";

    public static bool IsAccepted(Order order) =>
        order.ProductionAcceptedAt.HasValue ||
        order.StatusHistory.Any(h =>
            h.Comment != null &&
            (h.Comment.Contains("aceptado por producción", StringComparison.OrdinalIgnoreCase) ||
             h.Comment.Contains("aceptado por impresión", StringComparison.OrdinalIgnoreCase)));

    public static bool CanAccept(Order order) =>
        order.Status == OrderStatus.DisenoAprobado && !IsAccepted(order);

    public static bool CanStart(Order order) =>
        order.Status == OrderStatus.DisenoAprobado && IsAccepted(order);

    public static bool CanSetStage(Order order) =>
        order.Status is OrderStatus.EnImpresion or OrderStatus.EnPlanchado or OrderStatus.EnConfeccion;

    public static bool IsOnStage(Order order, OrderStatus stage) => order.Status == stage;

    public static bool CanMarkReadyForPickup(Order order)
    {
        if (order.Status == OrderStatus.ListoEntrega || order.Status == OrderStatus.Entregado)
        {
            return false;
        }

        if (order.ServiceOnlyPrintPress || !order.IncludesConfection)
        {
            return order.Status is OrderStatus.EnImpresion or OrderStatus.EnPlanchado;
        }

        return order.Status == OrderStatus.EnConfeccion;
    }

    public static bool IsActive(Order order) =>
        order.Status is OrderStatus.DisenoAprobado or OrderStatus.EnImpresion
            or OrderStatus.EnPlanchado or OrderStatus.EnConfeccion;

    public static bool IsCompleted(Order order) =>
        order.Status is OrderStatus.ListoEntrega or OrderStatus.Entregado;

    public static string GetStageLabel(Order order) => order.Status switch
    {
        OrderStatus.DisenoAprobado when !IsAccepted(order) => "Nuevo — por aceptar",
        OrderStatus.DisenoAprobado => "Aceptado — por iniciar",
        OrderStatus.EnImpresion => "Impresión",
        OrderStatus.EnPlanchado => "Planchado",
        OrderStatus.EnConfeccion => "Confección",
        OrderStatus.ListoEntrega => "Pendiente por recoger",
        OrderStatus.Entregado => "Entregado",
        _ => OrderStatusHelper.GetLabel(order.Status)
    };
}

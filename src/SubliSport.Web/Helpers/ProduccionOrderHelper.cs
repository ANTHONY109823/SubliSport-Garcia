using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public static class ProduccionOrderHelper
{
    public const string AcceptedComment = "Pedido aceptado por producción";
    public const string ImpresionStartedComment = "Impresión iniciada";
    public const string ImpresionFinishedComment = "Impresión finalizada";
    public const string PlanchadoStartedComment = "Enviado a planchado — planchado iniciado";
    public const string PlanchadoFinishedComment = "Planchado finalizado";
    public const string ConfeccionStartedComment = "Confección iniciada";
    public const string ConfeccionFinishedComment = "Confección finalizada";
    public const string ReadyPickupComment = "Pedido listo para entrega — pendiente por recoger";
    public const string DeliveredComment = "Pedido entregado al cliente";

    public static bool RequiresConfection(Order order) =>
        order.IncludesConfection && !order.ServiceOnlyPrintPress;

    public static ProductionSubStage GetEffectiveSubStage(Order order)
    {
        if (order.ProductionSubStage != ProductionSubStage.None)
        {
            return order.ProductionSubStage;
        }

        return order.Status switch
        {
            OrderStatus.EnImpresion => ProductionSubStage.ImpresionEnCurso,
            OrderStatus.EnPlanchado => ProductionSubStage.PlanchadoEnCurso,
            OrderStatus.EnConfeccion => ProductionSubStage.ConfeccionEnCurso,
            _ => ProductionSubStage.None
        };
    }

    public static bool IsAccepted(Order order) =>
        order.ProductionAcceptedAt.HasValue ||
        order.StatusHistory.Any(h =>
            h.Comment != null &&
            (h.Comment.Contains("aceptado por producción", StringComparison.OrdinalIgnoreCase) ||
             h.Comment.Contains("aceptado por impresión", StringComparison.OrdinalIgnoreCase)));

    public static bool CanAccept(Order order) =>
        order.Status == OrderStatus.DisenoAprobado && !IsAccepted(order);

    public static bool CanStartImpresion(Order order)
    {
        var sub = GetEffectiveSubStage(order);
        return IsAccepted(order)
               && order.Status == OrderStatus.DisenoAprobado
               && sub == ProductionSubStage.None;
    }

    public static bool CanFinishImpresion(Order order) =>
        order.Status == OrderStatus.EnImpresion
        && GetEffectiveSubStage(order) == ProductionSubStage.ImpresionEnCurso;

    public static bool IsImpresionComplete(Order order) =>
        GetEffectiveSubStage(order) is ProductionSubStage.ImpresionLista
            or ProductionSubStage.PlanchadoEnCurso
            or ProductionSubStage.PlanchadoListo
            or ProductionSubStage.ConfeccionEnCurso
            or ProductionSubStage.ConfeccionListo;

    public static bool CanStartPlanchado(Order order) =>
        order.Status == OrderStatus.EnImpresion
        && GetEffectiveSubStage(order) == ProductionSubStage.ImpresionLista;

    public static bool CanFinishPlanchado(Order order) =>
        order.Status == OrderStatus.EnPlanchado
        && GetEffectiveSubStage(order) == ProductionSubStage.PlanchadoEnCurso;

    public static bool IsPlanchadoComplete(Order order) =>
        GetEffectiveSubStage(order) is ProductionSubStage.PlanchadoListo
            or ProductionSubStage.ConfeccionEnCurso
            or ProductionSubStage.ConfeccionListo;

    public static bool CanStartConfeccion(Order order) =>
        RequiresConfection(order)
        && order.Status == OrderStatus.EnPlanchado
        && GetEffectiveSubStage(order) == ProductionSubStage.PlanchadoListo;

    public static bool CanFinishConfeccion(Order order) =>
        RequiresConfection(order)
        && order.Status == OrderStatus.EnConfeccion
        && GetEffectiveSubStage(order) == ProductionSubStage.ConfeccionEnCurso;

    public static bool CanMarkDelivered(Order order) =>
        order.Status == OrderStatus.ListoEntrega;

    public static bool CanMarkReadyForPickup(Order order)
    {
        if (order.Status is OrderStatus.ListoEntrega or OrderStatus.Entregado)
        {
            return false;
        }

        var sub = GetEffectiveSubStage(order);

        if (RequiresConfection(order))
        {
            return order.Status == OrderStatus.EnConfeccion && sub == ProductionSubStage.ConfeccionListo;
        }

        return order.Status == OrderStatus.EnPlanchado && sub == ProductionSubStage.PlanchadoListo;
    }

    public static bool IsActive(Order order) =>
        order.Status is OrderStatus.DisenoAprobado or OrderStatus.EnImpresion
            or OrderStatus.EnPlanchado or OrderStatus.EnConfeccion;

    public static bool IsCompleted(Order order) =>
        order.Status is OrderStatus.ListoEntrega or OrderStatus.Entregado;

    public static string GetStageLabel(Order order)
    {
        if (order.Status == OrderStatus.DisenoAprobado && !IsAccepted(order))
        {
            return "Nuevo — por aceptar";
        }

        if (order.Status == OrderStatus.DisenoAprobado && CanStartImpresion(order))
        {
            return "Aceptado — por iniciar impresión";
        }

        return (order.Status, GetEffectiveSubStage(order)) switch
        {
            (OrderStatus.EnImpresion, ProductionSubStage.ImpresionEnCurso) => "Impresión en curso",
            (OrderStatus.EnImpresion, ProductionSubStage.ImpresionLista) => "Impresión finalizada",
            (OrderStatus.EnPlanchado, ProductionSubStage.PlanchadoEnCurso) => "Planchado en curso",
            (OrderStatus.EnPlanchado, ProductionSubStage.PlanchadoListo) => "Planchado finalizado",
            (OrderStatus.EnConfeccion, ProductionSubStage.ConfeccionEnCurso) => "Confección en curso",
            (OrderStatus.EnConfeccion, ProductionSubStage.ConfeccionListo) => "Confección finalizada",
            (OrderStatus.ListoEntrega, _) => "Pendiente por recoger",
            (OrderStatus.Entregado, _) => "Entregado",
            _ => OrderStatusHelper.GetLabel(order.Status)
        };
    }
}

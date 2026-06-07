using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public static class OrderStatusHelper
{
    public static string GetLabel(OrderStatus status) => status switch
    {
        OrderStatus.CotizacionRecibida => "Cotización recibida",
        OrderStatus.EnRevision => "En revisión",
        OrderStatus.AsignadoDiseno => "Asignado a diseño",
        OrderStatus.EnDiseno => "En diseño",
        OrderStatus.DisenoAprobado => "Diseño aprobado",
        OrderStatus.EnImpresion => "En impresión",
        OrderStatus.EnPlanchado => "En planchado",
        OrderStatus.EnConfeccion => "En confección",
        OrderStatus.ListoEntrega => "Listo para entrega",
        OrderStatus.Entregado => "Entregado",
        OrderStatus.Cancelado => "Cancelado",
        _ => status.ToString()
    };

    public static string GetPriorityLabel(OrderPriority priority) => priority switch
    {
        OrderPriority.Baja => "Baja",
        OrderPriority.Normal => "Normal",
        OrderPriority.Alta => "Alta",
        OrderPriority.Urgente => "Urgente",
        _ => priority.ToString()
    };
}

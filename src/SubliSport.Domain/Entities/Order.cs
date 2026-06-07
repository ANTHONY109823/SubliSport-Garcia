using SubliSport.Domain.Enums;

namespace SubliSport.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? ClientPhone { get; set; }
    public string? ClientEmail { get; set; }
    public string GarmentType { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? SizeRange { get; set; }
    public string? Notes { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.CotizacionRecibida;
    public OrderPriority Priority { get; set; } = OrderPriority.Normal;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AgreedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public string? AssignedDesignerId { get; set; }
    public ApplicationUser? AssignedDesigner { get; set; }

    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = [];
}

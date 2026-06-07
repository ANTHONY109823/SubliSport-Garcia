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

    public DateTime? ProductionAcceptedAt { get; set; }
    public string? ProductionAcceptedByUserId { get; set; }
    public ProductionSubStage ProductionSubStage { get; set; } = ProductionSubStage.None;

    public int? FabricTypeId { get; set; }
    public string? FabricTypeName { get; set; }
    public int? FabricTypeRipId { get; set; }
    public string? FabricTypeRipName { get; set; }
    public decimal? FabricMeters { get; set; }
    public decimal? FabricMetersRip { get; set; }
    public bool IncludesConfection { get; set; } = true;
    public bool ServiceOnlyPrintPress { get; set; }

    public decimal? CalculatedFabricCost { get; set; }
    public decimal? CalculatedFabricRipCost { get; set; }
    public decimal? CalculatedLaserCost { get; set; }
    public decimal? CalculatedPrintPressCost { get; set; }
    public decimal? CalculatedExtraCost { get; set; }
    public decimal? CalculatedConfectionCost { get; set; }
    public decimal CalculatedTotal { get; set; }
    public decimal ChargeAmount { get; set; }
    public string? PricingNotes { get; set; }
    public DateTime? PricingUpdatedAt { get; set; }

    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = [];
}

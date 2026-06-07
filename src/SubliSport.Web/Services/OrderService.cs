using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;
using SubliSport.Infrastructure.Data;

using SubliSport.Domain.Pricing;
using SubliSport.Web.Helpers;

namespace SubliSport.Web.Services;

public class OrderService(AppDbContext db)
{
    public async Task<List<Order>> GetOrdersForUserAsync(string userId, IEnumerable<string> roles)
    {
        var query = db.Orders
            .Include(o => o.AssignedDesigner)
            .Include(o => o.CreatedByUser)
            .AsQueryable();

        if (roles.Contains(AppRoles.Admin))
        {
            return await query
                .OrderByDescending(o => o.Priority)
                .ThenBy(o => o.AgreedDeliveryDate)
                .ThenBy(o => o.CreatedAt)
                .ToListAsync();
        }

        if (roles.Contains(AppRoles.Designer))
        {
            return await query
                .Include(o => o.StatusHistory)
                .Where(o => o.AssignedDesignerId == userId)
                .OrderByDescending(o => o.Priority)
                .ThenBy(o => o.AgreedDeliveryDate)
                .ToListAsync();
        }

        if (roles.Contains(AppRoles.Production))
        {
            var productionStatuses = new[]
            {
                OrderStatus.DisenoAprobado,
                OrderStatus.EnImpresion,
                OrderStatus.EnPlanchado,
                OrderStatus.EnConfeccion,
                OrderStatus.ListoEntrega
            };

            return await query
                .Include(o => o.StatusHistory)
                .Where(o => productionStatuses.Contains(o.Status))
                .OrderByDescending(o => o.Priority)
                .ThenBy(o => o.AgreedDeliveryDate)
                .ToListAsync();
        }

        return [];
    }

    public async Task<Order?> GetByIdAsync(Guid id) =>
        await db.Orders
            .Include(o => o.AssignedDesigner)
            .Include(o => o.CreatedByUser)
            .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order> CreateManualOrderAsync(Order order, string createdByUserId)
    {
        order.Id = Guid.NewGuid();
        order.OrderNumber = await GenerateOrderNumberAsync();
        order.CreatedByUserId = createdByUserId;
        order.CreatedAt = DateTime.UtcNow;
        order.Status = OrderStatus.CotizacionRecibida;

        db.Orders.Add(order);
        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = OrderStatus.CotizacionRecibida,
            ToStatus = OrderStatus.CotizacionRecibida,
            ChangedByUserId = createdByUserId,
            Comment = "Pedido registrado manualmente"
        });

        await db.SaveChangesAsync();
        return order;
    }

    public async Task AssignDesignerAsync(Guid orderId, string designerId, string adminUserId)
    {
        var order = await db.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException("Pedido no encontrado.");

        var previous = order.Status;
        order.AssignedDesignerId = designerId;
        order.Status = OrderStatus.AsignadoDiseno;

        await AddHistoryAsync(order, previous, OrderStatus.AsignadoDiseno, adminUserId, "Diseñador asignado");
        await db.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus, string userId, string? comment = null)
    {
        var order = await db.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException("Pedido no encontrado.");

        var previous = order.Status;
        order.Status = newStatus;

        if (newStatus == OrderStatus.Entregado)
        {
            order.DeliveredAt = DateTime.UtcNow;
        }

        await AddHistoryAsync(order, previous, newStatus, userId, comment);
        await db.SaveChangesAsync();
    }

    public async Task DesignerAcceptAsync(Guid orderId, string designerId)
    {
        var order = await GetDesignerOrderAsync(orderId, designerId);
        if (order.Status != OrderStatus.AsignadoDiseno)
        {
            throw new InvalidOperationException("Solo puede aceptar pedidos recién asignados.");
        }

        if (DesignerOrderHelper.IsAccepted(order))
        {
            throw new InvalidOperationException("Este pedido ya fue aceptado.");
        }

        await AddHistoryAsync(order, order.Status, order.Status, designerId, DesignerOrderHelper.AcceptedComment);
        await db.SaveChangesAsync();
    }

    public async Task DesignerStartAsync(Guid orderId, string designerId)
    {
        var order = await GetDesignerOrderAsync(orderId, designerId);
        if (order.Status != OrderStatus.AsignadoDiseno || !DesignerOrderHelper.IsAccepted(order))
        {
            throw new InvalidOperationException("Debe aceptar el pedido antes de iniciarlo.");
        }

        var previous = order.Status;
        order.Status = OrderStatus.EnDiseno;
        await AddHistoryAsync(order, previous, OrderStatus.EnDiseno, designerId, DesignerOrderHelper.StartedComment);
        await db.SaveChangesAsync();
    }

    public async Task DesignerFinishAsync(Guid orderId, string designerId)
    {
        var order = await GetDesignerOrderAsync(orderId, designerId);
        if (order.Status != OrderStatus.EnDiseno)
        {
            throw new InvalidOperationException("Solo puede finalizar pedidos en diseño activo.");
        }

        var previous = order.Status;
        order.Status = OrderStatus.DisenoAprobado;
        await AddHistoryAsync(order, previous, OrderStatus.DisenoAprobado, designerId, DesignerOrderHelper.FinishedComment);
        await db.SaveChangesAsync();
    }

    public async Task DesignerReturnAsync(Guid orderId, string designerId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new InvalidOperationException("Indique el motivo de la devolución.");
        }

        var order = await GetDesignerOrderAsync(orderId, designerId);
        if (!DesignerOrderHelper.CanReturn(order))
        {
            throw new InvalidOperationException("Este pedido no puede devolverse.");
        }

        var previous = order.Status;
        order.Status = OrderStatus.EnRevision;
        order.AssignedDesignerId = null;
        await AddHistoryAsync(order, previous, OrderStatus.EnRevision, designerId, $"Devuelto a administración: {comment.Trim()}");
        await db.SaveChangesAsync();
    }

    public async Task DesignerUpdateDetailsAsync(Guid orderId, string designerId, string? sizeRange, string? notes)
    {
        var order = await GetDesignerOrderAsync(orderId, designerId);
        if (!DesignerOrderHelper.IsPending(order))
        {
            throw new InvalidOperationException("Solo puede editar pedidos en fase de diseño.");
        }

        order.SizeRange = string.IsNullOrWhiteSpace(sizeRange) ? order.SizeRange : sizeRange.Trim();
        if (notes is not null)
        {
            order.Notes = notes.Trim();
        }

        await db.SaveChangesAsync();
    }

    public async Task ProduccionAcceptAsync(Guid orderId, string userId)
    {
        var order = await GetProduccionOrderAsync(orderId);
        if (order.Status != OrderStatus.DisenoAprobado)
        {
            throw new InvalidOperationException("Solo puede aceptar pedidos enviados desde diseño.");
        }

        if (ProduccionOrderHelper.IsAccepted(order))
        {
            throw new InvalidOperationException("Este pedido ya fue aceptado.");
        }

        await AddHistoryAsync(order, order.Status, order.Status, userId, ProduccionOrderHelper.AcceptedComment);
        await db.SaveChangesAsync();
    }

    public async Task ProduccionStartAsync(Guid orderId, string userId)
    {
        var order = await GetProduccionOrderAsync(orderId);
        if (order.Status != OrderStatus.DisenoAprobado || !ProduccionOrderHelper.IsAccepted(order))
        {
            throw new InvalidOperationException("Debe aceptar el pedido antes de iniciarlo.");
        }

        var previous = order.Status;
        order.Status = OrderStatus.EnImpresion;
        await AddHistoryAsync(order, previous, OrderStatus.EnImpresion, userId, ProduccionOrderHelper.StartedComment);
        await db.SaveChangesAsync();
    }

    public async Task ProduccionSetStageAsync(Guid orderId, OrderStatus stage, string userId)
    {
        if (stage is not (OrderStatus.EnImpresion or OrderStatus.EnPlanchado or OrderStatus.EnConfeccion))
        {
            throw new InvalidOperationException("Etapa inválida.");
        }

        var order = await GetProduccionOrderAsync(orderId);
        var canChange = ProduccionOrderHelper.CanSetStage(order) ||
                        (order.Status == OrderStatus.DisenoAprobado && ProduccionOrderHelper.IsAccepted(order));
        if (!canChange)
        {
            throw new InvalidOperationException("El pedido aún no está en proceso de producción.");
        }

        if (stage == OrderStatus.EnConfeccion && (order.ServiceOnlyPrintPress || !order.IncludesConfection))
        {
            throw new InvalidOperationException("Este pedido no incluye confección.");
        }

        var previous = order.Status;
        order.Status = stage;
        await AddHistoryAsync(order, previous, stage, userId, $"Etapa: {OrderStatusHelper.GetLabel(stage)}");
        await db.SaveChangesAsync();
    }

    public async Task ProduccionMarkReadyForPickupAsync(Guid orderId, string userId)
    {
        var order = await GetProduccionOrderAsync(orderId);
        if (!ProduccionOrderHelper.CanMarkReadyForPickup(order))
        {
            throw new InvalidOperationException("Complete la etapa actual antes de marcar pendiente por recoger.");
        }

        var previous = order.Status;
        order.Status = OrderStatus.ListoEntrega;
        await AddHistoryAsync(order, previous, OrderStatus.ListoEntrega, userId, ProduccionOrderHelper.ReadyPickupComment);
        await db.SaveChangesAsync();
    }

    public async Task SaveOrderPricingAsync(
        Guid orderId,
        string userId,
        OrderPricingInput input,
        OrderPricingResult result,
        decimal chargeAmount,
        string? pricingNotes,
        bool includesConfection,
        bool serviceOnlyPrintPress)
    {
        var order = await GetProduccionOrderAsync(orderId);
        if (ProduccionOrderHelper.IsCompleted(order))
        {
            throw new InvalidOperationException("No puede modificar el cobro de un pedido ya finalizado.");
        }

        order.FabricTypeId = input.FabricTypeId;
        order.FabricTypeName = result.FabricName;
        order.FabricTypeRipId = input.FabricTypeRipId;
        order.FabricTypeRipName = result.FabricRipName;
        order.FabricMeters = input.FabricMeters;
        order.FabricMetersRip = input.FabricMetersRip;
        order.IncludesConfection = includesConfection;
        order.ServiceOnlyPrintPress = serviceOnlyPrintPress;
        order.CalculatedFabricCost = result.FabricCost + result.FabricRipCost;
        order.CalculatedFabricRipCost = result.FabricRipCost;
        order.CalculatedLaserCost = result.LaserCost;
        order.CalculatedPrintPressCost = result.PrintPressCost;
        order.CalculatedExtraCost = result.ExtraMeterCost;
        order.CalculatedConfectionCost = result.ConfectionCost;
        order.CalculatedTotal = result.SuggestedTotal;
        order.ChargeAmount = chargeAmount;
        order.PricingNotes = pricingNotes;
        order.PricingUpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    private async Task<Order> GetProduccionOrderAsync(Guid orderId)
    {
        var order = await db.Orders
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new InvalidOperationException("Pedido no encontrado.");

        var allowed = new[]
        {
            OrderStatus.DisenoAprobado,
            OrderStatus.EnImpresion,
            OrderStatus.EnPlanchado,
            OrderStatus.EnConfeccion,
            OrderStatus.ListoEntrega
        };

        if (!allowed.Contains(order.Status))
        {
            throw new InvalidOperationException("Este pedido no está en la cola de producción.");
        }

        return order;
    }

    private async Task<Order> GetDesignerOrderAsync(Guid orderId, string designerId)
    {
        var order = await db.Orders
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new InvalidOperationException("Pedido no encontrado.");

        if (order.AssignedDesignerId != designerId)
        {
            throw new InvalidOperationException("Este pedido no está asignado a usted.");
        }

        return order;
    }

    private async Task AddHistoryAsync(Order order, OrderStatus from, OrderStatus to, string userId, string? comment)
    {
        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = from,
            ToStatus = to,
            ChangedByUserId = userId,
            Comment = comment
        });
        await Task.CompletedTask;
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.Orders.CountAsync(o => o.CreatedAt.Year == year);
        return $"SSG-{year}-{(count + 1):D4}";
    }
}

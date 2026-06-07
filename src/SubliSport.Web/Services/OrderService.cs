using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;
using SubliSport.Infrastructure.Data;

namespace SubliSport.Web.Services;

public class OrderService(AppDbContext db)
{
    public async Task<List<Order>> GetOrdersForUserAsync(string userId, IEnumerable<string> roles)
    {
        var query = db.Orders
            .Include(o => o.AssignedDesigner)
            .Include(o => o.CreatedByUser)
            .AsQueryable();

        if (roles.Contains(AppRoles.SuperAdmin) || roles.Contains(AppRoles.Admin))
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

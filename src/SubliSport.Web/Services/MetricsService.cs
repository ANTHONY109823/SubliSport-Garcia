using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;
using SubliSport.Infrastructure.Data;
using SubliSport.Web.Helpers;

namespace SubliSport.Web.Services;

public record UserRoleMetric(string FullName, string Email, string Role, int ActiveOrders, double? AvgHoursInStage);

public record DesignerPersonalMetrics(
    int PendingAcceptance,
    int AcceptedNotStarted,
    int InProgress,
    int CompletedTotal,
    int CompletedThisMonth,
    int UrgentPending,
    double? AvgHoursToComplete,
    List<Order> PendingOrders,
    List<Order> CompletedOrders);

public record SystemMetrics(
    int TotalOrders,
    int TotalUsers,
    int ActiveUsers,
    Dictionary<string, int> OrdersByStatus,
    Dictionary<string, int> UsersByRole,
    int UrgentOrders,
    int PendingDesign,
    int InProduction,
    int Delivered,
    double? AvgDesignHours,
    double? AvgProductionHours,
    List<UserRoleMetric> DesignerMetrics,
    List<UserRoleMetric> ProductionMetrics);

public class MetricsService(AppDbContext db, UserManager<ApplicationUser> userManager)
{
    public async Task<SystemMetrics> GetSystemMetricsAsync()
    {
        return await BuildSystemMetricsAsync();
    }

    public async Task<SystemMetrics> GetAdminMetricsAsync() =>
        await BuildSystemMetricsAsync(includeUserRoleCounts: false);

    private async Task<SystemMetrics> BuildSystemMetricsAsync(bool includeUserRoleCounts = true)
    {
        var orders = await db.Orders
            .Include(o => o.AssignedDesigner)
            .Include(o => o.StatusHistory)
            .ToListAsync();

        var users = await userManager.Users.ToListAsync();
        var usersByRole = new Dictionary<string, int>();
        if (includeUserRoleCounts)
        {
            foreach (var role in AppRoles.All)
            {
                var count = (await userManager.GetUsersInRoleAsync(role)).Count;
                usersByRole[RoleHelper.GetLabel(role)] = count;
            }
        }
        else
        {
            foreach (var role in new[] { AppRoles.Designer, AppRoles.Production })
            {
                var count = (await userManager.GetUsersInRoleAsync(role)).Count(u => u.IsActive);
                usersByRole[RoleHelper.GetLabel(role)] = count;
            }
        }

        var ordersByStatus = orders
            .GroupBy(o => OrderStatusHelper.GetLabel(o.Status))
            .ToDictionary(g => g.Key, g => g.Count());

        var histories = await db.OrderStatusHistories
            .Include(h => h.ChangedByUser)
            .ToListAsync();

        var designHours = CalculateStageDurations(histories,
            OrderStatus.AsignadoDiseno,
            OrderStatus.DisenoAprobado);

        var productionHours = CalculateStageDurations(histories,
            OrderStatus.EnImpresion,
            OrderStatus.Entregado);

        var designers = await userManager.GetUsersInRoleAsync(AppRoles.Designer);
        var designerMetrics = designers.Select(d => new UserRoleMetric(
            d.FullName,
            d.Email ?? "",
            RoleHelper.GetLabel(AppRoles.Designer),
            orders.Count(o => o.AssignedDesignerId == d.Id &&
                              o.Status is OrderStatus.AsignadoDiseno or OrderStatus.EnDiseno),
            designHours.Where(x => x.UserId == d.Id).Select(x => (double?)x.Hours).DefaultIfEmpty(null).Average()
        )).ToList();

        var productionUsers = await userManager.GetUsersInRoleAsync(AppRoles.Production);
        var productionMetrics = productionUsers.Select(p => new UserRoleMetric(
            p.FullName,
            p.Email ?? "",
            RoleHelper.GetLabel(AppRoles.Production),
            orders.Count(o => o.Status is OrderStatus.EnImpresion or OrderStatus.EnPlanchado
                              or OrderStatus.EnConfeccion or OrderStatus.ListoEntrega),
            productionHours.Where(x => x.UserId == p.Id).Select(x => (double?)x.Hours).DefaultIfEmpty(null).Average()
        )).ToList();

        return new SystemMetrics(
            TotalOrders: orders.Count,
            TotalUsers: users.Count,
            ActiveUsers: users.Count(u => u.IsActive),
            OrdersByStatus: ordersByStatus,
            UsersByRole: usersByRole,
            UrgentOrders: orders.Count(o => o.Priority == OrderPriority.Urgente),
            PendingDesign: orders.Count(o => o.Status is OrderStatus.AsignadoDiseno or OrderStatus.EnDiseno),
            InProduction: orders.Count(o => o.Status is OrderStatus.EnImpresion or OrderStatus.EnPlanchado
                                             or OrderStatus.EnConfeccion or OrderStatus.ListoEntrega),
            Delivered: orders.Count(o => o.Status == OrderStatus.Entregado),
            AvgDesignHours: designHours.Count > 0 ? designHours.Average(x => x.Hours) : null,
            AvgProductionHours: productionHours.Count > 0 ? productionHours.Average(x => x.Hours) : null,
            DesignerMetrics: designerMetrics,
            ProductionMetrics: productionMetrics);
    }

    private static List<(string UserId, double Hours)> CalculateStageDurations(
        List<OrderStatusHistory> histories,
        OrderStatus stageStart,
        OrderStatus stageEnd)
    {
        var result = new List<(string, double)>();
        var byOrder = histories.GroupBy(h => h.OrderId);

        foreach (var group in byOrder)
        {
            var ordered = group.OrderBy(h => h.ChangedAt).ToList();
            var start = ordered.FirstOrDefault(h => h.ToStatus == stageStart);
            var end = ordered.FirstOrDefault(h => h.ToStatus == stageEnd);
            if (start is null || end is null || end.ChangedAt <= start.ChangedAt)
            {
                continue;
            }

            var hours = (end.ChangedAt - start.ChangedAt).TotalHours;
            result.Add((start.ChangedByUserId, hours));
        }

        return result;
    }

    public async Task<DesignerPersonalMetrics> GetDesignerMetricsAsync(string designerId)
    {
        var orders = await db.Orders
            .Include(o => o.StatusHistory)
            .Where(o => o.AssignedDesignerId == designerId ||
                        o.StatusHistory.Any(h => h.ChangedByUserId == designerId))
            .ToListAsync();

        var assigned = orders.Where(o => o.AssignedDesignerId == designerId).ToList();
        var pendingAcceptance = assigned.Count(o => DesignerOrderHelper.CanAccept(o));
        var acceptedNotStarted = assigned.Count(o => DesignerOrderHelper.CanStart(o));
        var inProgress = assigned.Count(o => o.Status == OrderStatus.EnDiseno);
        var completed = assigned.Where(DesignerOrderHelper.IsDesignPhaseComplete).ToList();
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var completedThisMonth = completed.Count(o =>
            o.StatusHistory.Any(h =>
                h.ToStatus == OrderStatus.DisenoAprobado &&
                h.ChangedAt >= monthStart));

        var urgentPending = assigned.Count(o =>
            DesignerOrderHelper.IsPending(o) && o.Priority == OrderPriority.Urgente);

        var completionHours = new List<double>();
        foreach (var order in completed)
        {
            var start = order.StatusHistory
                .Where(h => h.ToStatus == OrderStatus.EnDiseno)
                .OrderBy(h => h.ChangedAt)
                .FirstOrDefault();
            var end = order.StatusHistory
                .Where(h => h.ToStatus == OrderStatus.DisenoAprobado)
                .OrderBy(h => h.ChangedAt)
                .FirstOrDefault();
            if (start is not null && end is not null && end.ChangedAt > start.ChangedAt)
            {
                completionHours.Add((end.ChangedAt - start.ChangedAt).TotalHours);
            }
        }

        var pendingOrders = assigned
            .Where(DesignerOrderHelper.IsPending)
            .OrderByDescending(o => o.Priority)
            .ThenBy(o => o.AgreedDeliveryDate)
            .ToList();

        var completedOrders = completed
            .OrderByDescending(o => o.StatusHistory
                .Where(h => h.ToStatus == OrderStatus.DisenoAprobado)
                .Select(h => h.ChangedAt)
                .DefaultIfEmpty(o.CreatedAt)
                .Max())
            .Take(10)
            .ToList();

        return new DesignerPersonalMetrics(
            PendingAcceptance: pendingAcceptance,
            AcceptedNotStarted: acceptedNotStarted,
            InProgress: inProgress,
            CompletedTotal: completed.Count,
            CompletedThisMonth: completedThisMonth,
            UrgentPending: urgentPending,
            AvgHoursToComplete: completionHours.Count > 0 ? completionHours.Average() : null,
            PendingOrders: pendingOrders,
            CompletedOrders: completedOrders);
    }
}

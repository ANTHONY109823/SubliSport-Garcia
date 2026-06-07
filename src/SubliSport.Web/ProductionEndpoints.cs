using Microsoft.AspNetCore.Identity;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Web.Services;

namespace SubliSport.Web;

public static class ProductionEndpoints
{
    public static void MapProductionEndpoints(this WebApplication app)
    {
        app.MapPost("/produccion/pedidos/{orderId:guid}/aceptar", AcceptOrderAsync)
            .RequireAuthorization()
            .DisableAntiforgery();
    }

    private static async Task<IResult> AcceptOrderAsync(
        Guid orderId,
        HttpContext context,
        OrderService orderService,
        UserManager<ApplicationUser> userManager,
        ILogger<Program> logger)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            return Results.Redirect("/login");
        }

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Production))
        {
            return Results.Redirect($"/produccion/{orderId}?error={Uri.EscapeDataString("Solo producción puede aceptar pedidos.")}");
        }

        try
        {
            await orderService.ProduccionAcceptAsync(orderId, user.Id);
            return Results.Redirect($"/produccion/{orderId}?msg=accepted");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al aceptar pedido {OrderId}", orderId);
            return Results.Redirect($"/produccion/{orderId}?error={Uri.EscapeDataString(ex.Message)}");
        }
    }
}

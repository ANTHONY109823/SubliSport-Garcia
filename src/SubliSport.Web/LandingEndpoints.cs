using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Landing;
using SubliSport.Domain.Orders;
using SubliSport.Web.Services;

namespace SubliSport.Web;

public static class LandingEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void MapLandingEndpoints(this WebApplication app)
    {
        app.MapGet("/api/landing-config", async (LandingConfigurationService landingConfig) =>
            {
                var settings = await landingConfig.GetSettingsAsync();
                return Results.Json(settings);
            })
            .AllowAnonymous()
            .RequireCors("LandingPublic");

        app.MapPost("/api/landing-quote", async (
            LandingQuoteSubmitRequest request,
            OrderService orderService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            HttpContext http) =>
            {
                if (string.IsNullOrWhiteSpace(request.ClientName))
                {
                    return Results.BadRequest(new { error = "Indique su nombre o club." });
                }

                if (string.IsNullOrWhiteSpace(request.GarmentType) || string.IsNullOrWhiteSpace(request.Sport))
                {
                    return Results.BadRequest(new { error = "Seleccione prenda y deporte." });
                }

                var owner = (await userManager.GetUsersInRoleAsync(AppRoles.Admin))
                    .FirstOrDefault(u => u.IsActive)
                    ?? (await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin))
                        .FirstOrDefault(u => u.IsActive);

                if (owner is null)
                {
                    return Results.Problem("No hay administrador activo para registrar la solicitud.");
                }

                var referenceImageUrl = await SaveReferenceImageAsync(env, request.ReferenceImageBase64);
                var roster = request.Roster
                    .Where(r => !string.IsNullOrWhiteSpace(r.Name) ||
                                !string.IsNullOrWhiteSpace(r.Size) ||
                                !string.IsNullOrWhiteSpace(r.Number))
                    .Select(r => new ConfectionRosterLine
                    {
                        Name = r.Name.Trim(),
                        Size = r.Size.Trim(),
                        Number = r.Number.Trim()
                    })
                    .ToList();

                var quantity = roster.Count > 0 ? roster.Count : Math.Max(1, request.Quantity);
                var notes = (request.Notes ?? string.Empty).Trim();

                if (referenceImageUrl is not null)
                {
                    var absolute = $"{http.Request.Scheme}://{http.Request.Host}{referenceImageUrl}";
                    notes = string.IsNullOrEmpty(notes)
                        ? $"Foto modelo referencia: {absolute}"
                        : $"{notes}\n\nFoto modelo referencia: {absolute}";
                }

                var sizeRange = roster.Count > 0
                    ? string.Join(" · ", roster.Select(l =>
                        $"{l.Name} T{l.Size} N°{l.Number}".Trim()))
                    : request.SizeRangeSummary;

                var order = new Order
                {
                    ClientName = request.ClientName.Trim(),
                    ClientPhone = string.IsNullOrWhiteSpace(request.ClientPhone) ? null : request.ClientPhone.Trim(),
                    GarmentType = request.GarmentType.Trim(),
                    Sport = request.Sport.Trim(),
                    Quantity = quantity,
                    SizeRange = sizeRange,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    ConfectionRosterDetails = roster.Count > 0
                        ? JsonSerializer.Serialize(roster, JsonOptions)
                        : null,
                    ReceivedAt = DateTime.UtcNow
                };

                var created = await orderService.CreateManualOrderAsync(
                    order,
                    owner.Id,
                    "Cotización recibida desde la página web");

                return Results.Json(new LandingQuoteSubmitResponse
                {
                    OrderId = created.Id,
                    OrderNumber = created.OrderNumber,
                    ReferenceImageUrl = referenceImageUrl is null
                        ? null
                        : $"{http.Request.Scheme}://{http.Request.Host}{referenceImageUrl}"
                });
            })
            .AllowAnonymous()
            .RequireCors("LandingPublic");
    }

    private static async Task<string?> SaveReferenceImageAsync(IWebHostEnvironment env, string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return null;
        }

        var payload = base64;
        if (payload.Contains(','))
        {
            payload = payload.Split(',', 2)[1];
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(payload);
        }
        catch
        {
            return null;
        }

        if (bytes.Length == 0 || bytes.Length > 4 * 1024 * 1024)
        {
            return null;
        }

        var dir = Path.Combine(env.WebRootPath, "uploads", "quotes");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid():N}.jpg";
        await File.WriteAllBytesAsync(Path.Combine(dir, fileName), bytes);
        return $"/uploads/quotes/{fileName}";
    }
}

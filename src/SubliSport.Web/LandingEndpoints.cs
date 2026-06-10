using System.Globalization;
using Microsoft.AspNetCore.Identity;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Landing;
using SubliSport.Web.Helpers;
using SubliSport.Web.Services;

namespace SubliSport.Web;

public static class LandingEndpoints
{
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
            QuoteReferenceImageService quoteImages,
            UserManager<ApplicationUser> userManager) =>
            {
                if (string.IsNullOrWhiteSpace(request.ClientName))
                {
                    return Results.BadRequest(new { error = "Indique su nombre o club." });
                }

                if (string.IsNullOrWhiteSpace(request.Sport))
                {
                    return Results.BadRequest(new { error = "Seleccione el deporte." });
                }

                var isMixed = request.GarmentType.Equals("Mixta", StringComparison.OrdinalIgnoreCase) ||
                              request.MixedLines.Count > 0;
                if (string.IsNullOrWhiteSpace(request.GarmentType) && !isMixed)
                {
                    return Results.BadRequest(new { error = "Seleccione el tipo de prenda." });
                }

                if (isMixed && !request.MixedLines.Any(l => l.Quantity > 0))
                {
                    return Results.BadRequest(new { error = "Indique las cantidades del pedido mixto." });
                }

                var owner = (await userManager.GetUsersInRoleAsync(AppRoles.Admin))
                    .FirstOrDefault(u => u.IsActive)
                    ?? (await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin))
                        .FirstOrDefault(u => u.IsActive);

                if (owner is null)
                {
                    return Results.Problem("No hay administrador activo para registrar la solicitud.");
                }

                var agreedDeliveryDate = ParseSpanishDate(request.DesiredDeliveryDeadline);
                if (!string.IsNullOrWhiteSpace(request.DesiredDeliveryDeadline) && agreedDeliveryDate is null)
                {
                    var plazo = $"Fecha de entrega solicitada: {request.DesiredDeliveryDeadline.Trim()}";
                    request.Notes = string.IsNullOrWhiteSpace(request.Notes)
                        ? plazo
                        : $"{request.Notes.Trim()}\n\n{plazo}";
                }

                var images = (request.ReferenceImagesBase64 ?? [])
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                if (images.Count == 0 && !string.IsNullOrWhiteSpace(request.ReferenceImageBase64))
                {
                    images.Add(request.ReferenceImageBase64);
                }

                var quote = LandingQuoteCalculator.Calculate(request);
                var referenceImageUrls = await quoteImages.SaveAsync(images);
                var referenceImageUrl = referenceImageUrls.FirstOrDefault();

                var order = LandingQuoteOrderAssembler.Build(
                    request,
                    quote,
                    referenceImageUrls,
                    DateTime.UtcNow,
                    agreedDeliveryDate);

                var created = await orderService.CreateManualOrderAsync(
                    order,
                    owner.Id,
                    "Cotización recibida desde la página web");

                return Results.Json(new LandingQuoteSubmitResponse
                {
                    OrderId = created.Id,
                    OrderNumber = created.OrderNumber,
                    ReferenceImageUrl = referenceImageUrl,
                    ReferenceImageUrls = referenceImageUrls,
                    ClientRequestText = quote.ClientRequestText
                });
            })
            .AllowAnonymous()
            .RequireCors("LandingPublic");
    }

    private static DateTime? ParseSpanishDate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (DateTime.TryParseExact(
                text.Trim(),
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            return DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
        }

        return null;
    }
}

using System.Text.Json;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Enums;
using SubliSport.Domain.Landing;
using SubliSport.Domain.Orders;

namespace SubliSport.Web.Helpers;

public static class LandingQuoteOrderAssembler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static Order Build(
        LandingQuoteSubmitRequest request,
        LandingQuoteResult quote,
        IReadOnlyList<string> referenceImageUrls,
        DateTime receivedAt,
        DateTime? agreedDeliveryDate,
        OrderPriority priority = OrderPriority.Normal,
        GiftOption giftOption = GiftOption.None)
    {
        var isMixed = request.GarmentType.Equals("Mixta", StringComparison.OrdinalIgnoreCase) ||
                      request.MixedLines.Count > 0;

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

        var quantity = roster.Count > 0
            ? roster.Count
            : isMixed
                ? request.MixedLines.Where(l => l.Quantity > 0).Sum(l => l.Quantity)
                : Math.Max(1, request.Quantity);

        var notes = (request.Notes ?? string.Empty).Trim();

        if (referenceImageUrls.Count > 0)
        {
            var photoLines = referenceImageUrls.Select((url, index) =>
                referenceImageUrls.Count > 1
                    ? $"Foto modelo referencia {index + 1}: {url}"
                    : $"Foto modelo referencia: {url}");
            var photoBlock = string.Join("\n", photoLines);
            notes = string.IsNullOrEmpty(notes) ? photoBlock : $"{notes}\n\n{photoBlock}";
        }

        string? mixedJson = null;
        if (isMixed)
        {
            var mixedLines = request.MixedLines
                .Where(l => l.Quantity > 0 && !string.IsNullOrWhiteSpace(l.ItemType))
                .Select(l => new MixedGarmentLine
                {
                    ItemType = l.ItemType.Trim(),
                    Quantity = l.Quantity,
                    OtherDescription = string.IsNullOrWhiteSpace(l.OtherDescription) ? null : l.OtherDescription.Trim()
                })
                .ToList();
            mixedJson = MixedGarmentHelper.Serialize(mixedLines);
        }

        var sizeRange = roster.Count > 0
            ? string.Join(" · ", roster.Select(l => $"{l.Name} T{l.Size} N°{l.Number}".Trim()))
            : request.SizeRangeSummary;

        var pricingTier = request.PricingTier == (int)ClientPricingTier.MypeB2B
            ? ClientPricingTier.MypeB2B
            : ClientPricingTier.DirectRetail;

        var calculatedTotal = quote.Total;
        var chargeAmount = quote.Total;
        string? pricingNotes = LandingQuoteNotesHelper.Pack(quote.AdminSuggestionText, quote.ClientProformaDraft);
        if (pricingTier == ClientPricingTier.MypeB2B)
        {
            calculatedTotal = 0;
            chargeAmount = 0;
            pricingNotes = null;
        }

        return new Order
        {
            PricingTier = pricingTier,
            ClientName = request.ClientName.Trim(),
            ClientPhone = string.IsNullOrWhiteSpace(request.ClientPhone) ? null : request.ClientPhone.Trim(),
            GarmentType = isMixed ? "Mixta" : request.GarmentType.Trim(),
            MixedGarmentDetails = mixedJson,
            GiftOption = giftOption,
            Sport = request.Sport.Trim(),
            Quantity = quantity,
            SizeRange = string.IsNullOrWhiteSpace(sizeRange) ? null : sizeRange.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
            FabricTypeName = quote.FabricLabel,
            CalculatedTotal = calculatedTotal,
            ChargeAmount = chargeAmount,
            PricingNotes = pricingNotes,
            PricingUpdatedAt = DateTime.UtcNow,
            ConfectionRosterDetails = roster.Count > 0
                ? JsonSerializer.Serialize(roster, JsonOptions)
                : null,
            ReceivedAt = receivedAt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(receivedAt, DateTimeKind.Utc)
                : receivedAt.ToUniversalTime(),
            AgreedDeliveryDate = agreedDeliveryDate,
            Priority = priority
        };
    }
}

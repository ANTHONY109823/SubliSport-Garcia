using SubliSport.Domain.Entities;
using SubliSport.Domain.Landing;

namespace SubliSport.Web.Helpers;

public sealed record ProformaLineItem(
    string Description,
    int Units,
    decimal? UnitPrice,
    decimal? LineTotal);

public sealed class ProformaDocument
{
    public required Order Order { get; init; }
    public required decimal ChargeAmount { get; init; }
    public required IReadOnlyList<ProformaLineItem> Lines { get; init; }
    public string? Comments { get; init; }
    public DateTime IssueDate { get; init; }
    public string StatusLabel => ChargeAmount > 0 ? "COTIZACIÓN" : "BORRADOR";
    public decimal Subtotal => ChargeAmount;
    public decimal Total => ChargeAmount;
}

public static class ProformaDocumentBuilder
{
    public static ProformaDocument Build(Order order, decimal? chargeOverride = null, string? comments = null)
    {
        var charge = chargeOverride ?? order.ChargeAmount;
        if (charge <= 0 && order.CalculatedTotal > 0)
        {
            charge = order.CalculatedTotal;
        }

        return new ProformaDocument
        {
            Order = order,
            ChargeAmount = charge,
            Lines = BuildLines(order, charge),
            Comments = string.IsNullOrWhiteSpace(comments) ? order.Notes : comments.Trim(),
            IssueDate = order.PricingUpdatedAt?.ToLocalTime() ?? DateTime.Now
        };
    }

    private static List<ProformaLineItem> BuildLines(Order order, decimal total)
    {
        if (GarmentTypeHelper.IsMixed(order.GarmentType))
        {
            return BuildMixedLines(order, total);
        }

        var qty = Math.Max(1, order.Quantity);
        return
        [
            new ProformaLineItem(
                BuildServiceDescription(order, GarmentTypeHelper.GetLabel(order.GarmentType)),
                qty,
                total > 0 ? Math.Round(total / qty, 2) : null,
                total > 0 ? total : null)
        ];
    }

    private static List<ProformaLineItem> BuildMixedLines(Order order, decimal total)
    {
        var mixed = MixedGarmentHelper.Parse(order.MixedGarmentDetails)
            .Where(l => l.Quantity > 0)
            .ToList();

        if (mixed.Count == 0)
        {
            var qty = Math.Max(1, order.Quantity);
            return
            [
                new ProformaLineItem(
                    BuildServiceDescription(order, "Pedido mixto"),
                    qty,
                    total > 0 ? Math.Round(total / qty, 2) : null,
                    total > 0 ? total : null)
            ];
        }

        var totalQty = mixed.Sum(l => l.Quantity);
        var lines = new List<ProformaLineItem>();
        var allocated = 0m;

        for (var i = 0; i < mixed.Count; i++)
        {
            var line = mixed[i];
            var label = line.ItemType == "Otro" && !string.IsNullOrWhiteSpace(line.OtherDescription)
                ? line.OtherDescription.Trim()
                : line.ItemType;

            decimal? lineTotal = null;
            decimal? unitPrice = null;
            if (total > 0)
            {
                lineTotal = i == mixed.Count - 1
                    ? total - allocated
                    : Math.Round(total * line.Quantity / totalQty, 2);
                allocated += lineTotal.Value;
                unitPrice = Math.Round(lineTotal.Value / line.Quantity, 2);
            }

            lines.Add(new ProformaLineItem(
                BuildServiceDescription(order, label),
                line.Quantity,
                unitPrice,
                lineTotal));
        }

        return lines;
    }

    private static string BuildServiceDescription(Order order, string garmentLabel)
    {
        var parts = new List<string> { garmentLabel };

        if (!string.IsNullOrWhiteSpace(order.Sport))
        {
            parts.Add($"Deporte: {order.Sport}");
        }

        if (!string.IsNullOrWhiteSpace(order.FabricTypeName))
        {
            parts.Add($"Tela: {order.FabricTypeName}");
        }

        if (!string.IsNullOrWhiteSpace(order.SizeRange))
        {
            parts.Add($"Tallas: {order.SizeRange}");
        }

        if (order.GiftOption != Domain.Enums.GiftOption.None)
        {
            parts.Add($"Incluye obsequio: {GiftOptionHelper.GetLabel(order.GiftOption)}");
        }

        return string.Join(" · ", parts);
    }
}

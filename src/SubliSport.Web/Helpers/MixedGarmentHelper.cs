using System.Text.Json;
using SubliSport.Domain.Orders;

namespace SubliSport.Web.Helpers;

public static class MixedGarmentHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static List<MixedGarmentLine> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<MixedGarmentLine>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static string Serialize(IEnumerable<MixedGarmentLine> lines) =>
        JsonSerializer.Serialize(lines, JsonOptions);

    public static string FormatSummary(string? json)
    {
        var lines = Parse(json);
        if (lines.Count == 0)
        {
            return "Mixta (sin detalle)";
        }

        return string.Join(" · ", lines.Select(FormatLine));
    }

    public static string FormatLine(MixedGarmentLine line)
    {
        var label = line.ItemType == "Otro" && !string.IsNullOrWhiteSpace(line.OtherDescription)
            ? line.OtherDescription.Trim()
            : line.ItemType;

        return $"{line.Quantity} {label}";
    }

    public static int TotalQuantity(string? json) =>
        Parse(json).Sum(l => l.Quantity);
}

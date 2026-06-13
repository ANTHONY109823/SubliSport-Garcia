using System.Text.Json;
using SubliSport.Domain.Orders;

namespace SubliSport.Web.Helpers;

public static class ConfectionRosterHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static List<ConfectionRosterLine> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<ConfectionRosterLine>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static string Serialize(IEnumerable<ConfectionRosterLine> lines) =>
        JsonSerializer.Serialize(lines, JsonOptions);

    public static List<ConfectionRosterLine> Normalize(IEnumerable<ConfectionRosterLine> lines) =>
        lines
            .Where(l => !string.IsNullOrWhiteSpace(l.Name) ||
                        !string.IsNullOrWhiteSpace(l.Size) ||
                        !string.IsNullOrWhiteSpace(l.Number))
            .Select(l => new ConfectionRosterLine
            {
                Name = l.Name.Trim(),
                Size = l.Size.Trim(),
                Number = l.Number.Trim(),
                Gender = RosterGenderHelper.Normalize(l.Gender)
            })
            .ToList();
}

using SubliSport.Domain.Landing;

namespace SubliSport.Web.Helpers;

public static class RosterCsvImportHelper
{
    public static List<LandingQuoteRosterLine> Parse(string csvText)
    {
        var lines = csvText
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length == 0)
        {
            return [];
        }

        var rows = lines.Select(ParseLine).ToList();
        var header = rows[0].Select(NormalizeHeader).ToArray();
        var hasHeader = header.Any(h =>
            h.Contains("nombre", StringComparison.Ordinal) ||
            h.Contains("talla", StringComparison.Ordinal) ||
            h is "n" or "numero" or "nro");

        var dataRows = hasHeader ? rows.Skip(1) : rows;
        var nameIdx = 0;
        var sizeIdx = 1;
        var numIdx = 2;
        var genderIdx = 3;
        var kitIdx = 4;

        if (hasHeader)
        {
            nameIdx = Array.FindIndex(header, h => h.Contains("nombre", StringComparison.Ordinal));
            sizeIdx = Array.FindIndex(header, h => h.Contains("talla", StringComparison.Ordinal));
            numIdx = Array.FindIndex(header, h => h.Contains("numero", StringComparison.Ordinal) || h is "n" or "nro");
            genderIdx = Array.FindIndex(header, h => h.Contains("genero", StringComparison.Ordinal) || h.Contains("sexo", StringComparison.Ordinal));
            kitIdx = Array.FindIndex(header, h =>
                h.Contains("prenda", StringComparison.Ordinal) ||
                h.Contains("kit", StringComparison.Ordinal) ||
                h.Contains("conjunto", StringComparison.Ordinal));
            if (nameIdx < 0) nameIdx = 0;
            if (sizeIdx < 0) sizeIdx = 1;
            if (numIdx < 0) numIdx = 2;
        }

        return dataRows
            .Select(row =>
            {
                var line = new LandingQuoteRosterLine
                {
                    Name = GetCell(row, nameIdx),
                    Size = GetCell(row, sizeIdx),
                    Number = GetCell(row, numIdx),
                    Gender = genderIdx >= 0
                        ? RosterGenderHelper.Normalize(GetCell(row, genderIdx))
                        : RosterGenderHelper.Varon,
                    KitType = kitIdx >= 0
                        ? RosterKitHelper.Normalize(GetCell(row, kitIdx))
                        : RosterKitHelper.Conjunto
                };
                return line;
            })
            .Where(r => !string.IsNullOrWhiteSpace(r.Name) ||
                        !string.IsNullOrWhiteSpace(r.Size) ||
                        !string.IsNullOrWhiteSpace(r.Number))
            .ToList();
    }

    private static string GetCell(string[] row, int index) =>
        index >= 0 && index < row.Length ? row[index].Trim() : string.Empty;

    private static string[] ParseLine(string line)
    {
        if (!line.Contains('"'))
        {
            return line.Split(',', StringSplitOptions.TrimEntries);
        }

        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        result.Add(current.ToString().Trim());
        return result.ToArray();
    }

    private static string NormalizeHeader(string value) =>
        value.ToLowerInvariant()
            .Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Aggregate(new System.Text.StringBuilder(), (sb, c) => sb.Append(c), sb => sb.ToString())
            .Trim();
}

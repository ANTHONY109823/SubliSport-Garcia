using System.Text.RegularExpressions;

namespace SubliSport.Web.Helpers;

public static partial class ReferenceImageNotesHelper
{
    [GeneratedRegex(@"^Foto modelo referencia(?:\s+\d+)?:\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ReferencePhotoLineRegex();

    public static (string TextNotes, List<string> ImageUrls) Split(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return (string.Empty, []);
        }

        var urls = new List<string>();
        var textLines = new List<string>();

        foreach (var line in notes.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            var match = ReferencePhotoLineRegex().Match(trimmed);
            if (match.Success)
            {
                var url = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    urls.Add(url);
                }
            }
            else
            {
                textLines.Add(trimmed);
            }
        }

        return (string.Join("\n", textLines), urls);
    }

    public static string Merge(string? textNotes, IReadOnlyList<string> imageUrls)
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(textNotes))
        {
            lines.AddRange(textNotes.Trim().Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)));
        }

        for (var i = 0; i < imageUrls.Count; i++)
        {
            var label = imageUrls.Count > 1
                ? $"Foto modelo referencia {i + 1}"
                : "Foto modelo referencia";
            lines.Add($"{label}: {imageUrls[i].Trim()}");
        }

        return lines.Count == 0 ? string.Empty : string.Join("\n", lines);
    }
}

namespace SubliSport.Web.Services;

public class QuoteReferenceImageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
{
    public async Task<List<string>> SaveAsync(IEnumerable<string?> base64Images, int max = 3)
    {
        var saved = new List<string>();
        foreach (var base64 in base64Images.Where(s => !string.IsNullOrWhiteSpace(s)).Take(max))
        {
            var relative = await SaveOneAsync(base64!);
            if (relative is null)
            {
                continue;
            }

            var http = httpContextAccessor.HttpContext;
            if (http is null)
            {
                saved.Add(relative);
                continue;
            }

            saved.Add($"{http.Request.Scheme}://{http.Request.Host}{relative}");
        }

        return saved;
    }

    private async Task<string?> SaveOneAsync(string base64)
    {
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

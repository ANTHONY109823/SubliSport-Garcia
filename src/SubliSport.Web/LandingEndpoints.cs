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
    }
}

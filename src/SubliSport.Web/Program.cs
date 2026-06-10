using System.Net;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Infrastructure;
using SubliSport.Infrastructure.Data;
using SubliSport.Web.Components;
using SubliSport.Web.Services;
using SubliSport.Web;

var port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "8080");

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, port);
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", p => p.RequireRole(AppRoles.SuperAdmin));
    options.AddPolicy("Administration", p => p.RequireRole(AppRoles.SuperAdmin, AppRoles.Admin));
    options.AddPolicy("DesignTeam", p => p.RequireRole(AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.Designer));
    options.AddPolicy("ProductionTeam", p => p.RequireRole(AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.Production));
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(2);
});

builder.Services.AddScoped<PricingConfigurationService>();
builder.Services.AddScoped<ProductionConfigurationService>();
builder.Services.AddScoped<LandingConfigurationService>();
builder.Services.AddScoped<PanelUiConfigurationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LandingPublic", policy =>
        policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin))
                {
                    return true;
                }

                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Host.EndsWith("github.io", StringComparison.OrdinalIgnoreCase)
                       || uri.Host.EndsWith("github.dev", StringComparison.OrdinalIgnoreCase)
                       || uri.Host.Contains("railway.app", StringComparison.OrdinalIgnoreCase)
                       || uri.Host is "localhost" or "127.0.0.1";
            })
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<QuoteReferenceImageService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<MetricsService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.Logger.LogInformation("SubliSport escuchando en 0.0.0.0:{Port} (PORT env={EnvPort})",
    port, Environment.GetEnvironmentVariable("PORT") ?? "no definido");

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
else
{
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("LandingPublic");

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapAccountEndpoints();
app.MapProductionEndpoints();
app.MapLandingEndpoints();

app.UseDefaultFiles();
app.MapStaticAssets();
app.MapGet("/", () => Results.Redirect("/index.html")).AllowAnonymous();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.InitializeDatabaseAsync();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<PricingConfigurationService>().EnsureSeedAsync();
    await scope.ServiceProvider.GetRequiredService<ProductionConfigurationService>().EnsureSeedAsync();
    await scope.ServiceProvider.GetRequiredService<LandingConfigurationService>().EnsureSeedAsync();
    await scope.ServiceProvider.GetRequiredService<PanelUiConfigurationService>().EnsureSeedAsync();
}

app.Run();

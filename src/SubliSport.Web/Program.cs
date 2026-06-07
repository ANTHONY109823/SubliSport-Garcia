using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Infrastructure;
using SubliSport.Infrastructure.Data;
using SubliSport.Web.Components;
using SubliSport.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Railway inyecta PORT; local/Docker usa 8080 por defecto
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

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

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.MapStaticAssets();
app.MapGet("/", () => Results.Redirect("/index.html")).AllowAnonymous();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.InitializeDatabaseAsync();

app.Run();

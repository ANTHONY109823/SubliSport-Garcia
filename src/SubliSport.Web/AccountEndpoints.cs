using Microsoft.AspNetCore.Identity;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;
using SubliSport.Web.Services;

namespace SubliSport.Web;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/account/login", async Task<IResult> (
            HttpContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<Program> logger) =>
        {
            var form = await context.Request.ReadFormAsync();
            var email = form["Email"].ToString().Trim();
            var password = form["Password"].ToString();
            var rememberMe = form.ContainsKey("RememberMe");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Results.Redirect("/login?error=empty");
            }

            try
            {
                context.Response.Cookies.Delete(ImpersonationConstants.CookieName);

                var user = await userManager.FindByEmailAsync(email);
                if (user is null)
                {
                    logger.LogWarning("Login fallido: usuario no encontrado ({Email})", email);
                    return Results.Redirect($"/login?error=invalid&email={Uri.EscapeDataString(email)}");
                }

                if (!user.IsActive)
                {
                    logger.LogWarning("Login fallido: usuario inactivo ({Email})", email);
                    return Results.Redirect($"/login?error=inactive&email={Uri.EscapeDataString(email)}");
                }

                var result = await signInManager.PasswordSignInAsync(
                    user.UserName!,
                    password,
                    rememberMe,
                    lockoutOnFailure: true);

                if (result.IsLockedOut)
                {
                    return Results.Redirect($"/login?error=locked&email={Uri.EscapeDataString(email)}");
                }

                if (!result.Succeeded)
                {
                    logger.LogWarning("Login fallido: clave incorrecta ({Email})", email);
                    return Results.Redirect($"/login?error=invalid&email={Uri.EscapeDataString(email)}");
                }

                user.LastLoginAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);

                return Results.Redirect("/dashboard");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado en login ({Email})", email);
                return Results.Redirect($"/login?error=failed&email={Uri.EscapeDataString(email)}");
            }
        }).DisableAntiforgery();

        app.MapPost("/account/impersonate", async Task<IResult> (
            HttpContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) =>
        {
            var current = await userManager.GetUserAsync(context.User);
            if (current is null || !await userManager.IsInRoleAsync(current, AppRoles.SuperAdmin))
            {
                return Results.Redirect("/dashboard");
            }

            var form = await context.Request.ReadFormAsync();
            var targetId = form["UserId"].ToString();
            if (string.IsNullOrWhiteSpace(targetId) || targetId == current.Id)
            {
                return Results.Redirect("/usuarios");
            }

            var target = await userManager.FindByIdAsync(targetId);
            if (target is null || !target.IsActive)
            {
                return Results.Redirect("/usuarios?error=inactive");
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromHours(8)
            };
            if (context.Request.IsHttps)
            {
                cookieOptions.Secure = true;
            }

            context.Response.Cookies.Append(ImpersonationConstants.CookieName, current.Id, cookieOptions);

            await signInManager.SignInAsync(target, isPersistent: false);

            var targetRoles = await userManager.GetRolesAsync(target);
            var redirect = targetRoles.Contains(AppRoles.Production) ? "/produccion"
                : targetRoles.Contains(AppRoles.Designer) ? "/pedidos"
                : "/dashboard";

            return Results.Redirect(redirect);
        }).DisableAntiforgery();

        app.MapPost("/account/stop-impersonation", async Task<IResult> (
            HttpContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) =>
        {
            var returnId = context.Request.Cookies[ImpersonationConstants.CookieName];
            if (string.IsNullOrWhiteSpace(returnId))
            {
                return Results.Redirect("/dashboard");
            }

            var superAdmin = await userManager.FindByIdAsync(returnId);
            context.Response.Cookies.Delete(ImpersonationConstants.CookieName);

            if (superAdmin is not null && superAdmin.IsActive)
            {
                await signInManager.SignInAsync(superAdmin, isPersistent: false);
            }

            return Results.Redirect("/dashboard");
        }).DisableAntiforgery();

        app.MapPost("/account/logout", async Task<IResult> (
            HttpContext context,
            SignInManager<ApplicationUser> signInManager) =>
        {
            context.Response.Cookies.Delete(ImpersonationConstants.CookieName);
            await signInManager.SignOutAsync();
            return Results.Redirect("/login");
        }).DisableAntiforgery();
    }
}

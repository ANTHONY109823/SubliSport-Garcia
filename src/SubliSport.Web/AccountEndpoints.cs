using Microsoft.AspNetCore.Identity;
using SubliSport.Domain.Entities;

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
                var user = await userManager.FindByEmailAsync(email);
                if (user is null || !user.IsActive)
                {
                    logger.LogWarning("Login fallido: usuario no encontrado o inactivo ({Email})", email);
                    return Results.Redirect($"/login?error=invalid&email={Uri.EscapeDataString(email)}");
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

        app.MapPost("/account/logout", async Task<IResult> (SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/login");
        }).DisableAntiforgery();
    }
}

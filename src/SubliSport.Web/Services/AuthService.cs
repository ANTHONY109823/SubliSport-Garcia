using Microsoft.AspNetCore.Identity;
using SubliSport.Domain.Entities;

namespace SubliSport.Web.Services;

public class AuthService(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager)
{
    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password, bool rememberMe)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null || !user.IsActive)
        {
            return (false, "Credenciales inválidas.");
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            password,
            rememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return (false, "Cuenta bloqueada temporalmente. Intente más tarde.");
        }

        if (!result.Succeeded)
        {
            return (false, "Credenciales inválidas.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return (true, null);
    }

    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync();
    }
}

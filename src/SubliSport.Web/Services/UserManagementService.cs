using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;

namespace SubliSport.Web.Services;

public record CreateUserRequest(
    string Email,
    string FullName,
    string Password,
    string Role);

public class UserManagementService(UserManager<ApplicationUser> userManager)
{
    public async Task<List<ApplicationUser>> GetAllUsersAsync() =>
        await userManager.Users.OrderBy(u => u.FullName).ToListAsync();

    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user) =>
        await userManager.GetRolesAsync(user);

    public async Task<List<ApplicationUser>> GetDesignersAsync()
    {
        var designers = await userManager.GetUsersInRoleAsync(AppRoles.Designer);
        return designers.Where(u => u.IsActive).OrderBy(u => u.FullName).ToList();
    }

    public async Task<(bool Success, string? Error)> CreateUserAsync(CreateUserRequest request, string requestingUserId, bool isSuperAdmin)
    {
        if (!AppRoles.All.Contains(request.Role))
        {
            return (false, "Rol inválido.");
        }

        if (request.Role == AppRoles.SuperAdmin && !isSuperAdmin)
        {
            return (false, "Solo SuperAdmin puede crear otros SuperAdmin.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            EmailConfirmed = true,
            FullName = request.FullName.Trim(),
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, request.Role);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ToggleActiveAsync(string userId, bool isSuperAdmin)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "Usuario no encontrado.");
        }

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains(AppRoles.SuperAdmin) && !isSuperAdmin)
        {
            return (false, "No puede desactivar un SuperAdmin.");
        }

        user.IsActive = !user.IsActive;
        await userManager.UpdateAsync(user);
        return (true, null);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;

namespace SubliSport.Web.Services;

public record CreateUserRequest(
    string Email,
    string FullName,
    string Password,
    string Role,
    string? PhoneNumber = null);

public record UpdateUserRequest(
    string UserId,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    string? NewPassword,
    string? PhoneNumber = null);

public class UserManagementService(UserManager<ApplicationUser> userManager)
{
    public async Task<List<ApplicationUser>> GetAllUsersAsync() =>
        await userManager.Users.OrderBy(u => u.FullName).ToListAsync();

    public async Task<ApplicationUser?> GetByIdAsync(string userId) =>
        await userManager.FindByIdAsync(userId);

    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user) =>
        await userManager.GetRolesAsync(user);

    public async Task<List<ApplicationUser>> GetDesignersAsync()
    {
        var designers = await userManager.GetUsersInRoleAsync(AppRoles.Designer);
        return designers.Where(u => u.IsActive).OrderBy(u => u.FullName).ToList();
    }

    public async Task<(bool Success, string? Error)> CreateUserAsync(CreateUserRequest request, bool isSuperAdmin)
    {
        if (!AppRoles.All.Contains(request.Role))
        {
            return (false, "Rol inválido.");
        }

        if (request.Role == AppRoles.SuperAdmin && !isSuperAdmin)
        {
            return (false, "Solo SuperAdmin puede crear otros SuperAdmin.");
        }

        var existing = await userManager.FindByEmailAsync(request.Email.Trim());
        if (existing is not null)
        {
            return (false, "Ya existe un usuario con ese correo.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            EmailConfirmed = true,
            FullName = request.FullName.Trim(),
            PhoneNumber = NormalizePhone(request.PhoneNumber),
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

    public async Task<(bool Success, string? Error)> UpdateUserAsync(UpdateUserRequest request, string currentUserId, bool isSuperAdmin)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return (false, "Usuario no encontrado.");
        }

        if (!AppRoles.All.Contains(request.Role))
        {
            return (false, "Rol inválido.");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains(AppRoles.SuperAdmin) && request.Role != AppRoles.SuperAdmin && !isSuperAdmin)
        {
            return (false, "No puede cambiar el rol de un SuperAdmin.");
        }

        if (request.Role == AppRoles.SuperAdmin && !isSuperAdmin)
        {
            return (false, "Solo SuperAdmin puede asignar rol SuperAdmin.");
        }

        var email = request.Email.Trim();
        var emailOwner = await userManager.FindByEmailAsync(email);
        if (emailOwner is not null && emailOwner.Id != user.Id)
        {
            return (false, "Ya existe otro usuario con ese correo.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = NormalizePhone(request.PhoneNumber);
        user.IsActive = request.IsActive;

        if (user.Id == currentUserId && !request.IsActive)
        {
            return (false, "No puede desactivar su propia cuenta.");
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return (false, string.Join(" ", updateResult.Errors.Select(e => e.Description)));
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!passwordResult.Succeeded)
            {
                return (false, string.Join(" ", passwordResult.Errors.Select(e => e.Description)));
            }
        }

        foreach (var role in currentRoles)
        {
            await userManager.RemoveFromRoleAsync(user, role);
        }

        await userManager.AddToRoleAsync(user, request.Role);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(string userId, string currentUserId, bool isSuperAdmin)
    {
        if (userId == currentUserId)
        {
            return (false, "No puede eliminar su propia cuenta.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "Usuario no encontrado.");
        }

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains(AppRoles.SuperAdmin))
        {
            var superAdmins = await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);
            if (superAdmins.Count <= 1)
            {
                return (false, "Debe existir al menos un SuperAdmin.");
            }
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ToggleActiveAsync(string userId, string currentUserId, bool isSuperAdmin)
    {
        if (userId == currentUserId)
        {
            return (false, "No puede desactivar su propia cuenta.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "Usuario no encontrado.");
        }

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains(AppRoles.SuperAdmin) && user.IsActive)
        {
            var superAdmins = await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);
            var activeSuperAdmins = superAdmins.Count(u => u.IsActive);
            if (activeSuperAdmins <= 1)
            {
                return (false, "Debe existir al menos un SuperAdmin activo.");
            }
        }

        user.IsActive = !user.IsActive;
        await userManager.UpdateAsync(user);
        return (true, null);
    }

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return phone.Trim();
    }
}

using Microsoft.AspNetCore.Identity;
using SubliSport.Domain.Constants;
using SubliSport.Domain.Entities;

namespace SubliSport.Web.Services;

public static class ImpersonationHelper
{
    public static bool IsImpersonating(HttpContext? context) =>
        context?.Request.Cookies.ContainsKey(ImpersonationConstants.CookieName) == true;

    public static bool IsSuperAdminSupportMode(IEnumerable<string> roles, HttpContext? context) =>
        roles.Contains(AppRoles.SuperAdmin) && !IsImpersonating(context);
}

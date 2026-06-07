namespace SubliSport.Domain.Constants;

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Designer = "Designer";
    public const string Production = "Production";

    public static readonly string[] All =
    [
        SuperAdmin,
        Admin,
        Designer,
        Production
    ];
}

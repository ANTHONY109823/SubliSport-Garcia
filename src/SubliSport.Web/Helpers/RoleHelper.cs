namespace SubliSport.Web.Helpers;

public static class RoleHelper
{
    public static string GetLabel(string role) => role switch
    {
        "SuperAdmin" => "Super Administrador",
        "Admin" => "Administración",
        "Designer" => "Diseñador",
        "Production" => "Producción",
        _ => role
    };
}

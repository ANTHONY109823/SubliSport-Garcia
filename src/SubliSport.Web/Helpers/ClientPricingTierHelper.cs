using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public static class ClientPricingTierHelper
{
    public static string GetLabel(ClientPricingTier tier) => tier switch
    {
        ClientPricingTier.DirectRetail => "Cliente directo",
        ClientPricingTier.MypeB2B => "MYPE / taller",
        _ => "—"
    };

    public static string GetDescription(ClientPricingTier tier) => tier switch
    {
        ClientPricingTier.DirectRetail => "Persona natural o club que cotiza por prenda (ej. S/ 55–58 conjunto).",
        ClientPricingTier.MypeB2B => "Empresa que compra servicio: diseño, impresión, planchado y opcional confección (precio por metraje).",
        _ => ""
    };

    public static string GetBadgeClass(ClientPricingTier tier) => tier switch
    {
        ClientPricingTier.DirectRetail => "tier-badge tier-retail",
        ClientPricingTier.MypeB2B => "tier-badge tier-mype",
        _ => "tier-badge"
    };
}

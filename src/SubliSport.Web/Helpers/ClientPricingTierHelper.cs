using SubliSport.Domain.Enums;

namespace SubliSport.Web.Helpers;

public static class ClientPricingTierHelper
{
    public static string GetLabel(ClientPricingTier tier) => tier switch
    {
        ClientPricingTier.DirectRetail => "Cliente directo",
        ClientPricingTier.MypeB2B => "Por servicio",
        _ => "—"
    };

    public static string GetDescription(ClientPricingTier tier) => tier switch
    {
        ClientPricingTier.DirectRetail => "Persona natural o club que cotiza por prenda (ej. S/ 55–58 conjunto).",
        ClientPricingTier.MypeB2B => "Taller o empresa — servicio por metraje (diseño, impresión, planchado y confección opcional).",
        _ => ""
    };

    public static string GetBadgeClass(ClientPricingTier tier) => tier switch
    {
        ClientPricingTier.DirectRetail => "tier-badge tier-retail",
        ClientPricingTier.MypeB2B => "tier-badge tier-mype",
        _ => "tier-badge"
    };
}

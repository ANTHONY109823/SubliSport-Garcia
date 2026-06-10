using System.Text;
using SubliSport.Domain.Landing;
using SubliSport.Web.Helpers;

namespace SubliSport.Web.Services;

public static class LandingQuoteCalculator
{
    private static readonly Dictionary<string, decimal[]> ConjuntoPrices = new()
    {
        ["xxl_xl"] = [49, 50, 51, 52, 54, 55],
        ["l_m_s"] = [48, 49, 50, 51, 53, 54],
        ["s_14_16"] = [46, 47, 48, 49, 51, 52],
        ["s_10_12"] = [45, 46, 47, 48, 50, 54],
        ["s_4_6_8"] = [44, 45, 46, 47, 49, 50]
    };

    private static readonly Dictionary<string, decimal[]> CamisetaPrices = new()
    {
        ["xxl_xl"] = [32, 33, 34, 35, 36, 37],
        ["l_m_s"] = [30, 31, 32, 33, 34, 35],
        ["s_14_16"] = [28, 29, 30, 31, 32, 33],
        ["s_10_12"] = [26, 27, 28, 29, 30, 31],
        ["s_4_6_8"] = [24, 25, 26, 27, 28, 29]
    };

    private static readonly string[] FabricKeys =
        ["dry_fit", "poly_exagonal", "puma", "gota_sig_sag", "marathon_micro", "labrado_brillo"];

    public const decimal EmbroideryInsigniaUnit = 3m;
    public const decimal EmbroideryBrandUnit = 2m;

    public static LandingQuoteResult Calculate(LandingQuoteSubmitRequest request)
    {
        var fabricKey = NormalizeFabricKey(request.FabricKey);
        var fabricIndex = Array.IndexOf(FabricKeys, fabricKey);
        if (fabricIndex < 0) fabricIndex = 0;

        var fabricLabel = LandingFabricCatalog.Fabrics.First(f => f.Key == FabricKeys[fabricIndex]).Label;
        var category = ResolveGarmentCategory(request.GarmentType);
        var lines = BuildLines(request, category, fabricIndex);
        var subtotal = lines.Sum(l => l.LineTotal);

        var insigniaTotal = Math.Max(0, request.EmbroideryInsigniaQty) * EmbroideryInsigniaUnit;
        var brandTotal = Math.Max(0, request.EmbroideryBrandQty) * EmbroideryBrandUnit;
        var total = subtotal + insigniaTotal + brandTotal;

        var proforma = BuildProforma(request, fabricLabel, category, lines, subtotal, insigniaTotal, brandTotal, total);
        var waSummary = BuildWhatsAppSummary(request, fabricLabel, total, proforma);

        return new LandingQuoteResult(
            subtotal,
            insigniaTotal,
            brandTotal,
            total,
            fabricLabel,
            category,
            lines,
            proforma,
            waSummary);
    }

    private static List<LandingQuoteLineItem> BuildLines(
        LandingQuoteSubmitRequest request,
        string category,
        int fabricIndex)
    {
        var roster = request.Roster
            .Where(r => !string.IsNullOrWhiteSpace(r.Name) ||
                        !string.IsNullOrWhiteSpace(r.Size) ||
                        !string.IsNullOrWhiteSpace(r.Number))
            .ToList();

        if (roster.Count > 0)
        {
            return roster.Select(r =>
            {
                var tier = ResolveSizeTier(r.Size);
                var unit = UnitPrice(category, tier, fabricIndex);
                return new LandingQuoteLineItem(
                    string.IsNullOrWhiteSpace(r.Name) ? request.GarmentType : r.Name,
                    string.IsNullOrWhiteSpace(r.Size) ? tier : r.Size.Trim(),
                    1,
                    unit,
                    unit);
            }).ToList();
        }

        var qty = Math.Max(1, request.Quantity);
        var tier = ResolveSizeTier(request.SizeRangeSummary ?? "M");
        var price = UnitPrice(category, tier, fabricIndex);
        return
        [
            new LandingQuoteLineItem(request.GarmentType, tier, qty, price, price * qty)
        ];
    }

    private static decimal UnitPrice(string category, string tier, int fabricIndex)
    {
        var table = category switch
        {
            "camiseta" => CamisetaPrices,
            "short" => null,
            _ => ConjuntoPrices
        };

        if (table is null)
        {
            var conj = ConjuntoPrices[tier][fabricIndex];
            var cam = CamisetaPrices[tier][fabricIndex];
            return Math.Round((conj - cam) * 0.7m, 2);
        }

        return table.TryGetValue(tier, out var prices) ? prices[fabricIndex] : table["l_m_s"][fabricIndex];
    }

    private static string ResolveGarmentCategory(string garmentType)
    {
        var g = garmentType.ToLowerInvariant();
        if (g.Contains("conjunto")) return "conjunto";
        if (g.Contains("camiseta")) return "camiseta";
        if (g.Contains("short")) return "short";
        return "conjunto";
    }

    private static string NormalizeFabricKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return FabricKeys[0];
        var k = key.Trim().ToLowerInvariant();
        return FabricKeys.Contains(k) ? k : FabricKeys[0];
    }

    public static string ResolveSizeTier(string? size)
    {
        if (string.IsNullOrWhiteSpace(size)) return "l_m_s";

        var s = size.Trim().ToUpperInvariant()
            .Replace("TALLA", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (s.Contains("XXL", StringComparison.Ordinal) || s == "XL" || s.Contains(" XL"))
            return "xxl_xl";
        if (s is "4" or "6" or "8" or "04" or "06" or "08" or "4A" or "6A" or "8A")
            return "s_4_6_8";
        if (s is "10" or "12" or "10A" or "12A")
            return "s_10_12";
        if (s is "14" or "16" or "14A" or "16A")
            return "s_14_16";
        if (s.Contains("MIXT", StringComparison.Ordinal) || s.Contains('/'))
            return "l_m_s";

        return "l_m_s";
    }

    private static string BuildProforma(
        LandingQuoteSubmitRequest request,
        string fabricLabel,
        string category,
        List<LandingQuoteLineItem> lines,
        decimal subtotal,
        decimal insigniaTotal,
        decimal brandTotal,
        decimal total)
    {
        var sb = new StringBuilder();
        sb.AppendLine("══════════════════════════════════");
        sb.AppendLine($"  {LandingCompanyInfo.BusinessName}");
        sb.AppendLine($"  RUC: {LandingCompanyInfo.Ruc}");
        sb.AppendLine($"  {LandingCompanyInfo.Address}");
        sb.AppendLine("══════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("PROFORMA / COTIZACIÓN (referencial)");
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("DATOS DEL CLIENTE");
        sb.AppendLine($"  Nombre/Club: {request.ClientName}");
        if (!string.IsNullOrWhiteSpace(request.ClientPhone))
            sb.AppendLine($"  WhatsApp: {request.ClientPhone}");
        sb.AppendLine($"  Deporte: {request.Sport}");
        sb.AppendLine();
        sb.AppendLine("DETALLE DEL PEDIDO");
        sb.AppendLine($"  Prenda: {request.GarmentType}");
        sb.AppendLine($"  Tela: {fabricLabel}");
        sb.AppendLine($"  Tipo tarifa: {CategoryLabel(category)}");
        sb.AppendLine();

        var i = 1;
        foreach (var line in lines)
        {
            sb.AppendLine($"  {i}. {line.Description} · Talla {line.Size} · S/ {line.UnitPrice:N2} x {line.Quantity} = S/ {line.LineTotal:N2}");
            i++;
        }

        if (request.Roster.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("  Lista (nombre · talla · número):");
            foreach (var r in request.Roster.Where(x =>
                         !string.IsNullOrWhiteSpace(x.Name) ||
                         !string.IsNullOrWhiteSpace(x.Size) ||
                         !string.IsNullOrWhiteSpace(x.Number)))
            {
                sb.AppendLine($"    · {r.Name} | {r.Size} | N°{r.Number}");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"  Subtotal prendas: S/ {subtotal:N2}");
        if (insigniaTotal > 0)
            sb.AppendLine($"  Bordado insignia ({request.EmbroideryInsigniaQty} uds.): S/ {insigniaTotal:N2}");
        if (brandTotal > 0)
            sb.AppendLine($"  Bordado marca ({request.EmbroideryBrandQty} uds.): S/ {brandTotal:N2}");
        sb.AppendLine($"  TOTAL REFERENCIAL: S/ {total:N2}");
        sb.AppendLine("  * Precios NO incluyen IGV");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            sb.AppendLine($"Observaciones cliente: {request.Notes}");
            sb.AppendLine();
        }

        sb.AppendLine(LandingQuoteTerms.Text);
        return sb.ToString().TrimEnd();
    }

    private static string BuildWhatsAppSummary(
        LandingQuoteSubmitRequest request,
        string fabricLabel,
        decimal total,
        string fullProforma)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🏅 *SOLICITUD DE COTIZACIÓN — SUBLISPORT GARCIA*");
        sb.AppendLine();
        sb.AppendLine($"👤 *Cliente:* {request.ClientName}");
        if (!string.IsNullOrWhiteSpace(request.ClientPhone))
            sb.AppendLine($"📱 *Mi WhatsApp:* {request.ClientPhone}");
        sb.AppendLine($"👕 *Prenda:* {request.GarmentType}");
        sb.AppendLine($"🧵 *Tela:* {fabricLabel}");
        sb.AppendLine($"⚽ *Deporte:* {request.Sport}");
        sb.AppendLine($"💰 *Total referencial:* S/ {total:N2} (sin IGV)");
        if (!string.IsNullOrWhiteSpace(request.Notes))
            sb.AppendLine($"📝 *Detalles:* {request.Notes}");
        sb.AppendLine();
        sb.AppendLine("📋 *Resumen proforma automática:*");
        sb.AppendLine(WhatsAppHelper.TruncateForWhatsApp(fullProforma, 1200));
        sb.AppendLine();
        sb.AppendLine("✅ Solicitud registrada en panel SubliSport (cotización pendiente).");
        sb.AppendLine("Adjunto foto de referencia si la subí en el formulario.");
        return sb.ToString().TrimEnd();
    }

    private static string CategoryLabel(string category) => category switch
    {
        "camiseta" => "Camiseta varón/dama",
        "short" => "Short (tarifa referencial)",
        _ => "Conjunto (camiseta + short + medias)"
    };
}

public static class LandingQuoteTerms
{
    public static string Text => """
        CONDICIONES COMERCIALES
        1. Los precios están considerados a partir de una docena.
        2. El tiempo de entrega es 07 días hábiles después del adelanto del 50%.
        3. El cliente podrá realizar cambios 3 veces; posterior a ello pagará por diseño.
        4. El cliente pagará el envío de encomienda.
        5. Los costos de este presupuesto NO incluyen IGV.

        FORMA DE PAGO
        1. 50% de adelanto — inicio según acuerdo.
        2. 50% restante el mismo día del envío o entrega.
        3. YAPE / PLIN: 960 840 874 · 982 765 879
        4. BCP: 191-06073742092
        5. INTERBANK: 898-3233877451
        6. SCOTIABANK: 970-1614761
        7. BANCO DE LA NACIÓN: 04-074-267780
        8. Cuenta a nombre de LIZARDO EPIFANIO GARCIA CCAYO

        BORDADO (si aplica)
        · Insignia: S/ 3.00 c/u
        · Marca: S/ 2.00 c/u
        """;
}

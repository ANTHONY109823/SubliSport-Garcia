using System.Text;
using SubliSport.Domain.Landing;
using SubliSport.Web.Helpers;
namespace SubliSport.Web.Services;

public static class LandingQuoteCalculator
{
    private static readonly Dictionary<string, decimal[]> ConjuntoPrices = new()
    {
        ["xxl_xl"] = [49, 49, 50, 51, 52, 52, 55, 55, 58],
        ["l_m_s"] = [48, 48, 49, 50, 51, 51, 54, 54, 57],
        ["s_14_16"] = [46, 46, 47, 48, 49, 49, 51, 51, 52],
        ["s_10_12"] = [45, 45, 46, 47, 48, 48, 50, 50, 54],
        ["s_4_6_8"] = [44, 44, 45, 46, 47, 47, 49, 49, 50]
    };

    private static readonly Dictionary<string, decimal[]> CamisetaPrices = new()
    {
        ["xxl_xl"] = [32, 32, 33, 34, 35, 35, 36, 36, 37],
        ["l_m_s"] = [30, 30, 31, 32, 33, 33, 34, 34, 35],
        ["s_14_16"] = [28, 28, 29, 30, 31, 31, 32, 32, 33],
        ["s_10_12"] = [26, 26, 27, 28, 29, 29, 30, 30, 31],
        ["s_4_6_8"] = [24, 24, 25, 26, 27, 27, 28, 28, 29]
    };

    private static readonly string[] FabricKeys =
        ["dry_fit", "win_fresch", "poly_exagonal", "puma", "gota", "sig_sag", "marathon", "micro_nike", "labrado_brillo"];

    public const decimal EmbroideryEscudoUnit = 3m;
    public const decimal EmbroideryMarcaUnit = 2m;
    public const decimal EmbroideryShortUnit = 2m;

    public static LandingQuoteResult Calculate(LandingQuoteSubmitRequest request)
    {
        var fabricKey = NormalizeFabricKey(request.FabricKey);
        var fabricIndex = Array.IndexOf(FabricKeys, fabricKey);
        if (fabricIndex < 0) fabricIndex = 0;

        var fabricLabel = LandingFabricCatalog.GetLabel(fabricKey);
        var isMixed = IsMixedOrder(request);
        var category = isMixed ? "mixta" : ResolveGarmentCategory(request.GarmentType);
        var lines = BuildLines(request, category, fabricIndex, isMixed);
        var subtotal = lines.Sum(l => l.LineTotal);

        var pieceCount = ResolvePieceCount(request, isMixed);
        var shortCount = ResolveShortPieceCount(request, isMixed);

        var escudoTotal = request.EmbroideryEscudo ? pieceCount * EmbroideryEscudoUnit : 0m;
        var marcaTotal = request.EmbroideryMarca ? pieceCount * EmbroideryMarcaUnit : 0m;
        var shortTotal = request.EmbroideryShort ? shortCount * EmbroideryShortUnit : 0m;
        var total = subtotal + escudoTotal + marcaTotal + shortTotal;

        var adminSuggestion = BuildAdminSuggestion(request, fabricLabel, category, lines, subtotal, escudoTotal, marcaTotal, shortTotal, total, isMixed);
        var clientRequest = BuildClientRequest(request, fabricLabel, isMixed);
        var clientProforma = BuildClientProformaDraft(request, fabricLabel, isMixed, total);

        return new LandingQuoteResult(
            subtotal,
            escudoTotal,
            marcaTotal,
            shortTotal,
            total,
            fabricLabel,
            category,
            lines,
            adminSuggestion,
            clientRequest,
            clientProforma);
    }

    private static bool IsMixedOrder(LandingQuoteSubmitRequest request) =>
        request.GarmentType.Equals("Mixta", StringComparison.OrdinalIgnoreCase) ||
        request.MixedLines.Count > 0;

    private static int ResolvePieceCount(LandingQuoteSubmitRequest request, bool isMixed)
    {
        var roster = ActiveRoster(request);
        if (roster.Count > 0) return roster.Count;

        if (isMixed)
        {
            return request.MixedLines.Where(l => l.Quantity > 0).Sum(l => l.Quantity);
        }

        return Math.Max(1, request.Quantity);
    }

    private static int ResolveShortPieceCount(LandingQuoteSubmitRequest request, bool isMixed)
    {
        var roster = ActiveRoster(request);
        if (roster.Count > 0)
        {
            return roster.Count(r => RosterKitHelper.ResolveCategory(r.KitType) == "conjunto");
        }

        if (isMixed)
        {
            return request.MixedLines
                .Where(l => l.Quantity > 0 && ResolveGarmentCategory(l.ItemType) == "short")
                .Sum(l => l.Quantity);
        }

        return ResolveGarmentCategory(request.GarmentType) == "short"
            ? Math.Max(1, request.Quantity)
            : 0;
    }

    private static List<LandingQuoteRosterLine> ActiveRoster(LandingQuoteSubmitRequest request) =>
        request.Roster
            .Where(r => !string.IsNullOrWhiteSpace(r.Name) ||
                        !string.IsNullOrWhiteSpace(r.Size) ||
                        !string.IsNullOrWhiteSpace(r.Number))
            .ToList();

    private static List<LandingQuoteLineItem> BuildLines(
        LandingQuoteSubmitRequest request,
        string category,
        int fabricIndex,
        bool isMixed)
    {
        var roster = ActiveRoster(request);

        if (roster.Count > 0)
        {
            return roster.Select(r =>
            {
                var tier = ResolveSizeTier(r.Size);
                var lineCategory = RosterKitHelper.ResolveCategory(r.KitType);
                var unit = UnitPrice(lineCategory, tier, fabricIndex);
                return new LandingQuoteLineItem(
                    string.IsNullOrWhiteSpace(r.Name) ? request.GarmentType : r.Name,
                    string.IsNullOrWhiteSpace(r.Size) ? tier : r.Size.Trim(),
                    1,
                    unit,
                    unit);
            }).ToList();
        }

        if (isMixed)
        {
            var tier = ResolveSizeTier(request.SizeRangeSummary ?? "M");
            return request.MixedLines
                .Where(l => l.Quantity > 0 && !string.IsNullOrWhiteSpace(l.ItemType))
                .Select(l =>
                {
                    var lineCategory = ResolveGarmentCategory(l.ItemType);
                    var unit = UnitPrice(lineCategory, tier, fabricIndex);
                    var label = l.ItemType == "Otro" && !string.IsNullOrWhiteSpace(l.OtherDescription)
                        ? l.OtherDescription.Trim()
                        : l.ItemType;
                    return new LandingQuoteLineItem(label, tier, l.Quantity, unit, unit * l.Quantity);
                })
                .ToList();
        }

        var qty = Math.Max(1, request.Quantity);
        var sizeTier = ResolveSizeTier(request.SizeRangeSummary ?? "M");
        var price = UnitPrice(category, sizeTier, fabricIndex);
        return
        [
            new LandingQuoteLineItem(request.GarmentType, sizeTier, qty, price, price * qty)
        ];
    }

    private static decimal UnitPrice(string category, string tier, int fabricIndex)
    {
        var table = category switch
        {
            "camiseta" => CamisetaPrices,
            "short" => null,
            "medias" => null,
            _ => ConjuntoPrices
        };

        if (category == "medias")
        {
            var conj = ConjuntoPrices[tier][fabricIndex];
            return Math.Round(conj * 0.15m, 2);
        }

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
        if (g.Contains("polo") || g.Contains("camiseta")) return "camiseta";
        if (g.Contains("short") || g.Contains("pantaloneta")) return "short";
        if (g.Contains("media")) return "medias";
        return "conjunto";
    }

    private static string NormalizeFabricKey(string? key) => LandingFabricCatalog.NormalizeKey(key);

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

    private static string DescribeEmbroidery(LandingQuoteSubmitRequest request)
    {
        var parts = new List<string>();
        if (request.EmbroideryEscudo) parts.Add("Bordado logo");
        if (request.EmbroideryMarca) parts.Add("Bordado marca");
        if (request.EmbroideryShort) parts.Add("Bordado short");
        return parts.Count == 0 ? "Sublimado (sin bordado)" : string.Join(" · ", parts);
    }

    public static string BuildClientProformaDraft(
        LandingQuoteSubmitRequest request,
        string fabricLabel,
        bool isMixed,
        decimal total) =>
        BuildClientProformaDraftCore(request, fabricLabel, isMixed, total);

    private static string BuildAdminSuggestion(
        LandingQuoteSubmitRequest request,
        string fabricLabel,
        string category,
        List<LandingQuoteLineItem> lines,
        decimal subtotal,
        decimal escudoTotal,
        decimal marcaTotal,
        decimal shortTotal,
        decimal total,
        bool isMixed)
    {
        var sb = new StringBuilder();
        sb.AppendLine("══════════════════════════════════");
        sb.AppendLine($"  {LandingCompanyInfo.BusinessName}");
        sb.AppendLine($"  RUC: {LandingCompanyInfo.Ruc}");
        sb.AppendLine($"  {LandingCompanyInfo.Address}");
        sb.AppendLine("══════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("SUGERENCIA INTERNA — SOLO PANEL ADMIN (NO ENVIAR AL CLIENTE)");
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("DATOS DEL CLIENTE");
        sb.AppendLine($"  Nombre/Club: {request.ClientName}");
        if (!string.IsNullOrWhiteSpace(request.ClientPhone))
            sb.AppendLine($"  WhatsApp: {request.ClientPhone}");
        if (!string.IsNullOrWhiteSpace(request.DesiredDeliveryDeadline))
            sb.AppendLine($"  Fecha de entrega de pedido: {request.DesiredDeliveryDeadline.Trim()}");
        sb.AppendLine();
        sb.AppendLine("DETALLE DEL PEDIDO");
        sb.AppendLine($"  Prenda: {(isMixed ? "Pedido mixto" : request.GarmentType)}");
        if (isMixed && request.MixedLines.Count > 0)
        {
            foreach (var m in request.MixedLines.Where(l => l.Quantity > 0))
            {
                var label = m.ItemType == "Otro" && !string.IsNullOrWhiteSpace(m.OtherDescription)
                    ? m.OtherDescription.Trim()
                    : m.ItemType;
                sb.AppendLine($"    · {m.Quantity} x {label}");
            }
        }
        sb.AppendLine($"  Tela: {fabricLabel}");
        sb.AppendLine($"  Acabado: {DescribeEmbroidery(request)}");
        if (!isMixed)
            sb.AppendLine($"  Tipo tarifa: {CategoryLabel(category)}");
        sb.AppendLine();

        var i = 1;
        foreach (var line in lines)
        {
            sb.AppendLine($"  {i}. {line.Description} · Talla {line.Size} · S/ {line.UnitPrice:N2} x {line.Quantity} = S/ {line.LineTotal:N2}");
            i++;
        }

        var roster = ActiveRoster(request);
        if (roster.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("  Lista (nombre · talla · número · corte · prenda):");
            foreach (var r in roster)
                sb.AppendLine($"    · {r.Name} | {r.Size} | N°{r.Number} | {RosterGenderHelper.GetCutLabel(r.Gender)} | {RosterKitHelper.GetDisplayLabel(r.KitType)}");
        }

        sb.AppendLine();
        sb.AppendLine($"  Subtotal prendas: S/ {subtotal:N2}");
        if (escudoTotal > 0)
            sb.AppendLine($"  Bordado logo: S/ {escudoTotal:N2}");
        if (marcaTotal > 0)
            sb.AppendLine($"  Bordado marca: S/ {marcaTotal:N2}");
        if (shortTotal > 0)
            sb.AppendLine($"  Bordado short: S/ {shortTotal:N2}");
        sb.AppendLine($"  TOTAL REFERENCIAL: S/ {total:N2}");
        sb.AppendLine("  * Precios NO incluyen IGV");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            sb.AppendLine($"Observaciones cliente: {request.Notes}");
            sb.AppendLine();
        }

        sb.AppendLine("Nota: Ajuste el precio final según tallas, cantidad y dificultad del diseño.");
        return sb.ToString().TrimEnd();
    }

    private static string BuildClientProformaDraftCore(
        LandingQuoteSubmitRequest request,
        string fabricLabel,
        bool isMixed,
        decimal total)
    {
        var sb = new StringBuilder();
        sb.AppendLine("══════════════════════════════════");
        sb.AppendLine($"  {LandingCompanyInfo.BusinessName}");
        sb.AppendLine($"  RUC: {LandingCompanyInfo.Ruc}");
        sb.AppendLine($"  {LandingCompanyInfo.Address}");
        sb.AppendLine("══════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("PROFORMA / COTIZACIÓN");
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("DATOS DEL CLIENTE");
        sb.AppendLine($"  Nombre/Club: {request.ClientName}");
        if (!string.IsNullOrWhiteSpace(request.ClientPhone))
            sb.AppendLine($"  WhatsApp: {request.ClientPhone}");
        if (!string.IsNullOrWhiteSpace(request.DesiredDeliveryDeadline))
            sb.AppendLine($"  Fecha de entrega de pedido: {request.DesiredDeliveryDeadline.Trim()}");
        sb.AppendLine();
        sb.AppendLine("DETALLE DEL PEDIDO");
        sb.AppendLine($"  Prenda: {(isMixed ? "Pedido mixto" : request.GarmentType)}");
        if (isMixed && request.MixedLines.Count > 0)
        {
            foreach (var m in request.MixedLines.Where(l => l.Quantity > 0))
            {
                var label = m.ItemType == "Otro" && !string.IsNullOrWhiteSpace(m.OtherDescription)
                    ? m.OtherDescription.Trim()
                    : m.ItemType;
                sb.AppendLine($"    · {m.Quantity} x {label}");
            }
        }
        else if (!isMixed)
        {
            sb.AppendLine($"    · Cantidad: {Math.Max(1, request.Quantity)} uds.");
        }
        sb.AppendLine($"  Tela: {fabricLabel}");
        sb.AppendLine($"  Acabado: {DescribeEmbroidery(request)}");
        if (!string.IsNullOrWhiteSpace(request.SizeRangeSummary))
            sb.AppendLine($"  Tallas generales: {request.SizeRangeSummary}");

        var roster = ActiveRoster(request);
        if (roster.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("  Lista (nombre · talla · número · corte · prenda):");
            foreach (var r in roster)
                sb.AppendLine($"    · {r.Name} | {r.Size} | N°{r.Number} | {RosterGenderHelper.GetCutLabel(r.Gender)} | {RosterKitHelper.GetDisplayLabel(r.KitType)}");
        }

        sb.AppendLine();
        sb.AppendLine($"  TOTAL: S/ {total:N2}");
        sb.AppendLine("  * Monto sujeto a confirmación. No incluye IGV.");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            sb.AppendLine($"Observaciones: {request.Notes}");
            sb.AppendLine();
        }

        sb.AppendLine(LandingQuoteTerms.ClientText);
        return sb.ToString().TrimEnd();
    }

    private static string BuildClientRequest(
        LandingQuoteSubmitRequest request,
        string fabricLabel,
        bool isMixed)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🏅 *SOLICITUD DE COTIZACIÓN — SUBLISPORT GARCIA*");
        sb.AppendLine();
        sb.AppendLine($"👤 *Nombre/Club:* {request.ClientName}");
        if (!string.IsNullOrWhiteSpace(request.ClientPhone))
            sb.AppendLine($"📱 *Mi WhatsApp:* {request.ClientPhone}");
        sb.AppendLine($"👕 *Prenda:* {(isMixed ? "Pedido mixto" : request.GarmentType)}");
        if (isMixed && request.MixedLines.Count > 0)
        {
            foreach (var m in request.MixedLines.Where(l => l.Quantity > 0))
            {
                var label = m.ItemType == "Otro" && !string.IsNullOrWhiteSpace(m.OtherDescription)
                    ? m.OtherDescription.Trim()
                    : m.ItemType;
                sb.AppendLine($"   · {m.Quantity} x {label}");
            }
        }
        else
        {
            sb.AppendLine($"📦 *Cantidad:* {Math.Max(1, request.Quantity)} uds.");
        }
        sb.AppendLine($"🧵 *Tela:* {fabricLabel}");
        sb.AppendLine($"✨ *Acabado:* {DescribeEmbroidery(request)}");
        if (!string.IsNullOrWhiteSpace(request.DesiredDeliveryDeadline))
            sb.AppendLine($"📅 *Fecha de entrega de pedido:* {request.DesiredDeliveryDeadline.Trim()}");
        if (!string.IsNullOrWhiteSpace(request.Notes))
            sb.AppendLine($"📝 *Detalles:* {request.Notes}");

        var roster = ActiveRoster(request);
        if (roster.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("*Lista jugadores:*");
            foreach (var r in roster)
                sb.AppendLine($"· {r.Name} | Talla {r.Size} | N°{r.Number} | {RosterGenderHelper.GetCutLabel(r.Gender)} | {RosterKitHelper.GetDisplayLabel(r.KitType)}");
        }

        sb.AppendLine();
        sb.AppendLine("Solicito cotización. *Los precios los confirmará el asesor.*");
        sb.AppendLine("Gracias 😊");
        return sb.ToString().TrimEnd();
    }

    private static string CategoryLabel(string category) => category switch
    {
        "camiseta" => "Camiseta varón/dama",
        "short" => "Short (tarifa referencial)",
        "medias" => "Medias",
        "mixta" => "Pedido mixto",
        _ => "Conjunto (camiseta + short + medias)"
    };
}

public static class LandingQuoteTerms
{
    public const string PaymentMethodsShort =
        "YAPE / PLIN: 960 840 874 · 982 765 879 · BCP: 191-06073742092 · INTERBANK: 898-3233877451 · Cuenta: LIZARDO EPIFANIO GARCIA CCAYO";

    public const string PrintFooter =
        "Términos y condiciones: Cotización referencial sujeta a confirmación. Precios desde una docena. Entrega aproximada de 7 días hábiles tras el adelanto del 50% y la aprobación del diseño. Hasta 3 cambios de diseño incluidos; cambios adicionales tienen costo. Envío de encomienda por cuenta del cliente. Montos sin IGV. Adelanto 50% para iniciar; saldo al entregar o enviar.";

    public static string ClientText => $"""
        Hola, le enviamos su cotización de SubliSport García.

        {PaymentMethodsShort}

        {PrintFooter}
        """;

    public static string Text => """
        CONDICIONES COMERCIALES
        1. Los precios están considerados a partir de una docena.
        2. El tiempo de entrega es 07 días hábiles después del adelanto del 50%.
        3. Plazo máximo recomendado: 07 días hábiles después de la aprobación del diseño, sujeto a la carga de trabajo vigente.
        4. El cliente podrá realizar cambios 3 veces; posterior a ello pagará por diseño.
        5. El cliente pagará el envío de encomienda.
        6. Los costos de este presupuesto NO incluyen IGV.

        FORMA DE PAGO
        1. 50% de adelanto — inicio según acuerdo.
        2. 50% restante el mismo día del envío o entrega.
        3. YAPE / PLIN: 960 840 874 · 982 765 879
        4. BCP: 191-06073742092
        5. INTERBANK: 898-3233877451
        6. SCOTIABANK: 970-1614761
        7. BANCO DE LA NACIÓN: 04-074-267780
        8. Cuenta a nombre de LIZARDO EPIFANIO GARCIA CCAYO

        BORDADO (opcional — si no marca ninguna opción, todo es sublimado)
        · Escudo / insignia: S/ 3.00 c/u
        · Marca: S/ 2.00 c/u
        · Short: S/ 2.00 c/u
        """;
}

using System.Text;
using System.Text.Json;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Landing;
using SubliSport.Domain.Orders;
using SubliSport.Web.Services;

namespace SubliSport.Web.Helpers;

public static class OrderProformaBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string BuildClientDraft(Order order, decimal chargeAmount)
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
        if (!string.IsNullOrWhiteSpace(order.OrderNumber))
        {
            sb.AppendLine($"Referencia: {order.OrderNumber}");
        }

        sb.AppendLine();
        sb.AppendLine("DATOS DEL CLIENTE");
        sb.AppendLine($"  Nombre/Club: {order.ClientName}");
        if (!string.IsNullOrWhiteSpace(order.ClientPhone))
        {
            sb.AppendLine($"  WhatsApp: {order.ClientPhone}");
        }

        sb.AppendLine($"  Deporte: {order.Sport}");
        sb.AppendLine($"  Fecha de ingreso de pedido: {order.ReceivedAt.ToLocalTime():dd/MM/yyyy}");
        if (order.AgreedDeliveryDate.HasValue)
        {
            sb.AppendLine($"  Fecha de entrega de pedido: {order.AgreedDeliveryDate.Value.ToLocalTime():dd/MM/yyyy}");
        }

        sb.AppendLine();
        sb.AppendLine("DETALLE DEL PEDIDO");
        sb.AppendLine($"  Prenda: {GarmentTypeHelper.GetLabel(order.GarmentType)}");
        if (GarmentTypeHelper.IsMixed(order.GarmentType))
        {
            foreach (var line in MixedGarmentHelper.Parse(order.MixedGarmentDetails))
            {
                sb.AppendLine($"    · {MixedGarmentHelper.FormatLine(line)}");
            }
        }
        else
        {
            sb.AppendLine($"    · Cantidad: {order.Quantity} uds.");
        }

        if (!string.IsNullOrWhiteSpace(order.FabricTypeName))
        {
            sb.AppendLine($"  Tela: {order.FabricTypeName}");
        }

        if (!string.IsNullOrWhiteSpace(order.SizeRange))
        {
            sb.AppendLine($"  Tallas: {order.SizeRange}");
        }

        AppendRoster(sb, order.ConfectionRosterDetails);

        if (order.GiftOption != Domain.Enums.GiftOption.None)
        {
            sb.AppendLine($"  Obsequio: {GiftOptionHelper.GetLabel(order.GiftOption)}");
        }

        sb.AppendLine();
        if (chargeAmount > 0)
        {
            sb.AppendLine($"  TOTAL: S/ {chargeAmount:N2}");
            sb.AppendLine("  * Monto sujeto a confirmación. No incluye IGV.");
        }
        else
        {
            sb.AppendLine("  TOTAL: S/ ________");
            sb.AppendLine("  * Complete el monto antes de enviar al cliente.");
        }

        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(order.Notes))
        {
            sb.AppendLine($"Observaciones: {order.Notes}");
            sb.AppendLine();
        }

        sb.AppendLine(LandingQuoteTerms.ClientText);
        return sb.ToString().TrimEnd();
    }

    private static void AppendRoster(StringBuilder sb, string? rosterJson)
    {
        if (string.IsNullOrWhiteSpace(rosterJson))
        {
            return;
        }

        try
        {
            var roster = JsonSerializer.Deserialize<List<ConfectionRosterLine>>(rosterJson, JsonOptions);
            if (roster is null || roster.Count == 0)
            {
                return;
            }

            sb.AppendLine();
            sb.AppendLine("  Lista (nombre · talla · número):");
            foreach (var r in roster)
            {
                sb.AppendLine($"    · {r.Name} | {r.Size} | N°{r.Number}");
            }
        }
        catch
        {
            // ignore invalid roster json
        }
    }
}

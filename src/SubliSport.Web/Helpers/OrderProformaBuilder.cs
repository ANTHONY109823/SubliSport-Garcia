using System.Text;
using SubliSport.Domain.Entities;
using SubliSport.Domain.Landing;
using SubliSport.Web.Services;

namespace SubliSport.Web.Helpers;

public static class OrderProformaBuilder
{
    public static string BuildClientDraft(Order order, decimal chargeAmount)
    {
        var doc = ProformaDocumentBuilder.Build(order, chargeAmount);
        var sb = new StringBuilder();

        sb.AppendLine($"*COTIZACIÓN — {LandingCompanyInfo.BusinessName}*");
        sb.AppendLine();
        sb.AppendLine($"Cliente: {order.ClientName}");
        if (!string.IsNullOrWhiteSpace(order.OrderNumber))
        {
            sb.AppendLine($"Pedido: {order.OrderNumber}");
        }

        sb.AppendLine($"Fecha: {doc.IssueDate:dd/MM/yyyy}");
        if (order.AgreedDeliveryDate.HasValue)
        {
            sb.AppendLine($"Entrega estimada: {order.AgreedDeliveryDate.Value.ToLocalTime():dd/MM/yyyy}");
        }

        sb.AppendLine();
        foreach (var line in doc.Lines)
        {
            sb.AppendLine($"· {line.Description}");
            sb.AppendLine($"  {line.Units} uds. — {(line.LineTotal is > 0 ? $"S/ {line.LineTotal:N2}" : "precio a confirmar")}");
        }

        sb.AppendLine();
        if (chargeAmount > 0)
        {
            sb.AppendLine($"*TOTAL: S/ {chargeAmount:N2}* (sin IGV)");
        }
        else
        {
            sb.AppendLine("*TOTAL: a confirmar*");
        }

        if (!string.IsNullOrWhiteSpace(order.Notes))
        {
            sb.AppendLine();
            sb.AppendLine($"Notas: {order.Notes}");
        }

        sb.AppendLine();
        sb.AppendLine(LandingQuoteTerms.PaymentMethodsShort);
        sb.AppendLine();
        sb.AppendLine("Puede solicitar el PDF detallado desde nuestro equipo.");
        sb.AppendLine();
        sb.AppendLine(LandingQuoteTerms.PrintFooter);

        return sb.ToString().TrimEnd();
    }
}

namespace SubliSport.Web.Helpers;

public static class WhatsAppHelper
{
    public static string? BuildChatUrl(string? phone, string? prefilledMessage = null)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return null;

        if (digits.Length == 9)
        {
            digits = "51" + digits;
        }

        if (digits.Length < 10) return null;

        var url = $"https://wa.me/{digits}";
        if (!string.IsNullOrWhiteSpace(prefilledMessage))
        {
            url += $"?text={Uri.EscapeDataString(prefilledMessage)}";
        }

        return url;
    }

    public static string FormatDisplayPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "—";
        return phone.Trim();
    }

    public static string BuildOrderMessage(string clientName, string orderNumber) =>
        $"Hola {clientName}, le escribe el equipo de diseño de SubliSport García respecto a su pedido {orderNumber}.";

    public static string BuildProductionFileMessage(string orderNumber, string clientName, string? designerName = null)
    {
        var designerPart = string.IsNullOrWhiteSpace(designerName) ? string.Empty : $" · diseño: {designerName}";
        return $"Pedido {orderNumber} ({clientName}){designerPart}. Busco el archivo para imprimir en WhatsApp.";
    }
}

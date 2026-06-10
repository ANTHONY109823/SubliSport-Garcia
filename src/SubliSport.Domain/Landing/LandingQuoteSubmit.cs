namespace SubliSport.Domain.Landing;

public class LandingQuoteRosterLine
{
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
}

public class LandingQuoteSubmitRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string? ClientPhone { get; set; }
    public string GarmentType { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string? SizeRangeSummary { get; set; }
    public string? Notes { get; set; }
    public List<LandingQuoteRosterLine> Roster { get; set; } = [];
    public string? ReferenceImageBase64 { get; set; }
    public string? FabricKey { get; set; }
    public int EmbroideryInsigniaQty { get; set; }
    public int EmbroideryBrandQty { get; set; }
}

public class LandingQuoteSubmitResponse
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string? ReferenceImageUrl { get; set; }
    public decimal QuotedTotal { get; set; }
    public string ProformaText { get; set; } = string.Empty;
    public string WhatsAppText { get; set; } = string.Empty;
    public string ClientWhatsAppUrl { get; set; } = string.Empty;
}

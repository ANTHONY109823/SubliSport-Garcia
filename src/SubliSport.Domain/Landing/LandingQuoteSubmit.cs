namespace SubliSport.Domain.Landing;

public class LandingQuoteRosterLine
{
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Gender { get; set; } = "Varon";
}

public class LandingQuoteMixedLine
{
    public string ItemType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? OtherDescription { get; set; }
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
    public string? DesiredDeliveryDeadline { get; set; }
    public List<LandingQuoteRosterLine> Roster { get; set; } = [];
    public List<LandingQuoteMixedLine> MixedLines { get; set; } = [];
    public string? ReferenceImageBase64 { get; set; }
    public List<string> ReferenceImagesBase64 { get; set; } = [];
    public string? FabricKey { get; set; }
    public bool EmbroideryEscudo { get; set; }
    public bool EmbroideryMarca { get; set; }
    public bool EmbroideryShort { get; set; }
    /// <summary>0 = Cliente directo, 1 = Por servicio (MYPE/B2B).</summary>
    public int PricingTier { get; set; }
}

public class LandingQuoteSubmitResponse
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string? ReferenceImageUrl { get; set; }
    public List<string> ReferenceImageUrls { get; set; } = [];
    public string ClientRequestText { get; set; } = string.Empty;
    public string ClientWhatsAppUrl { get; set; } = string.Empty;
}

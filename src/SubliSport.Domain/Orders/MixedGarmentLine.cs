namespace SubliSport.Domain.Orders;

public class MixedGarmentLine
{
    public string ItemType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? OtherDescription { get; set; }
}

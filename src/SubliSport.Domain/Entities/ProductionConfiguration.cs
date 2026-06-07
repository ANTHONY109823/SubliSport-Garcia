namespace SubliSport.Domain.Entities;

public class ProductionConfiguration
{
    public int Id { get; set; } = 1;
    public string JsonData { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
}

using Microsoft.AspNetCore.Identity;

namespace SubliSport.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<Order> AssignedDesignOrders { get; set; } = [];
    public ICollection<Order> CreatedOrders { get; set; } = [];
    public ICollection<OrderStatusHistory> StatusChanges { get; set; } = [];
}

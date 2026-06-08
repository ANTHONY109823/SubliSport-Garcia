using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SubliSport.Domain.Entities;

namespace SubliSport.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<PricingConfiguration> PricingConfigurations => Set<PricingConfiguration>();
    public DbSet<ProductionConfiguration> ProductionConfigurations => Set<ProductionConfiguration>();
    public DbSet<LandingConfiguration> LandingConfigurations => Set<LandingConfiguration>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.Priority);
            entity.HasIndex(o => o.AgreedDeliveryDate);
            entity.HasIndex(o => o.CreatedAt);

            entity.Property(o => o.OrderNumber).HasMaxLength(32).IsRequired();
            entity.Property(o => o.ClientName).HasMaxLength(200).IsRequired();
            entity.Property(o => o.ClientPhone).HasMaxLength(30);
            entity.Property(o => o.ClientEmail).HasMaxLength(256);
            entity.Property(o => o.GarmentType).HasMaxLength(120).IsRequired();
            entity.Property(o => o.Sport).HasMaxLength(80).IsRequired();
            entity.Property(o => o.SizeRange).HasMaxLength(80);
            entity.Property(o => o.Notes).HasMaxLength(2000);
            entity.Property(o => o.FabricTypeName).HasMaxLength(120);
            entity.Property(o => o.FabricTypeRipName).HasMaxLength(120);
            entity.Property(o => o.PricingNotes).HasMaxLength(1000);
            entity.Property(o => o.FabricMeters).HasPrecision(10, 2);
            entity.Property(o => o.FabricMetersRip).HasPrecision(10, 2);
            entity.Property(o => o.CalculatedFabricCost).HasPrecision(12, 2);
            entity.Property(o => o.CalculatedFabricRipCost).HasPrecision(12, 2);
            entity.Property(o => o.CalculatedLaserCost).HasPrecision(12, 2);
            entity.Property(o => o.CalculatedPrintPressCost).HasPrecision(12, 2);
            entity.Property(o => o.CalculatedExtraCost).HasPrecision(12, 2);
            entity.Property(o => o.CalculatedConfectionCost).HasPrecision(12, 2);
            entity.Property(o => o.CalculatedTotal).HasPrecision(12, 2);
            entity.Property(o => o.ChargeAmount).HasPrecision(12, 2);

            entity.HasOne(o => o.CreatedByUser)
                .WithMany(u => u.CreatedOrders)
                .HasForeignKey(o => o.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.AssignedDesigner)
                .WithMany(u => u.AssignedDesignOrders)
                .HasForeignKey(o => o.AssignedDesignerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasIndex(h => h.OrderId);
            entity.HasIndex(h => h.ChangedAt);

            entity.Property(h => h.Comment).HasMaxLength(1000);

            entity.HasOne(h => h.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.ChangedByUser)
                .WithMany(u => u.StatusChanges)
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PricingConfiguration>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.JsonData).IsRequired();
        });

        builder.Entity<ProductionConfiguration>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.JsonData).IsRequired();
        });

        builder.Entity<LandingConfiguration>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.JsonData).IsRequired();
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        });
    }
}

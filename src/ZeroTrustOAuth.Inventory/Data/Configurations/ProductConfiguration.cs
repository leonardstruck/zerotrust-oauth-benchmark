using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Category)
            .HasMaxLength(100);

        builder.Property(e => e.SupplierId)
            .HasMaxLength(50);

        builder.HasIndex(e => e.Sku)
            .IsUnique();

        builder.HasIndex(e => e.Category);

        var laptop = Product.Create(
            "Laptop",
            "LAP-001",
            25,
            10,
            "High-performance laptop for business",
            "Electronics",
            "SUP-001"
        ).Value;
        laptop.Id = "1";
        laptop.CreatedAt = DateTime.UtcNow.AddDays(-30);
        laptop.UpdatedAt = DateTime.UtcNow.AddDays(-30);

        var mouse = Product.Create(
            "Wireless Mouse",
            "MOU-001",
            150,
            50,
            "Ergonomic wireless mouse",
            "Electronics",
            "SUP-002"
        ).Value;
        mouse.Id = "2";
        mouse.CreatedAt = DateTime.UtcNow.AddDays(-20);
        mouse.UpdatedAt = DateTime.UtcNow.AddDays(-20);

        var chair = Product.Create(
            "Office Chair",
            "CHR-001",
            8,
            10,
            "Comfortable ergonomic office chair",
            "Furniture",
            "SUP-003"
        ).Value;
        chair.Id = "3";
        chair.CreatedAt = DateTime.UtcNow.AddDays(-15);
        chair.UpdatedAt = DateTime.UtcNow.AddDays(-15);

        var monitor = Product.Create(
            "Monitor 27\"",
            "MON-001",
            40,
            15,
            "4K Ultra HD monitor",
            "Electronics",
            "SUP-001"
        ).Value;
        monitor.Id = "4";
        monitor.CreatedAt = DateTime.UtcNow.AddDays(-25);
        monitor.UpdatedAt = DateTime.UtcNow.AddDays(-25);

        var lamp = Product.Create(
            "Desk Lamp",
            "LMP-001",
            5,
            20,
            "LED desk lamp with adjustable brightness",
            "Office Supplies",
            "SUP-002"
        ).Value;
        lamp.Id = "5";
        lamp.CreatedAt = DateTime.UtcNow.AddDays(-10);
        lamp.UpdatedAt = DateTime.UtcNow.AddDays(-10);

        builder.HasData(laptop, mouse, chair, monitor, lamp);
    }
}

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
        laptop.CreatedAt = new DateTime(2024, 10, 25, 0, 0, 0, DateTimeKind.Utc);
        laptop.UpdatedAt = new DateTime(2024, 10, 25, 0, 0, 0, DateTimeKind.Utc);

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
        mouse.CreatedAt = new DateTime(2024, 11, 4, 0, 0, 0, DateTimeKind.Utc);
        mouse.UpdatedAt = new DateTime(2024, 11, 4, 0, 0, 0, DateTimeKind.Utc);

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
        chair.CreatedAt = new DateTime(2024, 11, 9, 0, 0, 0, DateTimeKind.Utc);
        chair.UpdatedAt = new DateTime(2024, 11, 9, 0, 0, 0, DateTimeKind.Utc);

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
        monitor.CreatedAt = new DateTime(2024, 10, 30, 0, 0, 0, DateTimeKind.Utc);
        monitor.UpdatedAt = new DateTime(2024, 10, 30, 0, 0, 0, DateTimeKind.Utc);

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
        lamp.CreatedAt = new DateTime(2024, 11, 14, 0, 0, 0, DateTimeKind.Utc);
        lamp.UpdatedAt = new DateTime(2024, 11, 14, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(laptop, mouse, chair, monitor, lamp);
    }
}

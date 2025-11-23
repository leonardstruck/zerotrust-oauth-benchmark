using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Data;

/// <summary>
/// Database context for the Inventory service.
/// </summary>
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the Products DbSet.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Category)
                .HasMaxLength(100);

            entity.Property(e => e.SupplierId)
                .HasMaxLength(50);

            entity.HasIndex(e => e.Sku)
                .IsUnique();

            entity.HasIndex(e => e.Category);
        });

        // Seed data
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = "1",
                Name = "Laptop",
                Description = "High-performance laptop for business",
                Sku = "LAP-001",
                QuantityInStock = 25,
                ReorderLevel = 10,
                Category = "Electronics",
                SupplierId = "SUP-001",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Id = "2",
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse",
                Sku = "MOU-001",
                QuantityInStock = 150,
                ReorderLevel = 50,
                Category = "Electronics",
                SupplierId = "SUP-002",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Product
            {
                Id = "3",
                Name = "Office Chair",
                Description = "Comfortable ergonomic office chair",
                Sku = "CHR-001",
                QuantityInStock = 8,
                ReorderLevel = 10,
                Category = "Furniture",
                SupplierId = "SUP-003",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Product
            {
                Id = "4",
                Name = "Monitor 27\"",
                Description = "4K Ultra HD monitor",
                Sku = "MON-001",
                QuantityInStock = 40,
                ReorderLevel = 15,
                Category = "Electronics",
                SupplierId = "SUP-001",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Product
            {
                Id = "5",
                Name = "Desk Lamp",
                Description = "LED desk lamp with adjustable brightness",
                Sku = "LMP-001",
                QuantityInStock = 5,
                ReorderLevel = 20,
                Category = "Office Supplies",
                SupplierId = "SUP-002",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            }
        );
    }
}

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Domain.Products;

namespace ZeroTrustOAuth.Inventory.Data.Seeding;

public static class ProductsSeeding
{
    public static async Task SeedProductsAsync(this DbSet<Product> products)
    {
        if (await products.AnyAsync())
        {
            return;
        }

        await products.AddRangeAsync(
            Product.Create(
                "Laptop",
                "LAP-001",
                25,
                10,
                "High-performance laptop for business",
                "Electronics",
                "SUP-001"
            ).Value,
            Product.Create(
                "Wireless Mouse",
                "MOU-001",
                150,
                50,
                "Ergonomic wireless mouse",
                "Electronics",
                "SUP-002"
            ).Value,
            Product.Create(
                "Office Chair",
                "CHR-001",
                8,
                10,
                "Comfortable ergonomic office chair",
                "Furniture",
                "SUP-003"
            ).Value,
            Product.Create(
                "Monitor 27\"",
                "MON-001",
                40,
                15,
                "4K Ultra HD monitor",
                "Electronics",
                "SUP-001"
            ).Value,
            Product.Create(
                "Desk Lamp",
                "LMP-001",
                5,
                20,
                "LED desk lamp with adjustable brightness",
                "Office Supplies",
                "SUP-002"
            ).Value
        );
    }
}
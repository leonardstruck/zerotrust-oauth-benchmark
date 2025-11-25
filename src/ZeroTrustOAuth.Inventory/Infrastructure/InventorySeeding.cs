using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Domain.Categories;
using ZeroTrustOAuth.Inventory.Domain.Products;

namespace ZeroTrustOAuth.Inventory.Infrastructure;

public static class InventorySeeding
{
    public static async Task Seed(this InventoryDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        Category electronics = Category.Create("Electronics").Value!;
        Category furniture = Category.Create("Furniture").Value!;
        Category officeSupplies = Category.Create("Office Supplies").Value!;

        await dbContext.Categories.AddRangeAsync(electronics, furniture, officeSupplies);

        await dbContext.Products.AddRangeAsync(
            Product.Create(
                "LAP-001",
                "Laptop",
                25,
                10,
                "High-performance laptop for business",
                electronics.Id
            ).Value,
            Product.Create(
                "MOU-001",
                "Wireless Mouse",
                150,
                50,
                "Ergonomic wireless mouse",
                electronics.Id
            ).Value,
            Product.Create(
                "CHR-001",
                "Office Chair",
                8,
                10,
                "Comfortable ergonomic office chair",
                furniture.Id
            ).Value,
            Product.Create(
                "MON-001",
                "Monitor 27\"",
                40,
                15,
                "4K Ultra HD monitor",
                electronics.Id
            ).Value,
            Product.Create(
                "LMP-001",
                "Desk Lamp",
                5,
                20,
                "LED desk lamp with adjustable brightness",
                officeSupplies.Id
            ).Value
        );

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
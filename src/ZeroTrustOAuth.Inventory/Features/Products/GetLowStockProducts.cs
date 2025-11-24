using Carter;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[UsedImplicitly]
public class GetLowStockProducts : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/low-stock", Handle)
            .WithName("GetLowStockProducts")
            .WithSummary("Get all products with stock below reorder level")
            .WithTags("Products");
    }

    private static async Task<Ok<Response>> Handle(InventoryDbContext db, CancellationToken ct)
    {
        var products = await db.Products
            .Where(p => p.QuantityInStock <= p.ReorderLevel)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Sku,
                p.QuantityInStock,
                p.ReorderLevel,
                p.Category,
                p.SupplierId,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(ct);

        return TypedResults.Ok(new Response(products));
    }

    public sealed record Response(IEnumerable<ProductDto> Products);

    public sealed record ProductDto(
        string Id,
        string Name,
        string? Description,
        string Sku,
        int QuantityInStock,
        int ReorderLevel,
        string? Category,
        string? SupplierId,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}

using Carter;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetBySku;

[UsedImplicitly]
public class GetProductBySku : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/sku/{sku}", Handle)
            .WithName("GetProductBySku")
            .WithSummary("Get a specific product by SKU")
            .WithTags("Products")
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<Response>, NotFound>> Handle(string sku, InventoryDbContext db,
        CancellationToken ct)
    {
        Product? product = await db.Products
            .FirstOrDefaultAsync(p => p.Sku == sku, ct);

        if (product is null)
        {
            return TypedResults.NotFound();
        }

        var response = new Response(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.QuantityInStock,
            product.ReorderLevel,
            product.Category,
            product.SupplierId,
            product.CreatedAt,
            product.UpdatedAt);

        return TypedResults.Ok(response);
    }

    public sealed record Response(
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

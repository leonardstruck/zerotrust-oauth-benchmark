using Carter;

using Microsoft.AspNetCore.Http.HttpResults;

using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[UsedImplicitly]
public class GetProductById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", Handle)
            .WithName("GetProductById")
            .WithSummary("Get a specific product by ID")
            .WithTags("Products")
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<Response>, NotFound>> Handle(string id, InventoryDbContext db,
        CancellationToken ct)
    {
        Product? product = await db.Products.FindAsync([id], ct);

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

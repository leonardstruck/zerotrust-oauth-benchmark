using Carter;

using Microsoft.AspNetCore.Http.HttpResults;

using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Features.Products.AdjustStock;

[UsedImplicitly]
public class AdjustStock : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/products/{id}/stock",
                Handle)
            .WithName("AdjustStock")
            .WithSummary("Adjust the stock quantity for a product")
            .WithTags("Products")
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<Response>, NotFound>> Handle(string id, Command command, InventoryDbContext db,
        CancellationToken ct)
    {
        Product? product = await db.Products.FindAsync([id], ct);

        if (product is null)
        {
            return TypedResults.NotFound();
        }

        product.QuantityInStock += command.Quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

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

    public sealed record Command(int Quantity, string? Reason);

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

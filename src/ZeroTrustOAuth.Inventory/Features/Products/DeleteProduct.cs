using Carter;

using Microsoft.AspNetCore.Http.HttpResults;

using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[UsedImplicitly]
public class DeleteProduct : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id}", Handle)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product from inventory")
            .WithTags("Products")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, NotFound>> Handle(string id, InventoryDbContext db,
        CancellationToken ct)
    {
        Product? product = await db.Products.FindAsync([id], ct);

        if (product is null)
        {
            return TypedResults.NotFound();
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }
}
using Carter;

using ErrorOr;

using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain;
using ZeroTrustOAuth.ServiceDefaults;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[UsedImplicitly]
public class AdjustStock : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/products/{id}/stock",
                Handle)
            .WithName("AdjustStock")
            .WithSummary("Adjust the stock quantity for a product")
            .WithTags("Products");
    }

    private static async Task<IResult> Handle(string id, Command command, InventoryDbContext db,
        CancellationToken ct)
    {
        Product? product = await db.Products.FindAsync([id], ct);

        if (product is null)
        {
            return Results.NotFound();
        }

        ErrorOr<Success> adjustResult = product.AdjustStock(command.Quantity);

        if (adjustResult.IsError)
        {
            return adjustResult.ToProblem();
        }

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

        return Results.Ok(response);
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

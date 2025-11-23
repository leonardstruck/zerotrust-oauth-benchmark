using Carter;

using Microsoft.AspNetCore.Http.HttpResults;

using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Features.Products.Update;

[UsedImplicitly]
public class UpdateProduct : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id}", Handle)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product")
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

        // Update only provided fields
        if (command.Name is not null) product.Name = command.Name;
        if (command.Description is not null) product.Description = command.Description;
        if (command.Sku is not null) product.Sku = command.Sku;
        if (command.QuantityInStock.HasValue) product.QuantityInStock = command.QuantityInStock.Value;
        if (command.ReorderLevel.HasValue) product.ReorderLevel = command.ReorderLevel.Value;
        if (command.Category is not null) product.Category = command.Category;
        if (command.SupplierId is not null) product.SupplierId = command.SupplierId;

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

    public sealed record Command(
        string? Name,
        string? Description,
        string? Sku,
        int? QuantityInStock,
        int? ReorderLevel,
        string? Category,
        string? SupplierId);

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

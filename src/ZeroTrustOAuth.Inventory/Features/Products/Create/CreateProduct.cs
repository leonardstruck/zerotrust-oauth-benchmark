using Carter;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Features.Products.Create;

[UsedImplicitly]
public class CreateProduct : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", Handle)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithTags("Products")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<Response>, BadRequest<string>>> Handle(Command command,
        InventoryDbContext db, CancellationToken ct)
    {
        // Check if SKU already exists
        bool skuExists = await db.Products
            .AnyAsync(p => p.Sku == command.Sku, ct);

        if (skuExists)
        {
            return TypedResults.BadRequest($"A product with SKU '{command.Sku}' already exists.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = command.Name,
            Description = command.Description,
            Sku = command.Sku,
            QuantityInStock = command.QuantityInStock,
            ReorderLevel = command.ReorderLevel,
            Category = command.Category,
            SupplierId = command.SupplierId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
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

        return TypedResults.Created($"/api/inventory/products/{product.Id}", response);
    }

    public sealed record Command(
        string Name,
        string? Description,
        string Sku,
        int QuantityInStock,
        int ReorderLevel,
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

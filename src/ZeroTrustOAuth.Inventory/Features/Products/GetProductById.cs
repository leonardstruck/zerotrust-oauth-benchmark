using Carter;

using Facet.Extensions;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

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

    private static async Task<IResult> Handle(
        string id,
        HttpContext context,
        InventoryDbContext db,
        CancellationToken ct)
    {
        ProductDto? product = await db.Products
            .Where(p => p.Id == id)
            .SelectFacet<ProductDto>()
            .FirstOrDefaultAsync(ct);

        return product is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(product);
    }
}
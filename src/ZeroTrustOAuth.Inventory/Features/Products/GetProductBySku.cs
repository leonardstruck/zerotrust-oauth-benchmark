using Carter;

using Facet.Extensions;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products;

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

    private static async Task<IResult> Handle(
        string sku,
        HttpContext context,
        InventoryDbContext db,
        CancellationToken ct)
    {
        ProductDto? product = await db.Products
            .Where(p => p.Sku == sku)
            .SelectFacet<ProductDto>()
            .FirstOrDefaultAsync(ct);

        return product is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(product);
    }
}
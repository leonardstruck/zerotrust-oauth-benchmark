using Carter;

using Facet.Extensions;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[UsedImplicitly]
public class GetProductsByCategory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/category/{category}", Handle)
            .WithName("GetProductsByCategory")
            .WithSummary("Get all products in a specific category")
            .WithTags("Products");
    }

    private static async Task<IResult> Handle(
        string category,
        HttpContext context,
        InventoryDbContext db,
        CancellationToken ct)
    {
        List<ProductDto> products = await db.Products
            .Where(p => p.Category == category)
            .SelectFacet<ProductDto>()
            .ToListAsync(ct);

        return TypedResults.Ok(new Response<ProductDto>(products));
    }

    public sealed record Response<T>(IEnumerable<T> Products);
}
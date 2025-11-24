using Carter;

using Facet.Extensions;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[UsedImplicitly]
public class GetAllProducts : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", Handle)
            .WithName("GetAllProducts")
            .WithSummary("Get all products in inventory")
            .WithTags("Products");
    }

    private static async Task<IResult> Handle(HttpContext context, InventoryDbContext db, CancellationToken ct)
    {
        List<ProductDto> products = await db.Products
            .SelectFacet<ProductDto>()
            .ToListAsync(ct);

        return TypedResults.Ok(new Response<ProductDto>(products));
    }

    public sealed record Response<T>(IEnumerable<T> Products);
}
using Carter;

using Facet.Extensions;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[UsedImplicitly]
public class GetLowStockProducts : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/low-stock", Handle)
            .WithName("GetLowStockProducts")
            .WithSummary("Get all products with stock below reorder level (Admin only)")
            .WithTags("Products");
    }

    private static async Task<Ok<Response>> Handle(InventoryDbContext db, CancellationToken ct)
    {
        List<ProductAdminDto> products = await db.Products
            .Where(p => p.QuantityInStock <= p.ReorderLevel)
            .SelectFacet<ProductAdminDto>()
            .ToListAsync(ct);

        return TypedResults.Ok(new Response(products));
    }

    public sealed record Response(IEnumerable<ProductAdminDto> Products);
}
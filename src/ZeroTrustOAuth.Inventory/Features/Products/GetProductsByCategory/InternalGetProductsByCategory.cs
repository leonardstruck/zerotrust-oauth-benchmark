using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductsByCategory;

public class InternalGetProductsByCategory(InventoryDbContext dbContext)
    : Endpoint<InternalGetProductsByCategoryRequest, InternalGetProductsByCategoryResponse>
{
    public override void Configure()
    {
        Get("/category/{category}");
        Group<InternalProductsGroup>();
    }

    public override async Task HandleAsync(InternalGetProductsByCategoryRequest req, CancellationToken ct)
    {
        List<InternalProductDto> products = await dbContext.Products
            .Where(p => p.Category == req.Category)
            .SelectFacet<InternalProductDto>()
            .ToListAsync(ct);

        await Send.OkAsync(new InternalGetProductsByCategoryResponse(products), ct);
    }
}

[PublicAPI]
public record InternalGetProductsByCategoryResponse(IEnumerable<InternalProductDto> Products);

[PublicAPI]
public record InternalGetProductsByCategoryRequest(string Category);
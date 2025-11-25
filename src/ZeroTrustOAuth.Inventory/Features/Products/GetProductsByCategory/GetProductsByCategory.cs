using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductsByCategory;

public class GetProductsByCategory(InventoryDbContext dbContext)
    : Endpoint<GetProductsByCategoryRequest, GetProductsByCategoryResponse>
{
    public override void Configure()
    {
        Get("/category/{category}");
        Group<ProductsGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetProductsByCategoryRequest req, CancellationToken ct)
    {
        List<ProductDto> products = await dbContext.Products
            .Where(p => p.Category == req.Category)
            .SelectFacet<ProductDto>()
            .ToListAsync(ct);

        await Send.OkAsync(new GetProductsByCategoryResponse(products), ct);
    }
}

[PublicAPI]
public record GetProductsByCategoryResponse(IEnumerable<ProductDto> Products);

[PublicAPI]
public record GetProductsByCategoryRequest(string Category);
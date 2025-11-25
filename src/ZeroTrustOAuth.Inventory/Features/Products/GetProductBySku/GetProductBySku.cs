using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductBySku;

public class GetProductBySku(InventoryDbContext dbContext) : Endpoint<GetProductBySkuRequest, GetProductBySkuResponse>
{
    public override void Configure()
    {
        Get("/sku/{sku}");
        Group<ProductsGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetProductBySkuRequest req, CancellationToken ct)
    {
        ProductDto? product = await dbContext.Products
            .Where(p => p.Sku == req.Sku)
            .SelectFacet<ProductDto>()
            .FirstOrDefaultAsync(ct);

        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new GetProductBySkuResponse(product), ct);
    }
}

[PublicAPI]
public record GetProductBySkuResponse(ProductDto Product);

[PublicAPI]
public record GetProductBySkuRequest(string Sku);
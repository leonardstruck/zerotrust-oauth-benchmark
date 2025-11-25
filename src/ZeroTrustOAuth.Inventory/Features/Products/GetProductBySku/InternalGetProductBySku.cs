using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductBySku;

public class InternalGetProductBySku(InventoryDbContext dbContext)
    : Endpoint<InternalGetProductBySkuRequest, InternalGetProductBySkuResponse>
{
    public override void Configure()
    {
        Get("/sku/{sku}");
        Group<InternalProductsGroup>();
    }

    public override async Task HandleAsync(InternalGetProductBySkuRequest req, CancellationToken ct)
    {
        InternalProductDto? product = await dbContext.Products
            .Where(p => p.Sku == req.Sku)
            .SelectFacet<InternalProductDto>()
            .FirstOrDefaultAsync(ct);

        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new InternalGetProductBySkuResponse(product), ct);
    }
}

[PublicAPI]
public record InternalGetProductBySkuResponse(InternalProductDto Product);

[PublicAPI]
public record InternalGetProductBySkuRequest(string Sku);
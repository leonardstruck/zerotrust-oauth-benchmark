using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductBySku;

public class InternalGetProductBySku(InventoryDbContext dbContext)
    : Endpoint<InternalGetProductBySkuRequest, InternalProductDto>
{
    public override void Configure()
    {
        Get("/sku/{sku}");
        Group<InternalProductsGroup>();
        Summary(s =>
        {
            s.Summary = "Get product by SKU (internal)";
            s.Description = "Retrieves a single product by its Stock Keeping Unit (SKU) with full internal details";
            s.Responses[200] = "Successfully retrieved the product with internal details";
            s.Responses[404] = "Product not found";
        });
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

        await Send.OkAsync(product, ct);
    }
}

[PublicAPI]
public record InternalGetProductBySkuRequest(string Sku);
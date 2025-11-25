using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductBySku;

public class GetProductBySku(InventoryDbContext dbContext) : Endpoint<GetProductBySkuRequest, ProductDto>
{
    public override void Configure()
    {
        Get("/sku/{sku}");
        Group<ProductsGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get product by SKU";
            s.Description = "Retrieves a single product by its Stock Keeping Unit (SKU) with public information";
            s.Responses[200] = "Successfully retrieved the product";
            s.Responses[404] = "Product not found";
        });
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

        await Send.OkAsync(product, ct);
    }
}

[PublicAPI]
public record GetProductBySkuRequest(string Sku);
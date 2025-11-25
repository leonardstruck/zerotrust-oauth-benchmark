using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetLowStockProducts;

public class GetLowStockProducts(InventoryDbContext dbContext)
    : Endpoint<GetLowStockProductsRequest, GetLowStockProductsResponse>
{
    public override void Configure()
    {
        Get("/low-stock");
        Group<InternalProductsGroup>();
    }

    public override async Task HandleAsync(GetLowStockProductsRequest req, CancellationToken ct)
    {
        List<InternalProductDto> products = await dbContext.Products
            .Where(p => p.QuantityInStock <= p.ReorderLevel)
            .SelectFacet<InternalProductDto>()
            .ToListAsync(ct);

        await Send.OkAsync(new GetLowStockProductsResponse(products), ct);
    }
}

[PublicAPI]
public record GetLowStockProductsRequest;

[PublicAPI]
public record GetLowStockProductsResponse(IEnumerable<InternalProductDto> Products);
using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetLowStockProducts;

public class GetLowStockProducts(InventoryDbContext dbContext)
    : EndpointWithoutRequest<GetLowStockProductsResponse>
{
    public override void Configure()
    {
        Get("/low-stock");
        Group<InternalProductsGroup>();
        Summary(s =>
        {
            s.Summary = "Get low stock products";
            s.Description = "Retrieves all products where quantity in stock is at or below the reorder level";
            s.Responses[200] = "Successfully retrieved low stock products";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<InternalProductDto> products = await dbContext.Products
            .Where(p => p.QuantityInStock <= p.ReorderLevel)
            .SelectFacet<InternalProductDto>()
            .ToListAsync(ct);

        await Send.OkAsync(new GetLowStockProductsResponse(products), ct);
    }
}

[PublicAPI]
public record GetLowStockProductsResponse(IEnumerable<InternalProductDto> Products);
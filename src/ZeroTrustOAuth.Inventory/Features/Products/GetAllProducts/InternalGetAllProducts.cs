using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetAllProducts;

public class InternalGetAllProducts(InventoryDbContext dbContext)
    : EndpointWithoutRequest<InternalGetAllProductsResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<InternalProductsGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<InternalProductDto> products = await dbContext.Products.SelectFacet<InternalProductDto>().ToListAsync(ct);
        await Send.OkAsync(new InternalGetAllProductsResponse(products), ct);
    }
}

[PublicAPI]
public record InternalGetAllProductsResponse(IEnumerable<InternalProductDto> Products);
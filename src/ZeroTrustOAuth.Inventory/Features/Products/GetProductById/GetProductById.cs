using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductById;

public class GetProductById(InventoryDbContext dbContext) : Endpoint<GetProductByIdRequest, GetProductByIdResponse>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<ProductsGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        ProductDto? product = await dbContext.Products
            .Where(p => p.Id == req.Id)
            .SelectFacet<ProductDto>()
            .FirstOrDefaultAsync(ct);

        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new GetProductByIdResponse(product), ct);
    }
}

[PublicAPI]
public record GetProductByIdResponse(ProductDto Product);

[PublicAPI]
public record GetProductByIdRequest(string Id);
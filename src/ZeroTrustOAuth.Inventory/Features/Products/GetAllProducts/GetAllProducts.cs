using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetAllProducts;

public class GetAllProducts(InventoryDbContext dbContext) : EndpointWithoutRequest<GetAllProductsResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<ProductsGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all products";
            s.Description = "Retrieves a list of all products with public information";
            s.Responses[200] = "Successfully retrieved all products";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<ProductDto> products = await dbContext.Products.SelectFacet<ProductDto>().ToListAsync(ct);
        await Send.OkAsync(new GetAllProductsResponse(products), ct);
    }
}

[PublicAPI]
public record GetAllProductsResponse(IEnumerable<ProductDto> Products);
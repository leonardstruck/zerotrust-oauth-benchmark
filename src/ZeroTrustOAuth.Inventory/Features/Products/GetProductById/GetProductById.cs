using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductById;

public class GetProductById(InventoryDbContext dbContext) : Endpoint<GetProductByIdRequest, ProductDto>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<ProductsGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get product by ID";
            s.Description = "Retrieves a single product by its unique identifier with public information";
            s.Responses[200] = "Successfully retrieved the product";
            s.Responses[404] = "Product not found";
        });
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

        await Send.OkAsync(product, ct);
    }
}

[PublicAPI]
public record GetProductByIdRequest(string Id);
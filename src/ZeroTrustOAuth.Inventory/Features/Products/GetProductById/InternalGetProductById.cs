// filepath: /Users/leonardstruck/Coding/zerotrust-oauth-benchmark/src/ZeroTrustOAuth.Inventory/Features/Products/GetProductById/InternalGetProductById.cs

using Facet.Extensions;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductById;

public class InternalGetProductById(InventoryDbContext dbContext)
    : EndpointWithoutRequest<InternalGetProductByIdResponse>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<InternalProductsGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? id = Route<string>("id");
        if (string.IsNullOrWhiteSpace(id))
        {
            AddError("id", "The 'id' route parameter is required.");
            ThrowIfAnyErrors();
            return;
        }

        InternalProductDto? product = await dbContext.Products
            .Where(p => p.Id == id)
            .SelectFacet<InternalProductDto>()
            .FirstOrDefaultAsync(ct);

        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new InternalGetProductByIdResponse(product), ct);
    }
}

[PublicAPI]
public record InternalGetProductByIdResponse(InternalProductDto Product);
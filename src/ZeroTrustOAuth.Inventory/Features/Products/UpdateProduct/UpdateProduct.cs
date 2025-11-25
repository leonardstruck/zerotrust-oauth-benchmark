// filepath: /Users/leonardstruck/Coding/zerotrust-oauth-benchmark/src/ZeroTrustOAuth.Inventory/Features/Products/UpdateProduct/UpdateProduct.cs

using ErrorOr;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Data.Extensions;
using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain.Products;

namespace ZeroTrustOAuth.Inventory.Features.Products.UpdateProduct;

public class UpdateProduct(InventoryDbContext dbContext)
    : Endpoint<UpdateProductRequest, UpdateProductResponse>
{
    public override void Configure()
    {
        Patch("/{id}");
        Group<InternalProductsGroup>();
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        Product? product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == req.Id, ct);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        ErrorOr<Success> updated = product.Update(
            req.Name,
            req.Sku,
            req.ReorderLevel,
            req.Description,
            req.Category,
            req.SupplierId);

        if (updated.IsError)
        {
            foreach (Error err in updated.Errors)
            {
                AddError(err.Code, err.Description);
            }

            ThrowIfAnyErrors();
            return;
        }

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("sku"))
        {
            AddError("Product.DuplicateSku", $"A product with sku '{req.Sku}' already exists.");
            ThrowIfAnyErrors();
            return;
        }

        await Send.OkAsync(new UpdateProductResponse(req.Id), ct);
    }
}

[PublicAPI]
public sealed record UpdateProductRequest(
    string Id,
    string? Name,
    string? Sku,
    int? ReorderLevel,
    string? Description,
    string? Category,
    string? SupplierId
);

[PublicAPI]
public sealed record UpdateProductResponse(string Id);
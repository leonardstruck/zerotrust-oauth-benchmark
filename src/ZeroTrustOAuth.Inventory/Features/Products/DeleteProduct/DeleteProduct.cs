using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain.Products;

namespace ZeroTrustOAuth.Inventory.Features.Products.DeleteProduct;

public class DeleteProduct(InventoryDbContext dbContext) : Endpoint<DeleteProductRequest>
{
    public override void Configure()
    {
        Delete("/{id}");
        Group<InternalProductsGroup>();
        Summary(s =>
        {
            s.Summary = "Delete a product";
            s.Description = "Permanently deletes a product from the inventory system";
            s.Responses[204] = "Product successfully deleted";
            s.Responses[404] = "Product not found";
        });
    }

    public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
    {
        Product? product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == req.Id, ct);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

[PublicAPI]
public record DeleteProductRequest(string Id);
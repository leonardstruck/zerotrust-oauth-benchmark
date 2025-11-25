using ErrorOr;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Data.Extensions;
using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain.Products;

namespace ZeroTrustOAuth.Inventory.Features.Products.CreateProduct;

public class CreateProduct(InventoryDbContext dbContext) : Endpoint<CreateProductRequest, CreateProductResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<InternalProductsGroup>();
        Summary(s =>
        {
            s.Summary = "Create a new product";
            s.Description = "Creates a new product in the inventory system with the specified details";
            s.Responses[201] = "Product successfully created";
            s.Responses[400] = "Invalid request or validation error";
            s.Responses[409] = "Product with the same SKU already exists";
        });
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        ErrorOr<Product> created = Product.Create(
            req.Name,
            req.Sku,
            req.QuantityInStock,
            req.ReorderLevel,
            req.Description,
            req.Category,
            req.SupplierId);

        if (created.IsError)
        {
            foreach (Error err in created.Errors)
            {
                AddError(err.Code, err.Description);
            }

            ThrowIfAnyErrors();
            return;
        }

        Product product = created.Value;
        dbContext.Products.Add(product);

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

        await Send.CreatedAtAsync($"/internal/products/{product.Id}", new CreateProductResponse(product.Id),
            cancellation: ct);
    }
}

[PublicAPI]
public sealed record CreateProductRequest(
    string Name,
    string Sku,
    int QuantityInStock,
    int ReorderLevel,
    string? Description,
    string? Category,
    string? SupplierId
);

[PublicAPI]
public sealed record CreateProductResponse(string Id);
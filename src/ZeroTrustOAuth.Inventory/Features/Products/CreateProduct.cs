using Carter;

using ErrorOr;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain;
using ZeroTrustOAuth.ServiceDefaults;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[UsedImplicitly]
public class CreateProduct : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost<Command>("/products", Handle)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithTags("Products");
    }

    private static async Task<IResult> Handle(Command command,
        InventoryDbContext db,
        CancellationToken ct)
    {
        bool skuExists = await db.Products
            .AnyAsync(p => p.Sku == command.Sku, ct);

        if (skuExists)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Sku"] = [$"A product with SKU '{command.Sku}' already exists."]
            });
        }

        ErrorOr<Product> productResult = Product.Create(
            command.Name,
            command.Sku,
            command.QuantityInStock,
            command.ReorderLevel,
            command.Description,
            command.Category,
            command.SupplierId);

        if (productResult.IsError)
        {
            return productResult.ToProblem();
        }

        var product = productResult.Value;
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        var response = new Response(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.QuantityInStock,
            product.ReorderLevel,
            product.Category,
            product.SupplierId,
            product.CreatedAt,
            product.UpdatedAt);

        return Results.Created($"/api/inventory/products/{product.Id}", response);
    }

    public sealed record Command(
        string Name,
        string? Description,
        string Sku,
        int QuantityInStock,
        int ReorderLevel,
        string? Category,
        string? SupplierId);

    public sealed record Response(
        string Id,
        string Name,
        string? Description,
        string Sku,
        int QuantityInStock,
        int ReorderLevel,
        string? Category,
        string? SupplierId,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    [UsedImplicitly]
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU is required.")
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

            RuleFor(x => x.QuantityInStock)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity in stock cannot be negative.");

            RuleFor(x => x.ReorderLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Reorder level cannot be negative.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
                .When(x => x.Description is not null);

            RuleFor(x => x.Category)
                .MaximumLength(100).WithMessage("Category must not exceed 100 characters.")
                .When(x => x.Category is not null);

            RuleFor(x => x.SupplierId)
                .MaximumLength(50).WithMessage("Supplier ID must not exceed 50 characters.")
                .When(x => x.SupplierId is not null);
        }
    }
}

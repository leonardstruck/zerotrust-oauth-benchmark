using Carter;

using ErrorOr;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Data.Extensions;
using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Domain;
using ZeroTrustOAuth.ServiceDefaults;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[UsedImplicitly]
public class UpdateProduct : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch<Command>("/products/{id}", Handle)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product")
            .WithTags("Products");
    }

    private static async Task<IResult> Handle(string id,
        Command command,
        InventoryDbContext db,
        CancellationToken ct)
    {
        Product? product = await db.Products.FindAsync([id], ct);

        if (product is null)
        {
            return Results.NotFound();
        }

        ErrorOr<Success> updateResult = product.Update(
            command.Name,
            command.Sku,
            command.ReorderLevel,
            command.Description,
            command.Category,
            command.SupplierId);

        if (updateResult.IsError)
        {
            return updateResult.ToProblem();
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("Sku"))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Sku"] = [$"A product with SKU '{command.Sku}' already exists."]
            });
        }

        Response response = new(
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

        return Results.Ok(response);
    }

    public sealed record Command(
        string? Name,
        string? Description,
        string? Sku,
        int? ReorderLevel,
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
                .NotEmpty().WithMessage("Product name cannot be empty if provided.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU cannot be empty if provided.")
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.")
                .When(x => x.Sku is not null);

            RuleFor(x => x.ReorderLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Reorder level cannot be negative.")
                .When(x => x.ReorderLevel.HasValue);

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
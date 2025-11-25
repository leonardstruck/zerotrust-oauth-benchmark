using FastEndpoints;

using FluentValidation;

namespace ZeroTrustOAuth.Inventory.Features.Products.CreateProduct;

public class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.QuantityInStock)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Category)
            .MaximumLength(100);

        RuleFor(x => x.SupplierId)
            .MaximumLength(50);
    }
}
using FastEndpoints;

using FluentValidation;

namespace ZeroTrustOAuth.Inventory.Features.Products.UpdateProduct;

public class UpdateProductRequestValidator : Validator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .MaximumLength(200);

        RuleFor(x => x.Sku)
            .MaximumLength(50);

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
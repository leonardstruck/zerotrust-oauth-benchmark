using FastEndpoints;

using FluentValidation;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductsByCategory;

public class InternalGetProductsByCategoryValidator : Validator<InternalGetProductsByCategoryRequest>
{
    public InternalGetProductsByCategoryValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(100);
    }
}
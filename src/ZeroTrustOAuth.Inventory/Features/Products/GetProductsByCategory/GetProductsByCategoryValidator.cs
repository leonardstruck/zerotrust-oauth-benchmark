using FastEndpoints;

using FluentValidation;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductsByCategory;

public class GetProductsByCategoryValidator : Validator<GetProductsByCategoryRequest>
{
    public GetProductsByCategoryValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(100);
    }
}
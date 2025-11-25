using FastEndpoints;

using FluentValidation;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductBySku;

public class InternalGetProductBySkuValidator : Validator<InternalGetProductBySkuRequest>
{
    public InternalGetProductBySkuValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);
    }
}
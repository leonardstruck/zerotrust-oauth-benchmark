using FastEndpoints;

using FluentValidation;

namespace ZeroTrustOAuth.Inventory.Features.Products.GetProductBySku;

public class GetProductBySkuValidator : Validator<GetProductBySkuRequest>
{
    public GetProductBySkuValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);
    }
}
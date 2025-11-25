using FastEndpoints;

namespace ZeroTrustOAuth.Inventory.Features.Products;

public sealed class ProductsGroup : Group
{
    public ProductsGroup()
    {
        Configure("products", ep =>
        {
            ep.Description(x => x.WithTags("products"));
        });
    }
}

public sealed class InternalProductsGroup : SubGroup<ProductsGroup>
{
    public InternalProductsGroup()
    {
        Configure("internal", ep =>
        {
            ep.Description(x => x.WithTags("internal"));
        });
    }
}
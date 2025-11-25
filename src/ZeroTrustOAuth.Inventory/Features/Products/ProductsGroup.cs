using FastEndpoints;

namespace ZeroTrustOAuth.Inventory.Features.Products;

public sealed class ProductsGroup : Group
{
    public ProductsGroup()
    {
        Configure("products", ep =>
        {
            ep.Tags("Products");
        });
    }
}

public sealed class InternalProductsGroup : SubGroup<InternalGroup>
{
    public InternalProductsGroup()
    {
        Configure("products", ep =>
        {
            ep.Tags("Products");
        });
    }
}
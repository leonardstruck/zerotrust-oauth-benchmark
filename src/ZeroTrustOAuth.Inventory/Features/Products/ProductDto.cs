using Facet;

using ZeroTrustOAuth.Inventory.Domain.Products;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[Facet(typeof(Product),
    Include =
    [
        nameof(Product.Id), nameof(Product.Name), nameof(Product.Description), nameof(Product.Category),
        nameof(Product.Sku)
    ])
]
public partial record ProductDto;

[Facet(typeof(Product))]
public partial record InternalProductDto;
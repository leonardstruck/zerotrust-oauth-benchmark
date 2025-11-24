using Facet;

using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Features.Products;

[Wrapper(typeof(Product), "SupplierId")]
public partial record ProductDto;

[Wrapper(typeof(Product))]
public partial record ProductAdminDto;
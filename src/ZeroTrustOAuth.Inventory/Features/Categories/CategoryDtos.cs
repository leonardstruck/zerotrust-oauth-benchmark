using Facet;

using ZeroTrustOAuth.Inventory.Domain.Categories;

namespace ZeroTrustOAuth.Inventory.Features.Categories;

[Facet(typeof(Category), Include = [nameof(Category.Id), nameof(Category.Name)])]
public partial record CategorySummaryDto;

[Facet(typeof(Category), Include =
[
    nameof(Category.Id),
    nameof(Category.Name),
    nameof(Category.Description),
    nameof(Category.IsActive),
    nameof(Category.CreatedAt),
    nameof(Category.UpdatedAt)
])]
public partial record CategoryDetailsDto;

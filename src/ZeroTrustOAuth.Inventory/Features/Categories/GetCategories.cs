using Facet;
using Facet.Extensions;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Domain.Categories;
using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

namespace ZeroTrustOAuth.Inventory.Features.Categories;

[Facet(typeof(Category), Include = [nameof(Category.Id), nameof(Category.Name)])]
public partial record CategorySummaryDto;

public record GetCategoriesResponse(IEnumerable<CategorySummaryDto> Categories);

public class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories", Handler).WithTags("Categories");
    }

    private static async Task<Ok<GetCategoriesResponse>> Handler([FromServices] InventoryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<CategorySummaryDto> categories =
            await dbContext.Categories.SelectFacet<CategorySummaryDto>()
                .ToListAsync(cancellationToken);


        return TypedResults.Ok(new GetCategoriesResponse(categories));
    }
}
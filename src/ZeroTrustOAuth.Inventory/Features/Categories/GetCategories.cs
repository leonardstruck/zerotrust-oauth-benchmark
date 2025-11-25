using Facet.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

namespace ZeroTrustOAuth.Inventory.Features.Categories;

public record GetCategoriesResponse(IEnumerable<CategorySummaryDto> Categories);

public class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories", Handler)
            .WithTags("Categories")
            .Produces<GetCategoriesResponse>(StatusCodes.Status200OK);
    }

    private static async Task<Ok<GetCategoriesResponse>> Handler([FromServices] InventoryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<CategorySummaryDto> categories =
            await dbContext.Categories.AsNoTracking()
                .SelectFacet<CategorySummaryDto>()
                .ToListAsync(cancellationToken);


        return TypedResults.Ok(new GetCategoriesResponse(categories));
    }
}

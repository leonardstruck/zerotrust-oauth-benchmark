using Facet.Extensions;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

namespace ZeroTrustOAuth.Inventory.Features.Categories;

public class GetCategoryByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories/{id:guid}", Handler)
            .WithName("GetCategoryById")
            .WithSummary("Get a category by ID")
            .WithDescription("Retrieves detailed information about a specific category by its unique identifier.")
            .WithTags("Categories")
            .Produces<CategoryDetailsDto>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<CategoryDetailsDto>, NotFound>> Handler(
        [FromRoute] Guid id,
        [FromServices] InventoryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        CategoryDetailsDto? category = await dbContext.Categories
            .Where(category => category.Id == id)
            .SelectFacet<CategoryDetailsDto>()
            .SingleOrDefaultAsync(cancellationToken);

        return category is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(category);
    }
}
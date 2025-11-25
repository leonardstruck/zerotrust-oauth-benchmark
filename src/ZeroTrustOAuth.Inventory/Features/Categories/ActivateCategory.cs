using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ZeroTrustOAuth.Inventory.Domain.Categories;
using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

namespace ZeroTrustOAuth.Inventory.Features.Categories;

public class ActivateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("categories/{id:guid}/activate", Handler)
            .WithName("ActivateCategory")
            .WithSummary("Activate a category")
            .WithDescription("Activates a previously deactivated category, making it available for use in the inventory system.")
            .WithTags("Categories")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid id,
        [FromServices] InventoryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Category? category = await dbContext.Categories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return TypedResults.NotFound();
        }

        Ardalis.Result.Result activateResult = category.Activate();
        if (!activateResult.IsSuccess)
        {
            return activateResult.ToMinimalApiResult();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}

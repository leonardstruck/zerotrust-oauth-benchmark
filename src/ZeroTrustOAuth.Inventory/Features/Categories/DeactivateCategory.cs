using Ardalis.Result;
using Ardalis.Result.AspNetCore;

using Microsoft.AspNetCore.Mvc;

using ZeroTrustOAuth.Auth;
using ZeroTrustOAuth.Inventory.Domain.Categories;
using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

using IResult = Microsoft.AspNetCore.Http.IResult;

namespace ZeroTrustOAuth.Inventory.Features.Categories;

public class DeactivateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("categories/{id:guid}/deactivate", Handler)
            .WithName("DeactivateCategory")
            .WithSummary("Deactivate a category")
            .WithDescription(
                "Deactivates a category, making it unavailable for use in the inventory system while preserving its data.")
                .WithTags("Categories")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .RequireAuthorization(ScopePolicies.InventoryProductManage);
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

        Result deactivateResult = category.Deactivate();
        if (!deactivateResult.IsSuccess)
        {
            return deactivateResult.ToMinimalApiResult();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
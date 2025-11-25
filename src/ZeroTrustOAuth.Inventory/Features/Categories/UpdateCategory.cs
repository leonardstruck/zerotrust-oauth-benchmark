using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Ardalis.Result.FluentValidation;

using Facet.Extensions;
using Facet.Extensions.EFCore;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ZeroTrustOAuth.Inventory.Domain.Categories;
using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

using IResult = Microsoft.AspNetCore.Http.IResult;

namespace ZeroTrustOAuth.Inventory.Features.Categories;

public record UpdateCategoryRequest(string? Name, string? Description);

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(request => request)
            .Must(request => request.Name is not null || request.Description is not null)
            .WithMessage("At least one property must be provided.");

        When(request => request.Name is not null, () => RuleFor(request => request.Name).NotEmpty());
    }
}

public class UpdateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("categories/{id:guid}", Handler)
            .WithName("UpdateCategory")
            .WithSummary("Update a category")
            .WithDescription("Updates an existing category's name and/or description. At least one property must be provided.")
            .WithTags("Categories")
            .Produces<CategoryDetailsDto>(StatusCodes.Status200OK, contentType: "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid id,
        [FromBody] UpdateCategoryRequest request,
        [FromServices] InventoryDbContext dbContext,
        [FromServices] IValidator<UpdateCategoryRequest> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CategoryDetailsDto>.Invalid(validation.AsErrors()).ToMinimalApiResult();
        }

        Category? category = await dbContext.Categories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return Result<CategoryDetailsDto>.NotFound().ToMinimalApiResult();
        }

        Result updateResult = category.Update(request.Name, request.Description);
        if (!updateResult.IsSuccess)
        {
            return updateResult.ToMinimalApiResult();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        CategoryDetailsDto categoryDetails = await dbContext.Categories
            .Where(existingCategory => existingCategory.Id == category.Id)
            .SingleFacetAsync<CategoryDetailsDto>(cancellationToken);

        return TypedResults.Ok(categoryDetails);
    }
}

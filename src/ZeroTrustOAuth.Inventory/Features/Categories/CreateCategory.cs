using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Ardalis.Result.FluentValidation;

using Facet.Extensions;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Domain.Categories;
using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

using IResult = Microsoft.AspNetCore.Http.IResult;

namespace ZeroTrustOAuth.Inventory.Features.Categories;

public record CreateCategoryRequest(string Name, string? Description, bool IsActive = true);

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
    }
}

public class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("categories", Handler)
            .WithName("CreateCategory")
            .WithSummary("Create a new category")
            .WithDescription("Creates a new category in the inventory system with the provided name, description, and active status.")
            .WithTags("Categories")
            .Produces<CategoryDetailsDto>(StatusCodes.Status201Created, contentType: "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handler(
        [FromBody] CreateCategoryRequest request,
        [FromServices] InventoryDbContext dbContext,
        [FromServices] IValidator<CreateCategoryRequest> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CategoryDetailsDto>.Invalid(validation.AsErrors()).ToMinimalApiResult();
        }

        Result<Category> createResult =
            Category.Create(request.Name, request.Description, request.IsActive);

        if (!createResult.IsSuccess)
        {
            return createResult.ToMinimalApiResult();
        }

        Category category = createResult.Value!;

        await dbContext.Categories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        CategoryDetailsDto categoryDetails = await dbContext.Categories
            .Where(existingCategory => existingCategory.Id == category.Id)
            .SelectFacet<CategoryDetailsDto>()
            .SingleAsync(cancellationToken);

        return TypedResults.Created($"/categories/{categoryDetails.Id}", categoryDetails);
    }
}
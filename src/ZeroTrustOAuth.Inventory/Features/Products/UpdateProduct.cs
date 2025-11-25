using Ardalis.Result;
using Ardalis.Result.AspNetCore;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Domain.Products;
using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

using IResult = Microsoft.AspNetCore.Http.IResult;

namespace ZeroTrustOAuth.Inventory.Features.Products;

public record UpdateProductRequest(
    string? Sku,
    string? Name,
    decimal? Price,
    int? Stock,
    string? Description,
    Guid? CategoryId);

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(request => request)
            .Must(request =>
                request.Sku is not null || request.Name is not null || request.Price is not null ||
                request.Stock is not null || request.Description is not null || request.CategoryId is not null)
            .WithMessage("At least one property must be provided.");

        When(request => request.Sku is not null, () => RuleFor(request => request.Sku).NotEmpty());
        When(request => request.Name is not null, () => RuleFor(request => request.Name).NotEmpty());
        When(request => request.Price is not null, () => RuleFor(request => request.Price!.Value).GreaterThan(0));
        When(request => request.Stock is not null,
            () => RuleFor(request => request.Stock!.Value).GreaterThanOrEqualTo(0));
        When(request => request.CategoryId is not null,
            () => RuleFor(request => request.CategoryId).NotEqual(Guid.Empty));
    }
}

public class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/{id:guid}", Handler)
            .WithName("UpdateProduct")
            .WithSummary("Update a product")
            .WithDescription(
                "Updates an existing product's properties including SKU, name, price, stock, description, and category. At least one property must be provided.")
            .WithTags("Products")
            .Produces<ProductDetailsDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        [FromServices] InventoryDbContext dbContext,
        [FromServices] IValidator<UpdateProductRequest> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
        }

        Product? product = await dbContext.Products.FindAsync([id], cancellationToken);
        if (product is null)
        {
            return TypedResults.NotFound();
        }

        if (request.CategoryId is not null)
        {
            bool categoryExists = await dbContext.Categories
                .AnyAsync(category => category.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["CategoryId"] = ["Category not found."]
                });
            }
        }

        Result updateResult = product.Update(
            request.Sku,
            request.Name,
            request.Price,
            request.Stock,
            request.Description,
            request.CategoryId);

        if (!updateResult.IsSuccess)
        {
            return updateResult.ToMinimalApiResult();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        ProductDetailsDto productDetails = await dbContext.Products
            .Where(savedProduct => savedProduct.Id == product.Id)
            .Select(savedProduct => new ProductDetailsDto(
                savedProduct.Id,
                savedProduct.Sku,
                savedProduct.Name,
                savedProduct.Description,
                savedProduct.Price,
                savedProduct.Stock,
                savedProduct.CategoryId,
                savedProduct.Category != null ? savedProduct.Category.Name : null))
            .SingleAsync(cancellationToken);

        return TypedResults.Ok(productDetails);
    }
}
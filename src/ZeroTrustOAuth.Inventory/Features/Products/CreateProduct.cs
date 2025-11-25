using Ardalis.Result;
using Ardalis.Result.AspNetCore;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Auth;
using ZeroTrustOAuth.Inventory.Domain.Products;
using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

using IResult = Microsoft.AspNetCore.Http.IResult;

namespace ZeroTrustOAuth.Inventory.Features.Products;

public record CreateProductRequest(
    string Sku,
    string Name,
    decimal Price,
    int Stock,
    string? Description,
    Guid? CategoryId);

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(request => request.Sku).NotEmpty();
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Price).GreaterThan(0);
        RuleFor(request => request.Stock).GreaterThanOrEqualTo(0);

        When(request => request.CategoryId is not null,
            () => RuleFor(request => request.CategoryId).NotEqual(Guid.Empty));
    }
}

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products", Handler)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithDescription(
                "Creates a new product in the inventory system with SKU, name, price, stock quantity, optional description, and optional category assignment.")
                .WithTags("Products")
                .Produces<ProductDetailsDto>(StatusCodes.Status201Created, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .RequireAuthorization(ScopePolicies.InventoryProductManage);
    }

    private static async Task<IResult> Handler(
        [FromBody] CreateProductRequest request,
        [FromServices] InventoryDbContext dbContext,
        [FromServices] IValidator<CreateProductRequest> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
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

        Result<Product> createResult = Product.Create(
            request.Sku,
            request.Name,
            request.Price,
            request.Stock,
            request.Description,
            request.CategoryId);

        if (!createResult.IsSuccess)
        {
            return createResult.ToMinimalApiResult();
        }

        Product product = createResult.Value!;

        await dbContext.Products.AddAsync(product, cancellationToken);
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

        return TypedResults.Created($"/products/{productDetails.Id}", productDetails);
    }
}
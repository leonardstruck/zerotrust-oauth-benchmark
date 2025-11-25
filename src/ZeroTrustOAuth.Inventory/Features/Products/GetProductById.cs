using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

namespace ZeroTrustOAuth.Inventory.Features.Products;

public class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{id:guid}", Handler)
            .WithTags("Products")
            .Produces<ProductDetailsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ProductDetailsDto>, NotFound>> Handler(
        [FromRoute] Guid id,
        [FromServices] InventoryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ProductDetailsDto? product = await dbContext.Products
            .Where(product => product.Id == id)
            .Select(product => new ProductDetailsDto(
                product.Id,
                product.Sku,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.CategoryId,
                product.Category != null ? product.Category.Name : null))
            .SingleOrDefaultAsync(cancellationToken);

        return product is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(product);
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

namespace ZeroTrustOAuth.Inventory.Features.Products;

public record GetProductsResponse(IEnumerable<ProductSummaryDto> Products);

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products", Handler)
            .WithName("GetProducts")
            .WithSummary("Get all products")
            .WithDescription(
                "Retrieves a list of all products in the inventory system, including their SKU, name, price, stock, and associated category.")
            .WithTags("Products")
            .Produces<GetProductsResponse>(StatusCodes.Status200OK, "application/json");
    }

    private static async Task<Ok<GetProductsResponse>> Handler(
        [FromServices] InventoryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<ProductSummaryDto> products = await dbContext.Products
            .Select(product => new ProductSummaryDto(
                product.Id,
                product.Sku,
                product.Name,
                product.Price,
                product.Stock,
                product.CategoryId,
                product.Category != null ? product.Category.Name : null))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(new GetProductsResponse(products));
    }
}
namespace ZeroTrustOAuth.Inventory.Features.Products;

public record ProductSummaryDto(Guid Id, string Sku, string Name, decimal Price, int Stock, Guid? CategoryId,
    string? CategoryName);

public record ProductDetailsDto(Guid Id, string Sku, string Name, string? Description, decimal Price, int Stock,
    Guid? CategoryId, string? CategoryName);

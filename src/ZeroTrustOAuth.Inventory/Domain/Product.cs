using ErrorOr;

namespace ZeroTrustOAuth.Inventory.Domain;

/// <summary>
/// Represents a product in the inventory system.
/// </summary>
public class Product
{
#pragma warning disable CS8618
    private Product() { }
#pragma warning restore CS8618

    /// <summary>
    /// Gets the unique identifier for the product.
    /// </summary>
    public string Id { get; internal set; }

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the product description.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets the Stock Keeping Unit identifier.
    /// </summary>
    public string Sku { get; internal set; }

    /// <summary>
    /// Gets the current quantity in stock.
    /// </summary>
    public int QuantityInStock { get; internal set; }

    /// <summary>
    /// Gets the minimum stock level before reordering.
    /// </summary>
    public int ReorderLevel { get; internal set; }

    /// <summary>
    /// Gets the category of the product.
    /// </summary>
    public string? Category { get; internal set; }

    /// <summary>
    /// Gets the supplier identifier.
    /// </summary>
    public string? SupplierId { get; internal set; }

    /// <summary>
    /// Gets the date when the product was added to inventory.
    /// </summary>
    public DateTime CreatedAt { get; internal set; }

    /// <summary>
    /// Gets the date when the product was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; internal set; }

    public static class Errors
    {
        public static Error InsufficientStock(int requested, int available) => Error.Validation(
            "Product.InsufficientStock",
            $"Cannot reduce stock by {requested}. Only {available} units available.");

        public static Error NegativeStock => Error.Validation(
            "Product.NegativeStock",
            "Stock quantity cannot be negative.");

        public static Error NegativeReorderLevel => Error.Validation(
            "Product.NegativeReorderLevel",
            "Reorder level cannot be negative.");
    }

    /// <summary>
    /// Creates a new product with the specified details.
    /// </summary>
    public static ErrorOr<Product> Create(
        string name,
        string sku,
        int quantityInStock,
        int reorderLevel,
        string? description = null,
        string? category = null,
        string? supplierId = null)
    {
        if (quantityInStock < 0)
            return Errors.NegativeStock;

        if (reorderLevel < 0)
            return Errors.NegativeReorderLevel;

        var now = DateTime.UtcNow;
        return new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Sku = sku,
            QuantityInStock = quantityInStock,
            ReorderLevel = reorderLevel,
            Category = category,
            SupplierId = supplierId,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the product details.
    /// </summary>
    public ErrorOr<Success> Update(
        string? name = null,
        string? sku = null,
        int? reorderLevel = null,
        string? description = null,
        string? category = null,
        string? supplierId = null)
    {
        if (reorderLevel is < 0)
            return Errors.NegativeReorderLevel;

        if (name is not null) Name = name;
        if (sku is not null) Sku = sku;
        if (reorderLevel.HasValue) ReorderLevel = reorderLevel.Value;
        if (description is not null) Description = description;
        if (category is not null) Category = category;
        if (supplierId is not null) SupplierId = supplierId;

        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    /// Adjusts the stock quantity by the specified amount.
    /// </summary>
    public ErrorOr<Success> AdjustStock(int adjustment)
    {
        var newQuantity = QuantityInStock + adjustment;

        if (newQuantity < 0)
            return Errors.InsufficientStock(-adjustment, QuantityInStock);

        QuantityInStock = newQuantity;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }
}

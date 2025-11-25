using ErrorOr;

namespace ZeroTrustOAuth.Inventory.Domain.Products;

/// <summary>
///     Represents a product in the inventory system.
/// </summary>
public class Product
{
#pragma warning disable CS8618
    private Product() { }
#pragma warning restore CS8618

    /// <summary>
    ///     Gets the unique identifier for the product.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    ///     Gets the product name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     Gets the product description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    ///     Gets the Stock Keeping Unit identifier.
    /// </summary>
    public string Sku { get; private set; }

    /// <summary>
    ///     Gets the current quantity in stock.
    /// </summary>
    public int QuantityInStock { get; private set; }

    /// <summary>
    ///     Gets the minimum stock level before reordering.
    /// </summary>
    public int ReorderLevel { get; private set; }

    /// <summary>
    ///     Gets the category of the product.
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    ///     Gets the supplier identifier.
    /// </summary>
    public string? SupplierId { get; private set; }

    /// <summary>
    ///     Gets the date when the product was added to inventory.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Gets the date when the product was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }


    /// <summary>
    ///     Creates a new product with the specified details.
    /// </summary>
    public static ErrorOr<Product> Create(
        string name,
        string sku,
        int quantityInStock,
        int reorderLevel,
        string? description = null,
        string? category = null,
        string? supplierId = null,
        string? id = null)
    {
        if (quantityInStock < 0)
        {
            return Errors.NegativeStock;
        }

        if (reorderLevel < 0)
        {
            return Errors.NegativeReorderLevel;
        }

        DateTime now = DateTime.UtcNow;
        return new Product
        {
            Id = id ?? Guid.CreateVersion7().ToString(),
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
    ///     Updates the product details.
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
        {
            return Errors.NegativeReorderLevel;
        }

        Name = name ?? Name;
        Sku = sku ?? Sku;
        ReorderLevel = reorderLevel ?? ReorderLevel;
        Description = description ?? Description;
        Category = category ?? Category;
        SupplierId = supplierId ?? SupplierId;


        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    ///     Adjusts the stock quantity by the specified amount.
    /// </summary>
    public ErrorOr<Success> AdjustStock(int adjustment)
    {
        int newQuantity = QuantityInStock + adjustment;

        if (newQuantity < 0)
        {
            return Errors.InsufficientStock(-adjustment, QuantityInStock);
        }

        QuantityInStock = newQuantity;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }
}
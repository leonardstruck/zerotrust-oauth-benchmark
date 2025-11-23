namespace ZeroTrustOAuth.Inventory.Domain;

/// <summary>
/// Represents a product in the inventory system.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the Stock Keeping Unit identifier.
    /// </summary>
    public required string Sku { get; set; }

    /// <summary>
    /// Gets or sets the current quantity in stock.
    /// </summary>
    public int QuantityInStock { get; set; }

    /// <summary>
    /// Gets or sets the minimum stock level before reordering.
    /// </summary>
    public int ReorderLevel { get; set; }

    /// <summary>
    /// Gets or sets the category of the product.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the supplier identifier.
    /// </summary>
    public string? SupplierId { get; set; }

    /// <summary>
    /// Gets or sets the date when the product was added to inventory.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date when the product was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

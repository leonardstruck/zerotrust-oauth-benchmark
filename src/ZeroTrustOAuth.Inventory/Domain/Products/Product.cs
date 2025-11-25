using Ardalis.Result;

using ZeroTrustOAuth.Inventory.Domain.Categories;

namespace ZeroTrustOAuth.Inventory.Domain.Products;

public class Product
{
#pragma warning disable CS8618
    private Product() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }

    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public int Stock { get; private set; }

    public static Result<Product> Create(string sku, string name, decimal price, int stock,
        string? description = null, Guid? categoryId = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return Result.Invalid([new ValidationError(nameof(sku), "SKU cannot be empty.")]);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Invalid([new ValidationError(nameof(name), "Name cannot be empty.")]);
        }

        if (price <= 0)
        {
            return Result.Invalid([new ValidationError(nameof(price), "Price must be greater than zero.")]);
        }

        if (stock < 0)
        {
            return Result.Invalid([new ValidationError(nameof(stock), "Stock must be greater than or equal to zero.")]);
        }

        return Result.Success(new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            Stock = stock
        });
    }

    public Result Update(string? sku = null, string? name = null, decimal? price = null, int? stock = null,
        string? description = null, Guid? categoryId = null)
    {
        if (sku is not null)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return Result.Invalid([new ValidationError(nameof(sku), "SKU cannot be empty.")]);
            }

            Sku = sku;
        }

        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Invalid([new ValidationError(nameof(name), "Name cannot be empty.")]);
            }

            Name = name;
        }

        if (price is not null)
        {
            if (price <= 0)
            {
                return Result.Invalid([new ValidationError(nameof(price), "Price must be greater than zero.")]);
            }

            Price = price.Value;
        }

        if (stock is not null)
        {
            if (stock < 0)
            {
                return Result.Invalid([
                    new ValidationError(nameof(stock), "Stock must be greater than or equal to zero.")
                ]);
            }

            Stock = stock.Value;
        }

        if (description is not null)
        {
            Description = description;
        }

        if (categoryId is not null)
        {
            CategoryId = categoryId;
        }

        return Result.Success();
    }
}
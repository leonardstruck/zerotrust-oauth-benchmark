using ErrorOr;

namespace ZeroTrustOAuth.Inventory.Domain.Products;

public static class Errors
{
    public static Error NegativeStock => Error.Validation(
        "Product.NegativeStock",
        "Stock quantity cannot be negative.");

    public static Error NegativeReorderLevel => Error.Validation(
        "Product.NegativeReorderLevel",
        "Reorder level cannot be negative.");

    public static Error InsufficientStock(int requested, int available)
    {
        return Error.Validation(
            "Product.InsufficientStock",
            $"Cannot reduce stock by {requested}. Only {available} units available.");
    }
}
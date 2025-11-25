using Ardalis.Result;

namespace ZeroTrustOAuth.Inventory.Domain.Categories;

public sealed class Category
{
#pragma warning disable CS8618
    private Category() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }


    public static Result<Category> Create(string name, string? description = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Invalid([new ValidationError(nameof(name), "Name cannot be empty.")]);
        }

        Category category = new()
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        return Result.Success(category);
    }

    public Result Update(string? name = null, string? description = null)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Invalid([new ValidationError(nameof(name), "Name cannot be empty.")]);
            }

            Name = name;
        }

        if (description is not null)
        {
            Description = description;
        }


        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Invalid([new ValidationError(nameof(IsActive), "Category is already inactive.")]);
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
        {
            return Result.Invalid([new ValidationError(nameof(IsActive), "Category is already active.")]);
        }

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
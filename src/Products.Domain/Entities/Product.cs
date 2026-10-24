using Products.Domain.Common;

namespace Products.Domain.Entities;

/// <summary>
/// Product aggregate root. Instances can only be created through
/// <see cref="Create"/> so the domain invariants are always enforced.
/// </summary>
public sealed class Product : BaseEntity
{
    public const int NameMaxLength = 200;

    // Required by EF Core materialization.
    private Product()
    {
    }

    private Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public string Name { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public static Product Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (name.Trim().Length > NameMaxLength)
        {
            throw new ArgumentException(
                $"Product name cannot exceed {NameMaxLength} characters.",
                nameof(name));
        }

        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "Product price must be greater than zero.");
        }

        return new Product(name.Trim(), price);
    }
}

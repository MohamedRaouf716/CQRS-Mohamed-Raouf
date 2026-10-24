using Products.Domain.Entities;

namespace Products.Application.Common.Interfaces;

/// <summary>
/// Persistence abstraction owned by the Application layer. The Infrastructure
/// layer provides the EF Core implementation.
/// </summary>
public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

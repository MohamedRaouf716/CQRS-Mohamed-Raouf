namespace Products.Application.Features.Products.Dtos;

/// <summary>
/// Read model returned by the API. The domain entity is never exposed directly.
/// </summary>
public sealed record ProductDto(int Id, string Name, decimal Price);

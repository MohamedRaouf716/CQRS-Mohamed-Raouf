using MediatR;
using Products.Application.Features.Products.Dtos;

namespace Products.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Command that creates a new product. Returning the created DTO lets the API
/// respond with the representation of the new resource.
/// </summary>
public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;

using MediatR;
using Products.Application.Features.Products.Dtos;

namespace Products.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;

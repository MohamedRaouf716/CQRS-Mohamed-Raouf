using MediatR;
using Products.Application.Features.Products.Dtos;

namespace Products.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IRequest<ProductDto>;

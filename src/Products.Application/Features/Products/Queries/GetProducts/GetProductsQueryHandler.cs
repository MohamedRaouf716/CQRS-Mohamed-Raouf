using MediatR;
using Products.Application.Common.Interfaces;
using Products.Application.Features.Products.Dtos;

namespace Products.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler(IProductRepository repository)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await repository.GetAllAsync(cancellationToken);

        return products
            .Select(product => product.ToDto())
            .ToList();
    }
}

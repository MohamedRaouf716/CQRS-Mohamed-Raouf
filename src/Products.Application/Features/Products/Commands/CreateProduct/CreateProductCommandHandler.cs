using MediatR;
using Microsoft.Extensions.Logging;
using Products.Application.Common.Interfaces;
using Products.Application.Features.Products.Dtos;
using Products.Domain.Entities;

namespace Products.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository repository,
    ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price);

        await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created product {ProductId} with name {ProductName} and price {ProductPrice}",
            product.Id,
            product.Name,
            product.Price);

        return product.ToDto();
    }
}

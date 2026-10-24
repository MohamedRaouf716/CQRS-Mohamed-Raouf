using Products.Application.Features.Products.Dtos;
using Products.Domain.Entities;

namespace Products.Application.Features.Products;

internal static class ProductMappings
{
    public static ProductDto ToDto(this Product product)
        => new(product.Id, product.Name, product.Price);
}

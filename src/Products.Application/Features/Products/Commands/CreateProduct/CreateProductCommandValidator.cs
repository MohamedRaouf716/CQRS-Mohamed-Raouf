using FluentValidation;
using Products.Domain.Entities;

namespace Products.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(Product.NameMaxLength)
            .WithMessage($"Name cannot exceed {Product.NameMaxLength} characters.");

        RuleFor(command => command.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Price cannot have more than 2 decimal places.");
    }
}

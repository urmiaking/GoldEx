using FluentValidation;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Application.Validators.Products;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Validators;

public class CreateServerProductValidator : CreateProductValidator<Product>
{
    public CreateServerProductValidator(IProductRepository<Product> repository) : base(repository)
    {
        RuleFor(x => x.CreatedUserId)
            .NotEqual(Guid.Empty)
            .WithMessage("کاربر نامعتبر");
    }
}
using FluentValidation;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Application.Validators.Products;

namespace GoldEx.Server.Application.Validators;

public class CreateServerProductValidator : CreateProductValidator<Product, ProductCategory, GemStone>
{
    public CreateServerProductValidator(IProductRepository<Product, ProductCategory, GemStone> repository,
        IProductCategoryRepository<ProductCategory> categoryRepository) : base(repository, categoryRepository)
    {
        RuleFor(x => x.CreatedUserId)
            .NotEqual(Guid.Empty)
            .WithMessage("کاربر نامعتبر");
    }
}
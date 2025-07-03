using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Products;

namespace GoldEx.Server.Application.Validators.ProductCategories;

[ScopedService]
public class DeleteProductCategoryValidator : AbstractValidator<ProductCategory> 
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCategoryValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        RuleFor(x => x)
            .MustAsync(NotUsedByProducts)
            .WithMessage("امکان حذف این دسته بندی به دلیل استفاده در اجناس وجود ندارد");
    }

    private async Task<bool> NotUsedByProducts(ProductCategory category, CancellationToken cancellationToken = default)
    {
        return !await _productRepository.ExistsAsync(new ProductsByCategoryIdSpecification(category.Id), cancellationToken);
    }
}
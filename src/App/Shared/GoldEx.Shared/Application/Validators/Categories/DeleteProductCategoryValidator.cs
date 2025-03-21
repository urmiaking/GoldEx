using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators.Categories;

public class DeleteProductCategoryValidator<TCategory, TProduct, TGemStone> : AbstractValidator<TCategory> 
    where TCategory : ProductCategoryBase
    where TProduct : ProductBase<TCategory, TGemStone>
    where TGemStone : GemStoneBase
{
    private readonly IProductRepository<TProduct, TCategory, TGemStone> _productRepository;

    public DeleteProductCategoryValidator(IProductRepository<TProduct, TCategory, TGemStone> productRepository)
    {
        _productRepository = productRepository;

        RuleFor(x => x)
            .MustAsync(NotUsedByProducts).WithMessage("امکان حذف این دسته بندی به دلیل استفاده در اجناس وجود ندارد");
    }

    private async Task<bool> NotUsedByProducts(TCategory category, CancellationToken cancellationToken = default) 
        => await _productRepository.CheckCategoryUsedAsync(category.Id, cancellationToken);
}
using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators.Products;

public class UpdateProductValidator<TProduct, TCategory, TGemStone> : AbstractValidator<TProduct>
    where TProduct : ProductBase<TCategory, TGemStone>
    where TCategory : ProductCategoryBase
    where TGemStone : GemStoneBase
{
    public UpdateProductValidator(IProductRepository<TProduct, TCategory, TGemStone> repository, IProductCategoryRepository<TCategory> categoryRepository)
    {
        Include(new CreateProductValidator<TProduct, TCategory, TGemStone>(repository, categoryRepository));

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("شناسه کالا نمی تواند خالی باشد");
    }
}
using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators.Products;

public class UpdateProductValidator<TProduct, TCategory> : AbstractValidator<TProduct>
    where TProduct : ProductBase<TCategory>
    where TCategory : ProductCategoryBase
{
    public UpdateProductValidator(IProductRepository<TProduct, TCategory> repository, IProductCategoryRepository<TCategory> categoryRepository)
    {
        Include(new CreateProductValidator<TProduct, TCategory>(repository, categoryRepository));

        RuleFor(x => x.Id)
            .NotEqual(new ProductId(Guid.Empty)).WithMessage("شناسه کالا نمی تواند خالی باشد");
    }
}
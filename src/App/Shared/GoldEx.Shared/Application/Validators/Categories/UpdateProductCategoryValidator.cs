using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators.Categories;

public class UpdateProductCategoryValidator<T> :AbstractValidator<T> where T : ProductCategoryBase
{
    public UpdateProductCategoryValidator(IProductCategoryRepository<T> repository)
    {
        Include(new CreateProductCategoryValidator<T>(repository));

        RuleFor(x => x.Id)
            .NotEqual(new ProductCategoryId(Guid.Empty)).WithMessage("شناسه دسته بندی نمی تواند خالی باشد");
    }
}
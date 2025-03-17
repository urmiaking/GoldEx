using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators.Categories;

public class CreateProductCategoryValidator<T> : AbstractValidator<T> where T : ProductCategoryBase
{
    private readonly IProductCategoryRepository<T> _repository;

    public CreateProductCategoryValidator(IProductCategoryRepository<T> repository)
    {
        _repository = repository;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان دسته بندی نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("حداکثر طول دسته بندی 50 کاراکتر می باشد")
            .MustAsync(BeUniqueTitle).WithMessage("عنوان دسته بندی نمی تواند تکراری باشد");
    }

    private async Task<bool> BeUniqueTitle(T category, string title, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(title))
        {
            return true; // Let the NotEmpty rule handle this.
        }

        var existingCategory = await _repository.GetAsync(title, cancellationToken);

        if (existingCategory == null)
        {
            return true; // No existing category with this barcode, so it's unique.
        }

        // Check if it's the same category (update scenario)
        return category.Id.Value != Guid.Empty && category.Id == existingCategory.Id;
    }
}

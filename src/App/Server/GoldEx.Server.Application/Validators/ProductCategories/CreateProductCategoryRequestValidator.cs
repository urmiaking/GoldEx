using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Shared.DTOs.ProductCategories;

namespace GoldEx.Server.Application.Validators.ProductCategories;

[ScopedService]
internal class CreateProductCategoryRequestValidator : AbstractValidator<CreateProductCategoryRequest>
{
    private readonly IProductCategoryRepository _repository;

    public CreateProductCategoryRequestValidator(IProductCategoryRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان دسته بندی نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("حداکثر طول دسته بندی 50 کاراکتر می باشد")
            .MustAsync(BeUniqueTitle).WithMessage("عنوان دسته بندی نمی تواند تکراری باشد");
    }

    private async Task<bool> BeUniqueTitle(string title, CancellationToken cancellationToken = default)
    {
        return !await _repository.ExistsAsync(new ProductCategoriesByTitleSpecification(title), cancellationToken);
    }
}

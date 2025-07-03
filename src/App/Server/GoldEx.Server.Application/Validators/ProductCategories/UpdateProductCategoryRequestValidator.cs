using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Shared.DTOs.ProductCategories;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.ProductCategories;

[ScopedService]
internal class UpdateProductCategoryRequestValidator :AbstractValidator<(Guid id, UpdateProductCategoryRequest request)>
{
    private readonly IProductCategoryRepository _repository;

    public UpdateProductCategoryRequestValidator(IProductCategoryRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.request.Title)
            .NotEmpty().WithMessage("عنوان دسته بندی نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("حداکثر طول دسته بندی 50 کاراکتر می باشد")
            .MustAsync(BeUniqueTitle)
            .WithMessage("عنوان دسته بندی نمی تواند تکراری باشد");

        RuleFor(x => x.id)
            .NotEmpty().WithMessage("شناسه دسته بندی نمی تواند خالی باشد")
            .MustAsync(BeValidId)
            .WithMessage("دسته بندی با این شناسه وجود ندارد");
    }

    private async Task<bool> BeValidId(Guid id, CancellationToken cancellationToken)
    {
        var item = await _repository
            .Get(new ProductCategoriesByIdSpecification(new ProductCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null;
    }

    private async Task<bool> BeUniqueTitle((Guid id, UpdateProductCategoryRequest request) request, string title, CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new ProductCategoriesByTitleSpecification(title))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.id;
    }
}
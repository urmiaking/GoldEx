using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.ProductCategories;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.ProductCategories;

[ScopedService]
internal class UpdateProductCategoryRequestValidator :AbstractValidator<(Guid id, UpdateProductCategoryRequest request)>
{
    private readonly IProductCategoryRepository _repository;
    private readonly IProductRepository _productRepository;

    public UpdateProductCategoryRequestValidator(IProductCategoryRepository repository, IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;

        RuleFor(x => x.request.Title)
            .NotEmpty().WithMessage("عنوان دسته بندی نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("حداکثر طول دسته بندی 50 کاراکتر می باشد")
            .MustAsync(BeUniqueTitle)
            .WithMessage("عنوان دسته بندی نمی تواند تکراری باشد");

        RuleFor(x => x.request.PrefixCode)
            .NotEmpty().WithMessage("پیش کد دسته بندی نمی تواند خالی باشد")
            .MaximumLength(2).WithMessage("حداکثر طول پیش کد دسته بندی 2 کاراکتر می باشد")
            .MustAsync(BeUniqueCode).WithMessage("پیش کد دسته بندی نمی تواند تکراری باشد")
            .MustAsync(NotEditableIfUsedInProductBarcode).WithMessage("پیش کد دسته بندی قابل تغییر نمی باشد زیرا در بارکد محصولات استفاده شده است");

        RuleFor(x => x.id)
            .NotEmpty().WithMessage("شناسه دسته بندی نمی تواند خالی باشد")
            .MustAsync(BeValidId)
            .WithMessage("دسته بندی با این شناسه وجود ندارد");
    }

    private async Task<bool> NotEditableIfUsedInProductBarcode((Guid id, UpdateProductCategoryRequest request) request, string prefixCode, CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new ProductCategoriesByIdSpecification(new ProductCategoryId(request.id)))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        if (item.PrefixCode == prefixCode)
            return true;

        var usedInProduct = await _productRepository.ExistsAsync(new ProductsByCategoryPrefixCodeSpecification(prefixCode), cancellationToken);

        return !usedInProduct;
    }

    private async Task<bool> BeUniqueCode((Guid id, UpdateProductCategoryRequest request) request, string prefixCode, CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new ProductCategoriesByPrefixCodeSpecification(prefixCode))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.id;
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
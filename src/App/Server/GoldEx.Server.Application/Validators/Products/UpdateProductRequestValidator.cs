using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Products;

[ScopedService]
internal class UpdateProductRequestValidator : AbstractValidator<(Guid id, UpdateProductRequest request)>
{
    private readonly IProductRepository _repository;
    private readonly IProductCategoryRepository _categoryRepository;

    public UpdateProductRequestValidator(IProductRepository repository, IProductCategoryRepository categoryRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;

        RuleFor(x => x.request.Name)
            .NotEmpty().WithMessage("عنوان جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول عنوان جنس نمی تواند بیشتر از 50 کاراکتر باشد");

        RuleFor(x => x.request.Barcode)
            .NotEmpty().WithMessage("بارکد جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول بارکد جنس نمی تواند بیشتر از 50 کاراکتر باشد")
            .MustAsync(BeUniqueBarcode).WithMessage("بارکد جنس نباید تکراری باشد.");

        RuleFor(x => x.request.Weight)
            .GreaterThan(0).WithMessage("لطفا وزن جنس را وارد کنید");

        When(product => product.request.ProductType is ProductType.Gold or ProductType.Jewelry, () =>
        {
            RuleFor(product => product.request.Wage).NotNull().WithMessage("لطفا اجرت ساخت را وارد کنید");
            RuleFor(product => product.request.WageType)
                .NotNull()
                .WithMessage("لطفا نوع اجرت را وارد کنید");
        });

        When(product => product.request.ProductType is not (ProductType.Gold or ProductType.Jewelry), () =>
        {
            RuleFor(product => product.request.Wage).Null().WithMessage("اجرت ساخت برای سکه ها، طلای آبشده و دست دوم نباید وارد شود");
            RuleFor(product => product.request.WageType).Null().WithMessage("نوع اجرت برای سکه ها، طلای آبشده و دست دوم نباید وارد شود");
        });

        RuleFor(x => x.request.ProductType).IsInEnum().WithMessage("لطفا نوع جنس را انتخاب کنید");

        RuleFor(x => x.request.CaratType).IsInEnum().WithMessage("لطفا عیار را انتخاب کنید");

        RuleFor(x => x.request.ProductCategoryId)
            .NotEmpty().WithMessage("دسته بندی جنس نمی تواند خالی باشد")
            .NotEqual(Guid.Empty).WithMessage("دسته بندی جنس نمی تواند خالی باشد")
            .MustAsync(BeValidCategoryId).WithMessage("دسته بندی وارد شده معتبر نیست");

        RuleFor(x => x.id)
            .NotEmpty().WithMessage("شناسه کالا نمی تواند خالی باشد")
            .MustAsync(BeValidId).WithMessage("شناسه کالا معتبر نیست");
    }

    private async Task<bool> BeUniqueBarcode((Guid id, UpdateProductRequest request) request, string barcode, CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new ProductsByBarcodeSpecification(barcode))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.id;
    }

    private async Task<bool> BeValidCategoryId(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.ExistsAsync(new ProductCategoriesByIdSpecification(new ProductCategoryId(categoryId)), cancellationToken);
    }

    private async Task<bool> BeValidId(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new ProductsByIdSpecification(new ProductId(id)))
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null;
    }
}
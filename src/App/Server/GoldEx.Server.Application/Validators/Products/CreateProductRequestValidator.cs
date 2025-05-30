using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators.Products;

[ScopedService]
internal class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    private readonly IProductRepository _repository;
    private readonly IProductCategoryRepository _categoryRepository;
    public CreateProductRequestValidator(IProductRepository repository, IProductCategoryRepository categoryRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("عنوان جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول عنوان جنس نمی تواند بیشتر از 50 کاراکتر باشد");

        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("بارکد جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول بارکد جنس نمی تواند بیشتر از 50 کاراکتر باشد")
            .MustAsync(BeUniqueBarcode).WithMessage("بارکد جنس نباید تکراری باشد.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("لطفا وزن جنس را وارد کنید");

        When(product => product.ProductType is ProductType.Gold or ProductType.Jewelry, () =>
        {
            RuleFor(product => product.Wage).NotNull().WithMessage("لطفا اجرت ساخت را وارد کنید");
            RuleFor(product => product.WageType)
                .NotNull()
                .WithMessage("لطفا نوع اجرت را وارد کنید");
        });

        When(product => product.ProductType is not (ProductType.Gold or ProductType.Jewelry), () =>
        {
            RuleFor(product => product.Wage).NotNull().WithMessage("لطفا کامزد را وارد کنید");
            RuleFor(product => product.WageType)
                .NotNull()
                .WithMessage("لطفا کامزد را وارد کنید");
        });

        RuleFor(x => x.ProductType).IsInEnum().WithMessage("لطفا نوع جنس را انتخاب کنید");

        RuleFor(x => x.CaratType).IsInEnum().WithMessage("لطفا عیار را انتخاب کنید");

        RuleFor(x => x.ProductCategoryId)
            .NotEmpty().WithMessage("دسته بندی جنس نمی تواند خالی باشد")
            .NotEqual(Guid.Empty).WithMessage("دسته بندی جنس نمی تواند خالی باشد")
            .MustAsync(BeValidCategoryId).WithMessage("دسته بندی وارد شده معتبر نیست");
    }

    private async Task<bool> BeUniqueBarcode(CreateProductRequest request, string barcode, CancellationToken cancellationToken = default)
    {
        return !await _repository.ExistsAsync(new ProductsByBarcodeSpecification(barcode), cancellationToken);
    }

    private async Task<bool> BeValidCategoryId(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.ExistsAsync(new ProductCategoriesByIdSpecification(new ProductCategoryId(categoryId)), cancellationToken);
    }
}
using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Products;

[ScopedService]
internal class ProductRequestDtoValidator : AbstractValidator<ProductRequestDto>
{
    private readonly IProductRepository _repository;
    private readonly IProductCategoryRepository _categoryRepository;
    private readonly IPriceUnitRepository _priceUnitRepository;
    public ProductRequestDtoValidator(IProductRepository repository, IProductCategoryRepository categoryRepository, IPriceUnitRepository priceUnitRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _priceUnitRepository = priceUnitRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("عنوان جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول عنوان جنس نمی تواند بیشتر از 50 کاراکتر باشد");

        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("بارکد جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول بارکد جنس نمی تواند بیشتر از 50 کاراکتر باشد")
            .MustAsync(BeUniqueBarcode).WithMessage("بارکد جنس نباید تکراری باشد.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("لطفا وزن جنس را وارد کنید");

        RuleFor(x => x)
            .Must(x => x.ProductType is not ProductType.OldGold)
            .WithMessage("نوع جنس نمی تواند طلای کهنه باشد. لطفا نوع دیگری انتخاب کنید");

        RuleFor(product => product.Wage).NotNull().WithMessage("لطفا اجرت ساخت را وارد کنید");

        RuleFor(product => product.WageType)
            .NotNull()
            .WithMessage("لطفا نوع اجرت را وارد کنید");

        When(x => x.WageType is WageType.Fixed, () =>
        {
            RuleFor(product => product.WagePriceUnitId)
                .NotNull().WithMessage("لطفا واحد قیمت اجرت را وارد کنید")
                .NotEmpty().WithMessage("واحد قیمت اجرت نمی تواند خالی باشد")
                .MustAsync(BeValidWagePriceId)
                .WithMessage("واحد قیمت اجرت وارد شده معتبر نیست");
        });

        RuleFor(x => x.ProductType).IsInEnum().WithMessage("لطفا نوع جنس را انتخاب کنید");

        RuleFor(x => x.CaratType).IsInEnum().WithMessage("لطفا عیار را انتخاب کنید");

        RuleFor(x => x.ProductCategoryId)
            .NotEmpty()
                .When(x => x.ProductType is ProductType.Gold or ProductType.Jewelry)
                .WithMessage("دسته بندی جنس نمی تواند خالی باشد")
            .MustAsync(BeValidCategoryId).WithMessage("دسته بندی وارد شده معتبر نیست");

        RuleFor(x => x.Id)
            .MustAsync(BeValidId)
            .WithMessage("شناسه جنس وارد شده معتبر نیست");
    }

    private async Task<bool> BeValidId(Guid? id, CancellationToken cancellationToken = default)
    {
        if (!id.HasValue)
            return true;

        return await _repository.ExistsAsync(new ProductsByIdSpecification(new ProductId(id.Value)), cancellationToken);
    }

    private async Task<bool> BeValidWagePriceId(Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        if (!priceUnitId.HasValue)
            return true;

        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId.Value)), cancellationToken);
    }

    private async Task<bool> BeUniqueBarcode(ProductRequestDto request, string barcode, 
        CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new ProductsByBarcodeSpecification(barcode))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.Id;
    }

    private async Task<bool> BeValidCategoryId(Guid? categoryId, CancellationToken cancellationToken = default)
    {
        if (!categoryId.HasValue)
            return true;

        return await _categoryRepository.ExistsAsync(new ProductCategoriesByIdSpecification(new ProductCategoryId(categoryId.Value)), cancellationToken);
    }
}
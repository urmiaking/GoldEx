using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators.Products;

public class CreateProductValidator<TProduct, TCategory> : AbstractValidator<TProduct>
    where TProduct : ProductBase<TCategory>
    where TCategory : ProductCategoryBase
{
    private readonly IProductRepository<TProduct, TCategory> _repository;
    private readonly IProductCategoryRepository<TCategory> _categoryRepository;
    public CreateProductValidator(IProductRepository<TProduct, TCategory> repository, IProductCategoryRepository<TCategory> categoryRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("عنوان جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول عنوان جنس نمی تواند بیشتر از 50 کاراکتر باشد");

        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("بارکد جنس نمی تواند خالی باشد")
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
            RuleFor(product => product.Wage).Null().WithMessage("اجرت ساخت برای سکه ها، طلای آبشده و دست دوم نباید وارد شود");
            RuleFor(product => product.WageType).Null().WithMessage("نوع اجرت برای سکه ها، طلای آبشده و دست دوم نباید وارد شود");
        });

        RuleFor(x => x.ProductType).IsInEnum().WithMessage("لطفا نوع جنس را انتخاب کنید");

        RuleFor(x => x.CaratType).IsInEnum().WithMessage("لطفا عیار را انتخاب کنید");

        RuleFor(x => x.ProductCategoryId)
            .NotEmpty().WithMessage("دسته بندی جنس نمی تواند خالی باشد")
            .NotEqual(new ProductCategoryId(Guid.Empty)).WithMessage("دسته بندی جنس نمی تواند خالی باشد")
            .MustAsync(ValidCategoryId).WithMessage("دسته بندی وارد شده معتبر نیست");
    }

    private async Task<bool> ValidCategoryId(ProductCategoryId categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetAsync(categoryId, cancellationToken);

        return category is not null;
    }

    private async Task<bool> BeUniqueBarcode(TProduct product, string barcode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(barcode))
        {
            return true; // Let the NotEmpty rule handle this.
        }

        var existingProduct = await _repository.GetAsync(barcode, cancellationToken);

        if (existingProduct == null)
        {
            return true; // No existing product with this barcode, so it's unique.
        }

        // Check if it's the same product (update scenario)
        return product.Id.Value != Guid.Empty && product.Id == existingProduct.Id;
    }
}
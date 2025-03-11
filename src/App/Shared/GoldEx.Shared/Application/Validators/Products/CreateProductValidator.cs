using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators.Products;

public class CreateProductValidator<T> : AbstractValidator<T> where T : ProductBase
{
    private readonly IProductRepository<T> _repository;
    public CreateProductValidator(IProductRepository<T> repository)
    {
        _repository = repository;

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
    }

    private async Task<bool> BeUniqueBarcode(T product, string barcode, CancellationToken cancellationToken)
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
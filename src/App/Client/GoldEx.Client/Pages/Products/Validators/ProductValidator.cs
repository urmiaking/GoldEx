using FluentValidation;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Products.Validators;

public class ProductValidator : AbstractValidator<ProductVm>
{
    public ProductValidator()
    {
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("لطفا وزن را وارد کنید");

        RuleFor(x => x.Name)
            .NotEmpty().When(x => x.ProductType is not ProductType.MoltenGold).WithMessage("عنوان جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول عنوان جنس نمی تواند بیشتر از 50 کاراکتر باشد");

        When(product => product.WageType is WageType.Percent, () =>
        {
            RuleFor(product => product.Wage)
                .InclusiveBetween(0, 100)
                .WithMessage("اجرت ساخت باید بین 0 الی 100 درصد باشد");
        });

        RuleFor(x => x.ProductType).IsInEnum().WithMessage("لطفا نوع جنس را انتخاب کنید");

        RuleFor(x => x.Fineness)
            .InclusiveBetween(0, 1000)
            .WithMessage("عیار باید بین 0 تا 1000 باشد");

        When(x => x.ProductType == ProductType.Jewelry, () => 
        {
            RuleFor(x => x.StonePriceUnit)
                .NotNull().WithMessage("لطفا واحد قیمت سنگ را انتخاب کنید");
        });
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ProductVm>.CreateWithOptions((ProductVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
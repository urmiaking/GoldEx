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

        When(product => product.ProductType is ProductType.Gold or ProductType.Jewelry, () =>
        {
            RuleFor(product => product.Wage).NotNull().WithMessage("لطفا اجرت ساخت را وارد کنید");
            RuleFor(product => product.WageType)
                .NotNull()
                .WithMessage("لطفا نوع اجرت را وارد کنید");
        });

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

        RuleFor(x => x.CategoryVm)
            .NotEmpty()
            .When(x => x.ProductType is ProductType.Gold or ProductType.Jewelry)
            .WithMessage("لطفا دسته بندی را انتخاب کنید");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ProductVm>.CreateWithOptions((ProductVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
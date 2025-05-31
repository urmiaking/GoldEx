using FluentValidation;
using GoldEx.Client.Pages.Calculate.ViewModels;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Calculate.Validators;

public class CalculatorValidator : AbstractValidator<CalculatorVm>
{
    public CalculatorValidator()
    {
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("لطفا وزن را وارد کنید");

        When(product => product.ProductType is ProductType.Gold or ProductType.Jewelry, () =>
        {
            RuleFor(product => product.Wage).NotNull().WithMessage("لطفا اجرت ساخت را وارد کنید");
            RuleFor(product => product.WageType)
                .NotNull().WithMessage("لطفا نوع اجرت را وارد کنید");
        });

        When(product => product.ProductType is not (ProductType.Gold or ProductType.Jewelry), () =>
        {
            RuleFor(product => product.Wage).Null().WithMessage("اجرت ساخت برای طلای آبشده و دست دوم نباید وارد شود");
            RuleFor(product => product.WageType).Null().WithMessage("نوع اجرت برای سکه ها، طلای آبشده و دست دوم نباید وارد شود");
        });

        When(product => product.WageType is WageType.Percent, () =>
        {
            RuleFor(product => product.Wage)
                .InclusiveBetween(0, 100)
                .WithMessage("اجرت ساخت باید بین 0 الی 100 درصد باشد");
        });

        RuleFor(x => x.ProductType).IsInEnum().WithMessage("لطفا نوع جنس را انتخاب کنید");

        RuleFor(x => x.CaratType).IsInEnum().WithMessage("لطفا عیار را انتخاب کنید");

        RuleFor(x => x.ProfitPercent)
            .InclusiveBetween(0, 100)
            .WithMessage("سود باید بین 0 تا 100 درصد باشد");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CalculatorVm>.CreateWithOptions((CalculatorVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
using FluentValidation;
using GoldEx.Client.Components.Calculator.ViewModels;

namespace GoldEx.Client.Components.Calculator.Validators;

public class ReverseCalculatorValidator : AbstractValidator<ReverseCalculatorVm>
{
    public ReverseCalculatorValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("لطفا قیمت را وارد کنید");

        RuleFor(x => x.Fineness)
            .NotNull()
            .WithMessage("لطفا عیار را وارد کنید")
            .InclusiveBetween(0, 1000)
            .WithMessage("عیار باید بین 0 تا 1000 باشد");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("لطفا نرخ واحد را وارد کنید");

        RuleFor(x => x.ProfitPercent)
            .NotNull()
            .WithMessage("لطفا سود را وارد کنید")
            .InclusiveBetween(0, 100)
            .WithMessage("سود باید بین 0 تا 100 درصد باشد");

        RuleFor(x => x.GoldUnitType)
            .IsInEnum()
            .WithMessage("لطفا واحد سنجش طلا را انتخاب کنید");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ReverseCalculatorVm>.CreateWithOptions((ReverseCalculatorVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
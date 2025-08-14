using FluentValidation;
using GoldEx.Client.Pages.Invoices.ViewModels;

namespace GoldEx.Client.Pages.Invoices.Validators;

public class CoinItemValidator : AbstractValidator<CoinItemVm>
{
    public CoinItemValidator()
    {
        RuleFor(c => c.Coin)
            .NotNull().WithMessage("نوع سکه الزامی است");

        RuleFor(c => c.Quantity)
            .GreaterThan(0).WithMessage("تعداد سکه باید بزرگتر از صفر باشد");

        RuleFor(c => c.ProfitPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد سود باید بزرگتر یا مساوی صفر باشد")
            .LessThanOrEqualTo(100).WithMessage("درصد سود نمی‌تواند بیشتر از 100 باشد");

        RuleFor(c => c.UnitPrice)
            .GreaterThan(0).WithMessage("قیمت واحد سکه باید بزرگتر از صفر باشد");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CoinItemVm>.CreateWithOptions((CoinItemVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
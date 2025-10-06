using FluentValidation;
using GoldEx.Client.Pages.Invoices.ViewModels;

namespace GoldEx.Client.Pages.Invoices.Validators;

public class CurrencyItemValidator : AbstractValidator<CurrencyItemVm>
{
    public CurrencyItemValidator()
    {
        RuleFor(c => c.Currency)
            .NotNull().WithMessage("نوع ارز الزامی است");

        RuleFor(c => c.Amount)
            .GreaterThan(0).WithMessage("مبلغ ارز باید بزرگتر از صفر باشد");

        RuleFor(c => c.UnitPrice)
            .GreaterThan(0).WithMessage("قیمت واحد ارز باید بزرگتر از صفر باشد");

        RuleFor(c => c.ProfitPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد سود نمی‌تواند منفی باشد")
            .LessThanOrEqualTo(100).WithMessage("درصد سود نمی‌تواند بیشتر از 100 باشد");

        RuleFor(c => c.TaxPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد مالیات نمی‌تواند منفی باشد")
            .LessThanOrEqualTo(100).WithMessage("درصد مالیات نمی‌تواند بیشتر از 100 باشد");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CurrencyItemVm>.CreateWithOptions((CurrencyItemVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
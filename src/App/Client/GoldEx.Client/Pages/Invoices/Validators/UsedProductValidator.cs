using FluentValidation;
using GoldEx.Client.Pages.Invoices.ViewModels;

namespace GoldEx.Client.Pages.Invoices.Validators;

public class UsedProductValidator : AbstractValidator<UsedProductVm>
{
    public UsedProductValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("لطفا عنوان/توضیحات را وارد کنید")
            .MaximumLength(100).WithMessage("عنوان/توضیحات نمی تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.Fineness)
            .InclusiveBetween(0, 1000).WithMessage("عیار باید بین 0 و 1000 باشد.")
            .NotEmpty().WithMessage("لطفا عیار را وارد کنید");

        RuleFor(x => x.GramPrice)
            .GreaterThan(0).WithMessage("نرخ طلا باید بزرگتر از 0 باشد.")
            .NotEmpty().WithMessage("لطفا نرخ طلا را وارد کنید");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("وزن باید بزرگتر یا مساوی 0 باشد.")
            .NotEmpty().WithMessage("لطفا وزن را وارد کنید");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UsedProductVm>.CreateWithOptions((UsedProductVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
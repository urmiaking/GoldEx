using FluentValidation;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Invoices.Validators;

public class UsedProductValidator : AbstractValidator<UsedProductVm>
{
    public UsedProductValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("لطفا {PropertyName} را وارد کنید")
            .MaximumLength(100).WithMessage("{PropertyName} نمی تواند بیشتر از {MaxLength} کاراکتر باشد.");

        RuleFor(x => x.Fineness)
            .InclusiveBetween(0, 1000).WithMessage("{PropertyName} باید بین {From} و {To} باشد.")
            .NotEmpty().WithMessage("لطفا {PropertyName} را وارد کنید");

        RuleFor(x => x.GramPrice)
            .GreaterThan(0).WithMessage("{PropertyName} باید بزرگتر از 0 باشد.")
            .NotEmpty().WithMessage("لطفا {PropertyName} را وارد کنید");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} باید بزرگتر یا مساوی 0 باشد.")
            .NotEmpty().WithMessage("لطفا {PropertyName} را وارد کنید");

        RuleFor(x => x.ProductType)
            .Must(x => x is ProductType.Gold or ProductType.Jewelry)
            .WithMessage("نوع محصول باید طلا یا جواهر باشد.");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UsedProductVm>.CreateWithOptions((UsedProductVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
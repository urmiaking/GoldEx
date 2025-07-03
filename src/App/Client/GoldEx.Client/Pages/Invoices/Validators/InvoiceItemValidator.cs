using FluentValidation;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.Validators;

namespace GoldEx.Client.Pages.Invoices.Validators;

public class InvoiceItemValidator : AbstractValidator<InvoiceItemVm>
{
    public InvoiceItemValidator()
    {
        RuleFor(i => i.Quantity)
            .GreaterThan(0).WithMessage("تعداد باید بزرگتر از صفر باشد");

        RuleFor(i => i.GramPrice)
            .GreaterThan(0).WithMessage("نرخ گرم باید بزرگتر از صفر باشد");

        RuleFor(i => i.ProfitPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد سود نمی‌تواند منفی باشد");

        RuleFor(i => i.TaxPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد مالیات نمی‌تواند منفی باشد");

        RuleFor(i => i.Product)
            .NotNull().WithMessage("اطلاعات محصول برای آیتم فاکتور الزامی است")
            .SetValidator(new ProductValidator());
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<InvoiceItemVm>.CreateWithOptions((InvoiceItemVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
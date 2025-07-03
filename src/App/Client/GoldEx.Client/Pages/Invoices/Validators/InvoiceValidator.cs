using FluentValidation;
using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;

namespace GoldEx.Client.Pages.Invoices.Validators;

public class InvoiceValidator : AbstractValidator<InvoiceVm>
{
    public InvoiceValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("لطفا شماره فاکتور را وارد کنید")
            .GreaterThan(0).WithMessage("شماره فاکتور باید بزرگتر از صفر باشد");

        RuleFor(x => x.InvoiceDate)
            .NotEmpty().WithMessage("تاریخ فاکتور الزامی است");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.InvoiceDate)
            .When(x => x.DueDate.HasValue)
            .WithMessage("تاریخ سررسید نمی‌تواند قبل از تاریخ فاکتور باشد");

        RuleFor(x => x.Customer)
            .NotNull().WithMessage("اطلاعات مشتری الزامی است")
            .SetValidator(new CustomerValidator());

        RuleFor(x => x.InvoiceItems)
            .NotNull()
            .NotEmpty().WithMessage("فاکتور باید حداقل دارای یک آیتم باشد");

        RuleForEach(x => x.InvoiceItems)
            .SetValidator(new InvoiceItemValidator());

        RuleForEach(x => x.InvoiceDiscounts)
            .SetValidator(new InvoiceDiscountValidator());

        RuleForEach(x => x.InvoiceExtraCosts)
            .SetValidator(new InvoiceExtraCostValidator());

        RuleForEach(x => x.InvoicePayments)
            .SetValidator(new InvoicePaymentValidator());
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<InvoiceVm>.CreateWithOptions((InvoiceVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
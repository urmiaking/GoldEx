using FluentValidation;
using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.Enums;

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

        RuleFor(x => x)
            .Must(invoice =>
                invoice.ProductItems.Any() ||
                invoice.CurrencyItems.Any() ||
                invoice.CoinItems.Any() ||
                invoice.UsedProducts.Any())
            .WithMessage("فاکتور باید حداقل دارای یک آیتم (کالا، ارز، سکه یا جنس دست دوم) باشد.");

        RuleFor(x => x)
            .Must(invoice =>
                invoice.ProductItems.Any() ||
                invoice.CurrencyItems.Any() ||
                invoice.CoinItems.Any())
            .When(invoice =>
                invoice.InvoiceType == InvoiceType.Sell &&
                invoice.UsedProducts.Any())
            .WithMessage("در فاکتور فروش، کالای دست دوم نمی‌تواند به تنهایی ثبت شود و باید همراه با یک آیتم دیگر باشد.");

        RuleFor(x => x.Customer)
            .NotNull().WithMessage("اطلاعات مشتری الزامی است")
            .SetValidator(new CustomerValidator());

        RuleForEach(x => x.ProductItems)
            .SetValidator(new ProductItemValidator());

        RuleForEach(x => x.CoinItems)
            .SetValidator(new CoinItemValidator());

        RuleForEach(x => x.CurrencyItems)
            .SetValidator(new CurrencyItemValidator());

        RuleForEach(x => x.InvoiceDiscounts)
            .SetValidator(new InvoiceDiscountValidator());

        RuleForEach(x => x.InvoiceExtraCosts)
            .SetValidator(new InvoiceExtraCostValidator());

        RuleForEach(x => x.InvoicePayments)
            .SetValidator(new InvoicePaymentValidator());

        RuleForEach(x => x.UsedProducts)
            .SetValidator(new UsedProductValidator());
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<InvoiceVm>.CreateWithOptions((InvoiceVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
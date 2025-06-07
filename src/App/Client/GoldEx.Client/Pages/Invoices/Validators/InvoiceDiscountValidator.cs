using FluentValidation;
using GoldEx.Client.Pages.Invoices.ViewModels;

namespace GoldEx.Client.Pages.Invoices.Validators;

public class InvoiceDiscountValidator : AbstractValidator<InvoiceDiscountVm>
{
    public InvoiceDiscountValidator()
    {
        RuleFor(d => d.Amount)
            .GreaterThan(0).WithMessage("مبلغ تخفیف باید بزرگتر از صفر باشد");

        RuleFor(d => d.Description)
            .MaximumLength(250).WithMessage("توضیحات تخفیف نمی‌تواند بیش از 250 کاراکتر باشد");

        RuleFor(d => d.PriceUnit)
            .NotNull().WithMessage("واحد ارزی تخفیف الزامی است");
    }
}
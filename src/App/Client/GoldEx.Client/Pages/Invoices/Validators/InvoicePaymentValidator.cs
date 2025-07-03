using FluentValidation;
using GoldEx.Client.Pages.Invoices.ViewModels;

namespace GoldEx.Client.Pages.Invoices.Validators;

public class InvoicePaymentValidator : AbstractValidator<InvoicePaymentVm>
{
    public InvoicePaymentValidator()
    {
        RuleFor(p => p.Amount)
            .GreaterThan(0).WithMessage("مبلغ پرداختی باید بزرگتر از صفر باشد");

        RuleFor(p => p.PaymentDate)
            .NotNull().WithMessage("تاریخ پرداخت الزامی است")
            .NotEmpty().WithMessage("تاریخ پرداخت الزامی است");

        RuleFor(p => p.PaymentMethod)
            .NotNull().WithMessage("روش پرداخت الزامی است");

        RuleFor(p => p.PriceUnit)
            .NotNull().WithMessage("واحد ارزی پرداخت الزامی است");
    }
}
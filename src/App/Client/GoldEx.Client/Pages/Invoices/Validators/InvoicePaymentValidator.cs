using FluentValidation;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.Enums;

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

        When(x => !x.VoucherId.HasValue && x.PaymentType is PaymentType.InternalCash, () =>
        {
            RuleFor(p => p.FinancialAccount)
                .NotNull().WithMessage("وارد کردن حساب مالی الزامی است");
        });

        When(x => x.PaymentType is PaymentType.CustomerTransfer, () =>
        {
            RuleFor(x => x.Endorser)
                .NotNull().WithMessage("طرف حساب الزامی است");
        });

        When(x => x.PaymentType is PaymentType.MoltenGoldInventory or PaymentType.UsedGoldInventory, () =>
        {
            RuleFor(x => x.GoldFineness)
                .NotNull().WithMessage("عیار طلا الزامی است");
        });

        RuleFor(p => p.PriceUnit)
            .NotNull().WithMessage("واحد ارزی پرداخت الزامی است");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<InvoicePaymentVm>.CreateWithOptions((InvoicePaymentVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
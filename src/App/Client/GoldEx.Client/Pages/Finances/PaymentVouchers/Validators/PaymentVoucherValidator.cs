using FluentValidation;
using GoldEx.Client.Pages.Finances.PaymentVouchers.ViewModels;

namespace GoldEx.Client.Pages.Finances.PaymentVouchers.Validators;

public class PaymentVoucherValidator : AbstractValidator<PaymentVoucherVm>
{
    public PaymentVoucherValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("لطفا شرح را وارد کنید")
            .MaximumLength(150).WithMessage("شرح نمی تواند بیشتر از 150 کاراکتر باشد");

        RuleFor(x => x.VoucherNumber)
            .NotEmpty().WithMessage("لطفا شماره رسید پرداخت را وارد کنید");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("لطفا تاریخ پرداخت را وارد کنید");

        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("لطفا مبلغ را وارد کنید")
            .GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد");

        RuleFor(x => x.SourceFinancialAccount)
            .NotEmpty().WithMessage("لطفا حساب پرداختی را انتخاب کنید");

        RuleFor(x => x.Customer)
            .NotEmpty().WithMessage("لطفا طرف حساب را انتخاب کنید");

        RuleFor(x => x.VoucherType)
            .IsInEnum().WithMessage("لطفا نوع سند را انتخاب کنید");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<PaymentVoucherVm>.CreateWithOptions((PaymentVoucherVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
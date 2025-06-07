using FluentValidation;
using GoldEx.Client.Pages.Invoices.ViewModels;

namespace GoldEx.Client.Pages.Invoices.Validators;

public class InvoiceExtraCostValidator : AbstractValidator<InvoiceExtraCostVm>
{
    public InvoiceExtraCostValidator()
    {
        RuleFor(e => e.Amount)
            .GreaterThan(0).WithMessage("مبلغ مخارج اضافی باید بزرگتر از صفر باشد");

        RuleFor(e => e.Description)
            .MaximumLength(250).WithMessage("توضیحات مخارج اضافی نمی‌تواند بیش از 250 کاراکتر باشد");

        RuleFor(e => e.PriceUnit)
            .NotNull().WithMessage("واحد ارزی مخارج اضافی الزامی است");
    }
}
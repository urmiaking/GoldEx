using FluentValidation;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Customers.Validators;

public class BankAccountValidator : AbstractValidator<BankAccountVm>
{
    public BankAccountValidator()
    {
        RuleFor(x => x.AccountHolderName)
            .NotEmpty().WithMessage("نام صاحب حساب نباید خالی باشد.")
            .MaximumLength(100).WithMessage("نام صاحب حساب نباید بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("نام بانک نباید خالی باشد.")
            .MaximumLength(100).WithMessage("نام بانک نباید بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.PriceUnit)
            .NotEmpty().WithMessage("واحد ارزی نباید خالی باشد.");

        RuleFor(x => x.BankAccountType)
            .IsInEnum().WithMessage("نوع حساب بانکی نامعتبر است.");

        When(x => x.BankAccountType is BankAccountType.Local, () =>
        {
            RuleFor(x => x.LocalAccountNumber)
                .MaximumLength(20).WithMessage("شماره حساب نباید بیشتر از 20 کاراکتر باشد.");
            RuleFor(x => x.CardNumber)
                .MaximumLength(20).WithMessage("شماره کارت نباید بیشتر از 20 کاراکتر باشد.");
            RuleFor(x => x.ShabaNumber)
                .MaximumLength(26).WithMessage("شماره شبا نباید بیشتر از 26 کاراکتر باشد.");
        });

        When(x => x.BankAccountType is BankAccountType.International, () =>
        {
            RuleFor(x => x.InternationalAccountNumber)
                .MaximumLength(20).WithMessage("شماره حساب نباید بیشتر از 20 کاراکتر باشد.");
            RuleFor(x => x.SwiftBicCode)
                .MaximumLength(11).WithMessage("کد سوئیفت نباید بیشتر از 11 کاراکتر باشد.");
            RuleFor(x => x.IbanNumber)
                .MaximumLength(34).WithMessage("شماره IBAN نباید بیشتر از 34 کاراکتر باشد.");
        });
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<BankAccountVm>.CreateWithOptions((BankAccountVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
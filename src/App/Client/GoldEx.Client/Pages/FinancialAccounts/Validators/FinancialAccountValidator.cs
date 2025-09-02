using FluentValidation;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.FinancialAccounts.Validators;

public class FinancialAccountValidator : AbstractValidator<FinancialAccountVm>
{
    public FinancialAccountValidator()
    {
        RuleFor(x => x.PriceUnit)
            .NotEmpty().WithMessage("واحد ارزی نباید خالی باشد.");

        RuleFor(x => x.HolderName)
            .NotEmpty().WithMessage("نام صاحب حساب نباید خالی باشد.")
            .MaximumLength(100).WithMessage("نام صاحب حساب نباید بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.BrokerName)
            .NotEmpty().WithMessage("نام بانک/کارگزار نباید خالی باشد.")
            .MaximumLength(100).WithMessage("نام بانک/کارگزار نباید بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.FinancialAccountType)
            .IsInEnum().WithMessage("نوع حساب نامعتبر است.");

        When(x => x.FinancialAccountType is FinancialAccountType.LocalBankAccount && x.LocalBankAccount is not null, () =>
        {
            RuleFor(x => x.LocalBankAccount!.AccountNumber)
                .NotEmpty().WithMessage("شماره حساب نباید خالی باشد")
                .MaximumLength(20).WithMessage("شماره حساب نباید بیشتر از 20 کاراکتر باشد.");
            RuleFor(x => x.LocalBankAccount!.CardNumber)
                .MaximumLength(20).WithMessage("شماره کارت نباید بیشتر از 20 کاراکتر باشد.");
            RuleFor(x => x.LocalBankAccount!.ShabaNumber)
                .MaximumLength(26).WithMessage("شماره شبا نباید بیشتر از 26 کاراکتر باشد.");
        });

        When(x => x.FinancialAccountType is FinancialAccountType.InternationalBankAccount && x.InternationalBankAccount is not null, () =>
        {
            RuleFor(x => x.InternationalBankAccount!.AccountNumber)
                .NotEmpty().WithMessage("شماره حساب نباید خالی باشد")
                .MaximumLength(20).WithMessage("شماره حساب نباید بیشتر از 20 کاراکتر باشد.");
            RuleFor(x => x.InternationalBankAccount!.SwiftBicCode)
                .MaximumLength(11).WithMessage("کد سوئیفت نباید بیشتر از 11 کاراکتر باشد.");
            RuleFor(x => x.InternationalBankAccount!.IbanNumber)
                .MaximumLength(34).WithMessage("شماره IBAN نباید بیشتر از 34 کاراکتر باشد.");
        });
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<FinancialAccountVm>.CreateWithOptions((FinancialAccountVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
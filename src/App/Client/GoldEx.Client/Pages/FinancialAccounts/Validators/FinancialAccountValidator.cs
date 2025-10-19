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

        RuleFor(x => x.FinancialAccountType)
            .IsInEnum().WithMessage("نوع حساب نامعتبر است.");

        When(x => x.FinancialAccountType != FinancialAccountType.Gold &&
                  x is not { FinancialAccountType: FinancialAccountType.Cash, CashAccount.AccountType: CashAccountType.Internal }, () =>
        {
            RuleFor(x => x.HolderName)
                .NotEmpty()
                .WithMessage(dto => dto.FinancialAccountType == FinancialAccountType.Cash
                    ? "نام صاحب حساب برای حساب های سپرده نزد دیگران الزامی است"
                    : "نام صاحب حساب نباید خالی باشد.")
                .MaximumLength(100).WithMessage("نام صاحب حساب نباید بیشتر از 100 کاراکتر باشد.");

            RuleFor(x => x.BrokerName)
                .NotEmpty()
                .WithMessage(dto => dto.FinancialAccountType == FinancialAccountType.Cash
                    ? "نام بانک/کارگزار برای حساب های سپرده نزد دیگران الزامی است"
                    : "نام بانک/کارگزار نباید خالی باشد.")
                .MaximumLength(100).WithMessage("نام بانک/کارگزار نباید بیشتر از 100 کاراکتر باشد.");
        });

        RuleFor(x => x.LocalBankAccount)
            .NotNull().WithMessage("اطلاعات حساب بانکی داخلی الزامی است.")
            .SetValidator(new LocalBankAccountValidator()!)
            .When(x => x.FinancialAccountType == FinancialAccountType.LocalBankAccount);

        RuleFor(x => x.InternationalBankAccount)
            .NotNull().WithMessage("اطلاعات حساب بانکی بین المللی الزامی است.")
            .SetValidator(new InternationalBankAccountValidator()!)
            .When(x => x.FinancialAccountType == FinancialAccountType.InternationalBankAccount);
    }

    // This part remains the same
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<FinancialAccountVm>.CreateWithOptions((FinancialAccountVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}

public class LocalBankAccountValidator : AbstractValidator<LocalBankAccountVm>
{
    public LocalBankAccountValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("شماره حساب نباید خالی باشد")
            .MaximumLength(20).WithMessage("شماره حساب نباید بیشتر از 20 کاراکتر باشد.");

        RuleFor(x => x.CardNumber)
            .MaximumLength(20).WithMessage("شماره کارت نباید بیشتر از 20 کاراکتر باشد.");

        RuleFor(x => x.ShabaNumber)
            .MaximumLength(40).WithMessage("شماره شبا نباید بیشتر از 40 کاراکتر باشد.");
    }
}

public class InternationalBankAccountValidator : AbstractValidator<InternationalBankAccountVm>
{
    public InternationalBankAccountValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("شماره حساب نباید خالی باشد")
            .MaximumLength(20).WithMessage("شماره حساب نباید بیشتر از 20 کاراکتر باشد.");

        RuleFor(x => x.SwiftBicCode)
            .MaximumLength(20).WithMessage("کد سوئیفت نباید بیشتر از 20 کاراکتر باشد.");

        RuleFor(x => x.IbanNumber)
            .MaximumLength(45).WithMessage("شماره IBAN نباید بیشتر از 45 کاراکتر باشد.");
    }
}
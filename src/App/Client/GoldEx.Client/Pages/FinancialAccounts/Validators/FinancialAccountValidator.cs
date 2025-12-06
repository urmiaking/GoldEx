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
            .When(x => x.FinancialAccountType == FinancialAccountType.LocalBankAccount && x.LocalBankAccount != null);

        RuleFor(x => x.InternationalBankAccount)
            .NotNull().WithMessage("اطلاعات حساب بانکی بین المللی الزامی است.")
            .SetValidator(new InternationalBankAccountValidator()!)
            .When(x => x.FinancialAccountType == FinancialAccountType.InternationalBankAccount && x.InternationalBankAccount != null);
    }

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
        RuleFor(x => x)
            .Must(AtLeastOneLocalFieldProvided)
            .WithMessage("حداقل یکی از فیلدهای شماره حساب، شماره کارت یا شماره شبا باید پر شود.");

        RuleFor(x => x.AccountNumber)
            .MaximumLength(20).WithMessage("شماره حساب نباید بیشتر از 20 کاراکتر باشد.");

        RuleFor(x => x.CardNumber)
            .MaximumLength(20).WithMessage("شماره کارت نباید بیشتر از 20 کاراکتر باشد.");

        RuleFor(x => x.ShabaNumber)
            .MaximumLength(40).WithMessage("شماره شبا نباید بیشتر از 40 کاراکتر باشد.");
    }

    private bool AtLeastOneLocalFieldProvided(LocalBankAccountVm vm)
    {
        return !string.IsNullOrWhiteSpace(vm.AccountNumber) ||
               !string.IsNullOrWhiteSpace(vm.CardNumber) ||
               !string.IsNullOrWhiteSpace(vm.ShabaNumber);
    }
}

public class InternationalBankAccountValidator : AbstractValidator<InternationalBankAccountVm>
{
    public InternationalBankAccountValidator()
    {
        RuleFor(x => x)
            .Must(AtLeastOneInternationalFieldProvided)
            .WithMessage("حداقل یکی از فیلدهای شماره حساب، کد سوئیفت یا شماره IBAN باید پر شود.");

        RuleFor(x => x.AccountNumber)
            .MaximumLength(30).WithMessage("شماره حساب نباید بیشتر از 30 کاراکتر باشد.");

        RuleFor(x => x.SwiftBicCode)
            .MaximumLength(11).WithMessage("کد سوئیفت نباید بیشتر از 11 کاراکتر باشد.");

        RuleFor(x => x.IbanNumber)
            .MaximumLength(34).WithMessage("شماره IBAN نباید بیشتر از 34 کاراکتر باشد.");
    }

    private bool AtLeastOneInternationalFieldProvided(InternationalBankAccountVm vm)
    {
        return !string.IsNullOrWhiteSpace(vm.AccountNumber) ||
               !string.IsNullOrWhiteSpace(vm.SwiftBicCode) ||
               !string.IsNullOrWhiteSpace(vm.IbanNumber);
    }
}
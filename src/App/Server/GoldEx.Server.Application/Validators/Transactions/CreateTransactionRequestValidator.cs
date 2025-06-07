using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Server.Application.Validators.Transactions;

[ScopedService]
internal class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    private readonly ITransactionRepository _repository;
    public CreateTransactionRequestValidator(ITransactionRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("شماره تراكنش نمی تواند خالی باشد")
            .GreaterThan(0).WithMessage("شماره تراكنش نمی تواند منفی باشد")
            .MustAsync(BeUniqueNumber).WithMessage("شماره تراكنش تكراری است");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("شرح تراكنش نمی تواند خالی باشد");

        RuleFor(transaction => transaction)
            .Must(HaveAtLeastCreditOrDebitInfo)
            .WithMessage("وارد کردن حداقل یکی از مقادیر بدهکاری یا بستانکاری الزامی است");

        When(transaction => transaction.Credit.HasValue, () => {
            RuleFor(transaction => transaction.CreditPriceUnitId)
                .NotNull().WithMessage("وارد کردن واحد تبدیل بستانکاری الزامی است");

            RuleFor(transaction => transaction.CreditRate)
                .NotNull().WithMessage("وارد کردن نرخ تبدیل بستانکاری الزامی است")
                .GreaterThanOrEqualTo(0).WithMessage("نرخ تبدیل بستانکاری نباید منفی باشد");

            RuleFor(transaction => transaction.Credit)
                 .GreaterThan(0).WithMessage("مقدار بستانکاری نباید منفی باشد");
        });

        When(transaction => transaction.Debit.HasValue, () => {
            RuleFor(transaction => transaction.DebitPriceUnitId)
                .NotNull().WithMessage("وارد کردن واحد تبدیل بدهکاری الزامی است");

            RuleFor(transaction => transaction.DebitRate)
                .NotNull().WithMessage("وارد کردن نرخ تبدیل بدهکاری الزامی است")
                .GreaterThanOrEqualTo(0).WithMessage("نرخ تبدیل بدهکاری نباید منفی باشد");

            RuleFor(transaction => transaction.Debit)
                .GreaterThan(0).WithMessage("مقدار بدهکاری نباید منفی باشد");
        });
    }

    private async Task<bool> BeUniqueNumber(long number, CancellationToken cancellationToken = default)
    {
        return !await _repository.ExistsAsync(new TransactionsByNumberSpecification(number), cancellationToken);
    }

    private static bool HaveAtLeastCreditOrDebitInfo(CreateTransactionRequest transaction)
    {
        return transaction.Credit.HasValue || transaction.Debit.HasValue;
    }
}
using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Application.Validators.Transactions;

public class CreateTransactionValidator<TTransaction, TCustomer> : AbstractValidator<TTransaction>
    where TTransaction : TransactionBase<TCustomer>
    where TCustomer : CustomerBase
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("شماره تراكنش نمي تواند خالي باشد");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("شرح تراكنش نمي تواند خالي باشد");

        RuleFor(transaction => transaction)
            .Must(HaveAtLeastCreditOrDebitInfo)
            .WithMessage("وارد کردن حداقل یکی از مقادیر بدهکاری یا بستانکاری الزامی است");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("مشتری مشخص نشده است");

        When(transaction => transaction.Credit.HasValue, () => {
            RuleFor(transaction => transaction.CreditUnit)
                .NotNull()
                .WithMessage("وارد کردن واحد تبدیل بستانکاری الزامی است");

            When(transaction => transaction.CreditUnit is not UnitType.IRR, () =>
            {
                RuleFor(transaction => transaction.CreditRate)
                    .NotNull()
                    .WithMessage("وارد کردن نرخ تبدیل بستانکاری الزامی است");
            });

            RuleFor(transaction => transaction.Credit)
                 .GreaterThan(0)
                 .WithMessage("مقدار بستانکاری نباید منفی باشد");
        });

        When(transaction => transaction.Debit.HasValue, () => {
            RuleFor(transaction => transaction.DebitUnit)
                .NotNull()
                .WithMessage("وارد کردن واحد تبدیل بدهکاری الزامی است");

            When(transaction => transaction.DebitUnit is not UnitType.IRR, () =>
            {
                RuleFor(transaction => transaction.DebitRate)
                    .NotNull()
                    .WithMessage("وارد کردن نرخ تبدیل بدهکاری الزامی است");
            });

            RuleFor(transaction => transaction.Debit)
                .GreaterThan(0)
                .WithMessage("مقدار بستانکاری نباید منفی باشد");
        });
    }

    private static bool HaveAtLeastCreditOrDebitInfo(TTransaction transaction)
    {
        return transaction.Credit.HasValue || transaction.Debit.HasValue;
    }
}
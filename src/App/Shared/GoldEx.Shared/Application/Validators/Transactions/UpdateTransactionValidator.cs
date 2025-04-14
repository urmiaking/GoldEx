using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;

namespace GoldEx.Shared.Application.Validators.Transactions;

public class UpdateTransactionValidator<TTransaction, TCustomer> : AbstractValidator<TTransaction>
    where TTransaction : TransactionBase<TCustomer>
    where TCustomer : CustomerBase
{
    public UpdateTransactionValidator()
    {
        Include(new CreateTransactionValidator<TTransaction, TCustomer>());

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("شناسه تراكنش نمي تواند خالي باشد");
    }
}
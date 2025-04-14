using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;

namespace GoldEx.Shared.Application.Validators.Transactions;

public class DeleteTransactionValidator<TTransaction, TCustomer> : AbstractValidator<TTransaction>
    where TTransaction : TransactionBase<TCustomer>
    where TCustomer : CustomerBase
{
    public DeleteTransactionValidator()
    {
        
    }
}
using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Application.Validators.Transactions;

[ScopedService]
internal class DeleteTransactionValidator : AbstractValidator<Transaction>
{
    public DeleteTransactionValidator()
    {
        // No validation for now!
    }
}
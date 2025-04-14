using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.TransactionAggregate;

public class Transaction : TransactionBase<Customer>, ISoftDeleteEntity
{
    public Transaction(DateTime dateTime,
        int number,
        string description,
        double? credit,
        UnitType? creditUnit,
        double? creditRate,
        double? debit,
        UnitType? debitUnit,
        double? debitRate,
        CustomerId customerId) : base(dateTime,
        number,
        description,
        credit,
        creditUnit,
        creditRate,
        debit,
        debitUnit,
        debitRate,
        customerId)
    {
    }

    public Transaction(TransactionId id,
        DateTime dateTime,
        int number,
        string description,
        double? credit,
        UnitType? creditUnit,
        double? creditRate,
        double? debit,
        UnitType? debitUnit,
        double? debitRate,
        CustomerId customerId) : base(id,
        dateTime,
        number,
        description,
        credit,
        creditUnit,
        creditRate,
        debit,
        debitUnit,
        debitRate,
        customerId)
    {
    }

    public bool IsDeleted { get; private set; }
    public void SetDeleted() => IsDeleted = true;
}
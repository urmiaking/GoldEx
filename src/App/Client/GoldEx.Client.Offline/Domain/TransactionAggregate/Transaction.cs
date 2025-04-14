using GoldEx.Client.Offline.Domain.CustomerAggregate;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Offline.Domain.TransactionAggregate;

public class Transaction : TransactionBase<Customer>, ITrackableEntity
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
        CustomerId customerId)
        : base(
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
        CustomerId customerId
    ) : this(dateTime, number, description, credit, creditUnit, creditRate, debit, debitUnit, debitRate, customerId)
    {
        Id = id;
    }

    private Transaction()
    {
    }

    public ModifyStatus Status { get; private set; } = ModifyStatus.Created;
    public void SetStatus(ModifyStatus status) => Status = status;
}
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Domain.Aggregates.TransactionAggregate;

public readonly record struct TransactionId(Guid Value);
public class TransactionBase<TCustomer> : EntityBase<TransactionId>, ISyncableEntity
    where TCustomer : CustomerBase
{
    public TransactionBase(DateTime dateTime,
        int number,
        string description,
        double? credit,
        UnitType? creditUnit,
        double? creditRate,
        double? debit,
        UnitType? debitUnit,
        double? debitRate,
        CustomerId customerId) 
        : this(new TransactionId(Guid.NewGuid()),
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

    public TransactionBase(TransactionId id, 
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
    ) : base(id)
    {
        DateTime = dateTime;
        Number = number;
        Description = description;
        Credit = credit;
        CreditUnit = creditUnit;
        CreditRate = creditRate;
        Debit = debit;
        DebitUnit = debitUnit;
        DebitRate = debitRate;
        CustomerId = customerId;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected TransactionBase()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    public DateTime DateTime { get; private set; }
    public int Number { get; private set; }
    public string Description { get; private set; }
    public double? Credit { get; private set; }
    public UnitType? CreditUnit { get; private set; }
    public double? CreditRate { get; private set; }
    public double? Debit { get; private set; }
    public UnitType? DebitUnit { get; private set; }
    public double? DebitRate { get; private set; }

    public TCustomer? Customer { get; private set; }
    public CustomerId CustomerId { get; private set; }

    public DateTime LastModifiedDate { get; private set; }
    public void SetLastModifiedDate() => LastModifiedDate = DateTime.UtcNow;

    public void SetDateTime(DateTime date) => DateTime = date;
    public void SetNumber(int number) => Number = number;
    public void SetDescription(string description) => Description = description;
    public void SetCredit(double? credit) => Credit = credit;
    public void SetCreditUnit(UnitType? unitType) => CreditUnit = unitType;
    public void SetCreditRate(double? creditRate) => CreditRate = creditRate;
    public void SetDebit(double? debit) => Debit = debit;
    public void SetDebitUnit(UnitType? unitType) => DebitUnit = unitType;
    public void SetDebitRate(double? debitRate) => DebitRate = debitRate;
    public void SetCustomer(CustomerId customerId) => CustomerId = customerId;
}
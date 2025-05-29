using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.TransactionAggregate;

public readonly record struct TransactionId(Guid Value);
public class Transaction : EntityBase<TransactionId>
{
    public static Transaction Create(DateTime dateTime,
        int number,
        string description,
        decimal? credit,
        UnitType? creditUnit,
        decimal? creditRate,
        decimal? debit,
        UnitType? debitUnit,
        decimal? debitRate,
        CustomerId customerId)
    {
        return new Transaction
        {
            Id = new TransactionId(Guid.NewGuid()),
            DateTime = dateTime,
            Number = number,
            Description = description,
            Credit = credit,
            CreditUnit = creditUnit,
            CreditRate = creditRate,
            Debit = debit,
            DebitUnit = debitUnit,
            DebitRate = debitRate,
            CustomerId = customerId
        };

    }

#pragma warning disable CS8618
    private Transaction() { }
#pragma warning restore CS8618

    public DateTime DateTime { get; private set; }
    public int Number { get; private set; }
    public string Description { get; private set; }
    public decimal? Credit { get; private set; }
    public UnitType? CreditUnit { get; private set; }
    public decimal? CreditRate { get; private set; }
    public decimal? Debit { get; private set; }
    public UnitType? DebitUnit { get; private set; }
    public decimal? DebitRate { get; private set; }

    public Customer? Customer { get; private set; }
    public CustomerId CustomerId { get; private set; }

    public void SetDateTime(DateTime date) => DateTime = date;
    public void SetNumber(int number) => Number = number;
    public void SetDescription(string description) => Description = description;
    public void SetCredit(decimal? credit) => Credit = credit;
    public void SetCreditUnit(UnitType? unitType) => CreditUnit = unitType;
    public void SetCreditRate(decimal? creditRate) => CreditRate = creditRate;
    public void SetDebit(decimal? debit) => Debit = debit;
    public void SetDebitUnit(UnitType? unitType) => DebitUnit = unitType;
    public void SetDebitRate(decimal? debitRate) => DebitRate = debitRate;
    public void SetCustomer(CustomerId customerId) => CustomerId = customerId;
}
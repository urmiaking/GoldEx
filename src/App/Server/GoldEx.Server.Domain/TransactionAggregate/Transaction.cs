using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Domain.TransactionAggregate;

public readonly record struct TransactionId(Guid Value);
public class Transaction : EntityBase<TransactionId>
{
    public static Transaction Create(DateTime dateTime,
        long number,
        string description,
        PriceUnitId priceUnitId,
        decimal? credit,
        PriceUnitId? creditUnitId,
        decimal? creditRate,
        decimal? debit,
        PriceUnitId? debitUnitId,
        decimal? debitRate,
        CustomerId customerId)
    {
        return new Transaction
        {
            Id = new TransactionId(Guid.NewGuid()),
            DateTime = dateTime,
            Number = number,
            Description = description,
            PriceUnitId = priceUnitId,
            Credit = credit,
            CreditUnitId = creditUnitId,
            CreditRate = creditRate,
            Debit = debit,
            DebitUnitId = debitUnitId,
            DebitRate = debitRate,
            CustomerId = customerId
        };

    }

#pragma warning disable CS8618
    private Transaction() { }
#pragma warning restore CS8618

    public DateTime DateTime { get; private set; }
    public long Number { get; private set; }
    public string Description { get; private set; }
    public decimal? Credit { get; private set; }
    public decimal? CreditRate { get; private set; }
    public decimal? Debit { get; private set; }
    public decimal? DebitRate { get; private set; }

    public PriceUnitId? PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public PriceUnitId? CreditUnitId { get; private set; }
    public PriceUnit? CreditUnit { get; private set; }

    public PriceUnitId? DebitUnitId { get; private set; }
    public PriceUnit? DebitUnit { get; private set; }

    public Customer? Customer { get; private set; }
    public CustomerId CustomerId { get; private set; }

    public void SetPriceUnitId(PriceUnitId priceUnitId) => PriceUnitId = priceUnitId;
    public void SetDateTime(DateTime date) => DateTime = date;
    public void SetNumber(long number) => Number = number;
    public void SetDescription(string description) => Description = description;
    public void SetCredit(decimal? credit) => Credit = credit;
    public void SetCreditUnit(PriceUnitId? unitTypeId) => CreditUnitId = unitTypeId;
    public void SetCreditRate(decimal? creditRate) => CreditRate = creditRate;
    public void SetDebit(decimal? debit) => Debit = debit;
    public void SetDebitUnit(PriceUnitId? unitTypeId) => DebitUnitId = unitTypeId;
    public void SetDebitRate(decimal? debitRate) => DebitRate = debitRate;
    public void SetCustomer(CustomerId customerId) => CustomerId = customerId;
}
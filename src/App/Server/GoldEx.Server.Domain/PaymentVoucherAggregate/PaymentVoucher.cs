using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.BankAccountAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Domain.PaymentVoucherAggregate;

public readonly record struct PaymentVoucherId(Guid Value);
public class PaymentVoucher : EntityBase<PaymentVoucherId>
{
    public static PaymentVoucher Create(
        CustomerId customerId,
        BankAccountId bankAccountId,
        DateOnly paymentDate,
        decimal amount,
        PriceUnitId amountPriceUnitId,
        PriceUnitId voucherPriceUnitId,
        string description)
    {
        return new PaymentVoucher
        {
            Id = new PaymentVoucherId(Guid.NewGuid()),
            CustomerId = customerId,
            BankAccountId = bankAccountId,
            PaymentDate = paymentDate,
            Amount = amount,
            AmountPriceUnitId = amountPriceUnitId,
            VoucherPriceUnitId = voucherPriceUnitId,
            Description = description
        };
    }

    public Customer? Customer { get; private set; }
    public CustomerId CustomerId { get; private set; }

    public BankAccount? BankAccount { get; private set; }
    public BankAccountId BankAccountId { get; private set; }

    public long VoucherNumber { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaymentDate { get; private set; }

    public PriceUnit? AmountPriceUnit { get; private set; }
    public PriceUnitId AmountPriceUnitId { get; private set; }

    /// <summary>
    /// The exchange rate of the amount price unit to the voucher price unit.
    /// </summary>
    public decimal? ExchangeRate { get; private set; }

    public PriceUnit? VoucherPriceUnit { get; private set; }
    public PriceUnitId VoucherPriceUnitId { get; private set; }

#pragma warning disable CS8618 
    private PaymentVoucher() { }
#pragma warning restore CS8618

    public void SetCustomerId(CustomerId customerId) => CustomerId = customerId;
    public void SetBankAccountId(BankAccountId bankAccountId) => BankAccountId = bankAccountId;
    public void SetPaymentDate(DateOnly paymentDate) => PaymentDate = paymentDate;
    public void SetAmount(decimal amount) => Amount = amount;
    public void SetAmountPriceUnitId(PriceUnitId priceUnitId) => AmountPriceUnitId = priceUnitId;
    public void SetVoucherPriceUnitId(PriceUnitId priceUnitId) => VoucherPriceUnitId = priceUnitId;
    public void SetDescription(string description) => Description = description;
    public void SetExchangeRate(decimal exchangeRate) => ExchangeRate = exchangeRate;
}
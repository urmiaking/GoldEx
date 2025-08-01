using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Domain.PaymentVoucherAggregate;

public readonly record struct PaymentVoucherId(Guid Value);
public class PaymentVoucher : EntityBase<PaymentVoucherId>
{
    public static PaymentVoucher Create(
        decimal amount,
        long voucherNumber,
        string description,
        decimal? exchangeRate,
        DateOnly paymentDate,
        FinancialAccountId sourceFinancialAccountId,
        FinancialAccountId destinationFinancialAccountId,
        PriceUnitId voucherPriceUnitId)
    {
        return new PaymentVoucher
        {
            Id = new PaymentVoucherId(Guid.NewGuid()),
            SourceFinancialAccountId = sourceFinancialAccountId,
            DestinationFinancialAccountId = destinationFinancialAccountId,
            VoucherNumber = voucherNumber,
            Description = description,
            PaymentDate = paymentDate,
            Amount = amount,
            ExchangeRate = exchangeRate,
            VoucherPriceUnitId = voucherPriceUnitId
        };
    }

    public FinancialAccount? DestinationFinancialAccount { get; private set; }
    public FinancialAccountId DestinationFinancialAccountId { get; private set; }

    public FinancialAccount? SourceFinancialAccount { get; private set; }
    public FinancialAccountId SourceFinancialAccountId { get; private set; }

    public long VoucherNumber { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaymentDate { get; private set; }

    /// <summary>
    /// The exchange rate of the amount price unit to the voucher price unit.
    /// </summary>
    public decimal? ExchangeRate { get; private set; }

    public PriceUnit? VoucherPriceUnit { get; private set; }
    public PriceUnitId VoucherPriceUnitId { get; private set; }

#pragma warning disable CS8618 
    private PaymentVoucher() { }
#pragma warning restore CS8618

    public void SetSourceFinancialAccountId(FinancialAccountId financialAccountId) => SourceFinancialAccountId = financialAccountId;
    public void SetDestinationFinancialAccountId(FinancialAccountId financialAccountId) => DestinationFinancialAccountId = financialAccountId;
    public void SetPaymentDate(DateOnly paymentDate) => PaymentDate = paymentDate;
    public void SetVoucherNumber(long voucherNumber) => VoucherNumber = voucherNumber;
    public void SetAmount(decimal amount) => Amount = amount;
    public void SetVoucherPriceUnitId(PriceUnitId priceUnitId) => VoucherPriceUnitId = priceUnitId;
    public void SetDescription(string description) => Description = description;
    public void SetExchangeRate(decimal? exchangeRate) => ExchangeRate = exchangeRate;
}
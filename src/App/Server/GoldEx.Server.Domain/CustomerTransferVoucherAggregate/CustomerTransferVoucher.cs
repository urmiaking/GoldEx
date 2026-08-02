using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Domain.CustomerTransferVoucherAggregate;

public readonly record struct CustomerTransferVoucherId(Guid Value);

public class CustomerTransferVoucher : EntityBase<CustomerTransferVoucherId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }

    public static CustomerTransferVoucher Create(
        long voucherNumber,
        DateOnly transferDate,
        CustomerId sourceCustomerId,
        CustomerId destinationCustomerId,
        PriceUnitId priceUnitId,
        decimal amount,
        decimal? exchangeRate,
        InvoiceId? sourceInvoiceId,
        InvoiceId? destinationInvoiceId,
        string? description,
        StoreId storeId = default)
    {
        return new CustomerTransferVoucher
        {
            Id = new CustomerTransferVoucherId(Guid.CreateVersion7()),
            VoucherNumber = voucherNumber,
            TransferDate = transferDate,
            SourceCustomerId = sourceCustomerId,
            DestinationCustomerId = destinationCustomerId,
            PriceUnitId = priceUnitId,
            Amount = amount,
            ExchangeRate = exchangeRate,
            SourceInvoiceId = sourceInvoiceId,
            DestinationInvoiceId = destinationInvoiceId,
            Description = description ?? string.Empty,
            StoreId = storeId
        };
    }

    public long VoucherNumber { get; private set; }
    public DateOnly TransferDate { get; private set; }

    public CustomerId SourceCustomerId { get; private set; }
    public Customer? SourceCustomer { get; private set; }

    public CustomerId DestinationCustomerId { get; private set; }
    public Customer? DestinationCustomer { get; private set; }

    public PriceUnitId PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public decimal Amount { get; private set; }
    public decimal? ExchangeRate { get; private set; }

    public InvoiceId? SourceInvoiceId { get; private set; }
    public Invoice? SourceInvoice { get; private set; }

    public InvoiceId? DestinationInvoiceId { get; private set; }
    public Invoice? DestinationInvoice { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyList<Transaction>? Transactions { get; private set; }

#pragma warning disable CS8618 
    private CustomerTransferVoucher() { }
#pragma warning restore CS8618

    public void SetVoucherNumber(long voucherNumber) => VoucherNumber = voucherNumber;
    public void SetTransferDate(DateOnly transferDate) => TransferDate = transferDate;
    public void SetSourceCustomerId(CustomerId sourceCustomerId) => SourceCustomerId = sourceCustomerId;
    public void SetDestinationCustomerId(CustomerId destinationCustomerId) => DestinationCustomerId = destinationCustomerId;
    public void SetPriceUnitId(PriceUnitId priceUnitId) => PriceUnitId = priceUnitId;
    public void SetAmount(decimal amount) => Amount = amount;
    public void SetExchangeRate(decimal? exchangeRate) => ExchangeRate = exchangeRate;
    public void SetSourceInvoiceId(InvoiceId? sourceInvoiceId) => SourceInvoiceId = sourceInvoiceId;
    public void SetDestinationInvoiceId(InvoiceId? destinationInvoiceId) => DestinationInvoiceId = destinationInvoiceId;
    public void SetDescription(string? description) => Description = description ?? string.Empty;
}

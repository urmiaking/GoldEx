using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceId(Guid Value);
public class Invoice : EntityBase<InvoiceId>
{
    public Invoice(int invoiceNumber, decimal? additionalPrices, decimal? discount, CustomerId customerId) : base(new InvoiceId(Guid.NewGuid()))
    {
        InvoiceNumber = invoiceNumber;
        AdditionalPrices = additionalPrices;
        Discount = discount;
        CustomerId = customerId;
    }

    private Invoice() { }

    public int InvoiceNumber { get; private set; }
    public decimal? Discount { get; private set; }
    public decimal? AdditionalPrices { get; private set; }
    public DateTime InvoiceDate { get; private set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; private set; }

    public void SetInvoiceNumber(int invoiceNumber) => InvoiceNumber = invoiceNumber;
    public void SetDiscount(decimal? discount) => Discount = discount;
    public void SetAdditionalPrices(decimal? additionalPrices) => AdditionalPrices = additionalPrices;
    public void SetLastModifiedDate() => LastModifiedDate = DateTime.UtcNow;

    #region Customer

    public CustomerId CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public void SetCustomerId(CustomerId customerId) => CustomerId = customerId;

    #endregion

    #region InvoiceItems

    private readonly List<InvoiceItem> _items = [];
    public IReadOnlyList<InvoiceItem> Items => _items;

    public void AddItem(InvoiceItem item)
    {
        if (_items.Any(x => x.ProductId != item.ProductId))
            _items.Add(item);
    }

    public void RemoveItem(InvoiceItem item)
    {
        if (_items.Any(x => x.ProductId == item.ProductId))
            _items.Remove(item);
    }   

    public void ClearItems() => _items.Clear();

    #endregion

    #region Debt

    public InvoiceDebt? InvoiceDebt { get; private set; }
    public void SetInvoiceDebt(InvoiceDebt invoiceDebt) => InvoiceDebt = invoiceDebt;

    public void SetInvoiceDebtAsPaid(DateTime paymentDateTime)
    {
        if (InvoiceDebt is null)
            throw new InvalidOperationException("Invoice debt is not set.");

        InvoiceDebt.SetPaid(paymentDateTime);
    }

    public void SetInvoiceDebtAsUnpaid()
    {
        if (InvoiceDebt is null)
            throw new InvalidOperationException("Invoice debt is not set.");

        InvoiceDebt.SetUnpaid();
    }

    public void SetInvoiceDebtAsPartiallyPaid(decimal amount, DateTime paymentDateTime)
    {
        if (InvoiceDebt is null)
            throw new InvalidOperationException("Invoice debt is not set.");

        //InvoiceDebt.SetPartiallyPaid(amount, paymentDateTime);
    }

    #endregion
}
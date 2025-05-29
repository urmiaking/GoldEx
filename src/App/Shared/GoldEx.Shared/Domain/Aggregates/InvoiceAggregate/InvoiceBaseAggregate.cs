using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Domain.Aggregates.InvoiceAggregate;

public readonly record struct InvoiceId(Guid Value);
public class InvoiceBaseAggregate<TCustomer, TInvoiceDebt, TInvoiceItem, TProduct, TCategory, TGemStone> : EntityBase<InvoiceId>, ISyncableEntity
    where TCustomer : CustomerBase
    where TInvoiceDebt : InvoiceDebtBase
    where TInvoiceItem : InvoiceItemBase<TProduct, TCategory, TGemStone>
    where TProduct : ProductBase<TCategory, TGemStone>
    where TCategory : ProductCategoryBase
    where TGemStone : GemStoneBase
{
    public InvoiceBaseAggregate(int invoiceNumber, double? additionalPrices, double? discount, CustomerId customerId) : base(new InvoiceId(Guid.NewGuid()))
    {
        InvoiceNumber = invoiceNumber;
        AdditionalPrices = additionalPrices;
        Discount = discount;
        CustomerId = customerId;
    }

    protected InvoiceBaseAggregate() { }

    public int InvoiceNumber { get; private set; }
    public double? Discount { get; private set; }
    public double? AdditionalPrices { get; private set; }
    public DateTime InvoiceDate { get; private set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; private set; }

    public void SetInvoiceNumber(int invoiceNumber) => InvoiceNumber = invoiceNumber;
    public void SetDiscount(double? discount) => Discount = discount;
    public void SetAdditionalPrices(double? additionalPrices) => AdditionalPrices = additionalPrices;
    public void SetLastModifiedDate() => LastModifiedDate = DateTime.UtcNow;

    #region Customer

    public CustomerId CustomerId { get; private set; }
    public TCustomer? Customer { get; private set; }

    public void SetCustomerId(CustomerId customerId) => CustomerId = customerId;

    #endregion

    #region InvoiceItems

    private readonly List<TInvoiceItem> _items = [];
    public IReadOnlyList<TInvoiceItem> Items => _items;

    public void AddItem(TInvoiceItem item)
    {
        if (_items.Any(x => x.ProductId != item.ProductId))
            _items.Add(item);
    }

    public void RemoveItem(TInvoiceItem item)
    {
        if (_items.Any(x => x.ProductId == item.ProductId))
            _items.Remove(item);
    }   

    public void ClearItems() => _items.Clear();

    #endregion

    #region Debt

    public TInvoiceDebt? InvoiceDebt { get; private set; }
    public void SetInvoiceDebt(TInvoiceDebt invoiceDebt) => InvoiceDebt = invoiceDebt;

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

    public void SetInvoiceDebtAsPartiallyPaid(double amount, DateTime paymentDateTime)
    {
        if (InvoiceDebt is null)
            throw new InvalidOperationException("Invoice debt is not set.");

        //InvoiceDebt.SetPartiallyPaid(amount, paymentDateTime);
    }

    #endregion

}
using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceId(Guid Value);

public class Invoice : EntityBase<InvoiceId>
{
    public static Invoice Create(long invoiceNumber,
        decimal? unpaidAmountExchangeRate,
        decimal? exchangeRate,
        InvoiceType invoiceType,
        CustomerId customerId, 
        PriceUnitId priceUnitId, 
        PriceUnitId? unpaidPriceUnitId,
        DateOnly invoiceDate,
        DateOnly? dueDate)
    {
        return new Invoice
        {
            Id = new InvoiceId(Guid.NewGuid()),
            InvoiceNumber = invoiceNumber,
            InvoiceType = invoiceType,
            CustomerId = customerId,
            InvoiceDate = invoiceDate,
            PriceUnitId = priceUnitId,
            DueDate = dueDate,
            UnpaidPriceUnitId = unpaidPriceUnitId,
            UnpaidAmountExchangeRate = unpaidAmountExchangeRate,
            ExchangeRate = exchangeRate
        };
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Invoice() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public long InvoiceNumber { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateOnly InvoiceDate { get; private set; }
    public InvoiceType InvoiceType { get; private set; }
    public decimal? ExchangeRate { get; private set; }

    public void SetInvoiceNumber(long invoiceNumber) => InvoiceNumber = invoiceNumber;
    public void SetDueDate(DateOnly? dueDate) => DueDate = dueDate;
    public void SetInvoiceDate(DateOnly invoiceDate) => InvoiceDate = invoiceDate;
    public void SetExchangeRate(decimal? exchangeRate) => ExchangeRate = exchangeRate;

    #region Customer

    public CustomerId CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public Invoice SetCustomerId(CustomerId customerId)
    {
        CustomerId = customerId;
        return this;
    }

    #endregion

    #region InvoiceItems

    private readonly List<InvoiceProductItem> _products = [];
    public IReadOnlyList<InvoiceProductItem> ProductItems => _products;

    private readonly List<InvoiceCoinItem> _coins = [];
    public IReadOnlyList<InvoiceCoinItem> CoinItems => _coins;

    private readonly List<InvoiceCurrencyItem> _currencies = [];
    public IReadOnlyList<InvoiceCurrencyItem> CurrencyItems => _currencies;

    private readonly List<InvoiceTradeIn> _tradeIns = [];
    public IReadOnlyList<InvoiceTradeIn> TradeIns => _tradeIns;

    public void AddProductItem(InvoiceProductItem productItem)
    {
        if (_products.Any(x => x.ProductId == productItem.ProductId))
            throw new InvalidOperationException(
                $"The product with ID {productItem.ProductId.Value} is already present in the ProductItems list");

        _products.Add(productItem);
    }

    public void ClearProductItems() => _products.Clear();

    public void SetCoinItems(IEnumerable<InvoiceCoinItem> coinItems)
    {
        _coins.Clear();

        foreach (var coinItem in coinItems)
        {
            if (_coins.Any(x => x.CoinId == coinItem.CoinId))
                throw new InvalidOperationException(
                    $"The coin with ID {coinItem.CoinId.Value} is already present in the CoinItems list");

            _coins.Add(coinItem);
        }
    }

    public void SetCurrencyItems(IEnumerable<InvoiceCurrencyItem> currencyItems)
    {
        _currencies.Clear();

        foreach (var currencyItem in currencyItems)
        {
            if (_currencies.Any(x => x.CurrencyId == currencyItem.CurrencyId))
                throw new InvalidOperationException(
                    $"The Currency with ID {currencyItem.CurrencyId.Value} is already present in the CurrencyItems list");

            _currencies.Add(currencyItem);
        }
    }

    public void AddTradeIn(string description,
        decimal weight,
        decimal gramPrice,
        int fineness,
        bool isSellable,
        ProductId? resultingProductId)
    {
        _tradeIns.Add(InvoiceTradeIn.Create(description,
            weight,
            gramPrice,
            fineness,
            isSellable,
            resultingProductId,
            this));
    }

    public void ClearTradeIns() => _tradeIns.Clear();

    #endregion

    #region Payments

    public IReadOnlyList<InvoicePayment>? InvoicePayments { get; private set; }

    #endregion

    #region ExtraCosts

    private readonly List<InvoiceExtraCost> _extraCosts = [];
    public IReadOnlyList<InvoiceExtraCost> ExtraCosts => _extraCosts;

    public Invoice SetExtraCosts(IEnumerable<InvoiceExtraCost>? extraCosts)
    {
        ClearExtraCosts();

        if (extraCosts is not null)
            _extraCosts.AddRange(extraCosts);

        return this;
    }

    public void ClearExtraCosts() => _extraCosts.Clear();

    #endregion

    #region Discounts

    private readonly List<InvoiceDiscount> _discounts = [];
    public IReadOnlyList<InvoiceDiscount> Discounts => _discounts;

    public Invoice SetDiscounts(IEnumerable<InvoiceDiscount>? discounts)
    {
        ClearDiscounts();

        if (discounts is not null)
            _discounts.AddRange(discounts);

        return this;
    }

    public void ClearDiscounts() => _discounts.Clear();

    #endregion

    #region Unit

    public PriceUnit? PriceUnit { get; private set; }
    public PriceUnitId PriceUnitId { get; private set; }

    public void SetPriceUnitId(PriceUnitId priceUnitId)
    {
        PriceUnitId = priceUnitId;
    }

    public PriceUnit? UnpaidPriceUnit { get; private set; }
    public PriceUnitId? UnpaidPriceUnitId { get; private set; }

    public void SetUnpaidPriceUnitId(PriceUnitId? priceUnitId)
    {
        UnpaidPriceUnitId = priceUnitId;
    }

    public decimal? UnpaidAmountExchangeRate { get; set; }

    public void SetUnpaidAmountExchangeRate(decimal? exchangeRate)
    {
        UnpaidAmountExchangeRate = exchangeRate;
    }

    #endregion

    #region Transactions

    public IReadOnlyList<Transaction>? Transactions { get; private set; }

    #endregion

    #region Calculations

    public decimal TotalTaxAmount =>
        ProductItems.Sum(item => item.ItemTaxAmount) +
        CurrencyItems.Sum(item => item.ItemTaxAmount);

    public decimal TotalAmount => 
        ProductItems.Sum(item => item.ItemFinalAmount) +
        CoinItems.Sum(item => item.TotalAmount) +
        CurrencyItems.Sum(item => item.TotalAmount);

    public decimal TotalWageAmount => 
        ProductItems.Sum(item => item.ItemWageAmount);

    public decimal TotalProfitAmount =>
        ProductItems.Sum(item => item.ItemProfitAmount) +
        CoinItems.Sum(item => item.ItemProfitAmount) +
        CurrencyItems.Sum(item => item.ItemProfitAmount);

    public decimal TotalRawAmount => 
        ProductItems.Sum(item => item.ItemRawAmount) +
        CoinItems.Sum(item => item.ItemRawAmount) +
        CurrencyItems.Sum(item => item.ItemRawAmount);

    public decimal TotalPaidAmount => InvoicePayments?.Sum(payment => payment.Amount * (payment.ExchangeRate ?? 1)) ?? 0;

    public decimal TotalDiscountAmount => Discounts.Sum(discount => discount.Amount * (discount.ExchangeRate ?? 1));

    public decimal TotalExtraCostAmount => ExtraCosts.Sum(extraCost => extraCost.Amount * (extraCost.ExchangeRate ?? 1));

    public decimal TotalAmountWithDiscountsAndExtraCosts => TotalAmount - TotalDiscountAmount + TotalExtraCostAmount;

    public decimal TotalUnpaidAmount => TotalAmountWithDiscountsAndExtraCosts - TotalPaidAmount;

    #endregion
}
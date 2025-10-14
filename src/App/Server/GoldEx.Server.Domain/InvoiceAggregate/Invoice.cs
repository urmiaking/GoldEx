using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.NotificationAggregate;
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

    public void SetInvoiceNumber(long invoiceNumber)
    {
        if (InvoiceNumber != invoiceNumber)
            InvoiceNumber = invoiceNumber;
    }

    public void SetDueDate(DateOnly? dueDate)
    {
        if (DueDate != dueDate)
            DueDate = dueDate;
    }

    public void SetInvoiceDate(DateOnly invoiceDate)
    {
        if (InvoiceDate != invoiceDate)
            InvoiceDate = invoiceDate;
    }

    public void SetExchangeRate(decimal? exchangeRate)
    {
        if (ExchangeRate != exchangeRate)
            ExchangeRate = exchangeRate;
    }

    #region Customer

    public CustomerId CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public Invoice SetCustomerId(CustomerId customerId)
    {
        if (CustomerId != customerId)
            CustomerId = customerId;

        return this;
    }

    #endregion

    #region InvoiceItems

    #region ProductItems

    private readonly List<InvoiceProductItem> _products = [];
    public IReadOnlyList<InvoiceProductItem> ProductItems => _products;

    public void AddPurchaseProductItem(InvoiceProductItemId? id,
        decimal gramPrice,
        decimal profitPercent,
        decimal taxPercent,
        int quantity,
        decimal? costPrice,
        decimal? costPriceExchangeRate,
        decimal? stonePriceUnitExchangeRate,
        PriceUnitId? costPriceUnitId,
        bool isInstantProduct,
        Product product)
    {
        _products.Add(InvoiceProductItem.CreatePurchaseItem(id,
            gramPrice,
            profitPercent,
            taxPercent,
            quantity,
            product.Id,
            costPrice,
            costPriceExchangeRate,
            stonePriceUnitExchangeRate,
            costPriceUnitId,
            isInstantProduct)
            .SetInvoice(this)
            .RecalculateAmounts(product, InvoiceType));
    }

    public void AddSaleProductItem(InvoiceProductItemId? id,
        decimal gramPrice,
        decimal profitPercent,
        decimal taxPercent,
        int quantity,
        decimal? costPrice,
        decimal? costPriceExchangeRate,
        PriceUnitId? costPriceUnitId,
        bool isInstantProduct,
        decimal saleWage,
        WageType? saleWageType,
        PriceUnitId? saleWagePriceUnitId,
        decimal? saleWagePriceUnitExchangeRate,
        decimal? stonePriceUnitExchangeRate,
        Product product)
    {
        _products.Add(InvoiceProductItem.CreateSaleItem(id,
            gramPrice,
            profitPercent,
            taxPercent,
            quantity,
            product.Id,
            costPrice,
            costPriceExchangeRate,
            costPriceUnitId,
            isInstantProduct,
            saleWage,
            saleWageType,
            saleWagePriceUnitId,
            saleWagePriceUnitExchangeRate,
            stonePriceUnitExchangeRate)
            .SetInvoice(this)
            .RecalculateAmounts(product, InvoiceType));
    }

    public void RemoveProductItem(InvoiceProductItem productItem) => _products.Remove(productItem);

    public void ClearProductItems() => _products.Clear();

    #endregion

    #region CoinItems

    private readonly List<InvoiceCoinItem> _coins = [];
    public IReadOnlyList<InvoiceCoinItem> CoinItems => _coins;

    public void AddCoinItem(InvoiceCoinItemId? id,
        CoinId coinId,
        decimal unitPrice,
        int quantity,
        decimal profitPercent)
    {
        _coins.Add(InvoiceCoinItem.Create(id, coinId, unitPrice, quantity, profitPercent));
    }

    public void ClearCoinItems() => _coins.Clear();

    #endregion

    #region Currencies

    private readonly List<InvoiceCurrencyItem> _currencies = [];
    public IReadOnlyList<InvoiceCurrencyItem> CurrencyItems => _currencies;

    public void AddCurrencyItem(InvoiceCurrencyItemId? id,
        PriceUnitId currencyId,
        decimal unitPrice,
        decimal amount,
        decimal taxPercent,
        decimal profitPercent)
    {
        _currencies.Add(InvoiceCurrencyItem.Create(id, currencyId, unitPrice, amount, taxPercent, profitPercent));
    }

    public void ClearCurrencyItems() => _currencies.Clear();

    #endregion

    #region UsedProducts

    private readonly List<InvoiceUsedProduct> _usedProducts = [];
    public IReadOnlyList<InvoiceUsedProduct> UsedProducts => _usedProducts;

    public void AddUsedProduct(InvoiceUsedProductId? id,
        string description,
        decimal weight,
        decimal gramPrice,
        decimal? extraCostsAmount,
        decimal finenessDeductionRate,
        int quantity,
        bool isBroken,
        ProductType productType,
        GoldUnitType unitType,
        ProductId? productId)
    {
        if (!productId.HasValue)
        {
            if (_usedProducts.Any(x => x.ProductId == productId))
                throw new InvalidOperationException(
                    $"The used product with ID {productId?.Value} is already present in the UsedProducts list");
        }
        
        _usedProducts.Add(InvoiceUsedProduct.Create(id,
            description,
            weight,
            gramPrice,
            extraCostsAmount,
            finenessDeductionRate,
            quantity,
            isBroken,
            productType,
            unitType,
            productId,
            this));
    }

    public void ClearUsedProducts() => _usedProducts.Clear();

    #endregion

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
        if (PriceUnitId != priceUnitId) 
            PriceUnitId = priceUnitId;
    }

    public PriceUnit? UnpaidPriceUnit { get; private set; }
    public PriceUnitId? UnpaidPriceUnitId { get; private set; }

    public void SetUnpaidPriceUnitId(PriceUnitId? priceUnitId)
    {
        if (UnpaidPriceUnitId != priceUnitId)
            UnpaidPriceUnitId = priceUnitId;
    }

    public decimal? UnpaidAmountExchangeRate { get; set; }

    public void SetUnpaidAmountExchangeRate(decimal? exchangeRate)
    {
        if (UnpaidAmountExchangeRate != exchangeRate)
            UnpaidAmountExchangeRate = exchangeRate;
    }

    #endregion

    #region Transactions

    public IReadOnlyList<Transaction>? Transactions { get; private set; }

    #endregion

    #region Notifications

    public IReadOnlyList<Notification>? Notifications { get; private set; }

    #endregion

    #region Calculations

    public decimal TotalTaxAmount =>
        ProductItems.Sum(item => item.ItemTaxAmount) +
        CurrencyItems.Sum(item => item.ItemTaxAmount);

    public decimal TotalStoneAmount => ProductItems.Sum(item => item.ItemStoneAmount);

    public decimal TotalAmount =>
        ProductItems.Sum(item => item.ItemFinalAmount * (item.CostPriceExchangeRate ?? 1)) + TotalStoneAmount +
        CoinItems.Sum(item => item.ItemFinalAmount) +
        CurrencyItems.Sum(item => item.ItemFinalAmount) +
        UsedProducts.Sum(item => item.ItemFinalAmount);

    public decimal TotalWageAmount =>
        ProductItems.Sum(item => item.ItemWageAmount);

    public decimal TotalProfitAmount =>
        ProductItems.Sum(item => item.ItemProfitAmount) +
        CoinItems.Sum(item => item.ItemProfitAmount) +
        CurrencyItems.Sum(item => item.ItemProfitAmount);

    public decimal TotalRawAmount =>
        ProductItems.Sum(item => item.ItemRawAmount) +
        CoinItems.Sum(item => item.ItemRawAmount) +
        CurrencyItems.Sum(item => item.ItemRawAmount) +
        (InvoiceType is InvoiceType.Purchase ? TotalUsedProductsAmount : 0);

    public decimal TotalPaidAmount => InvoicePayments?.Sum(payment => payment.Amount * (payment.ExchangeRate ?? 1)) ?? 0;

    public decimal TotalDiscountAmount => Discounts.Sum(discount => discount.Amount * (discount.ExchangeRate ?? 1));

    public decimal TotalExtraCostAmount => ExtraCosts.Sum(extraCost => extraCost.Amount * (extraCost.ExchangeRate ?? 1));

    public decimal TotalAmountWithDiscountsAndExtraCosts => TotalAmount - TotalDiscountAmount + TotalExtraCostAmount;

    public decimal TotalUnpaidAmount => TotalAmountWithDiscountsAndExtraCosts - TotalPaidAmount - (InvoiceType is InvoiceType.Sell ? TotalUsedProductsAmount : 0);

    public decimal TotalUsedProductsAmount => UsedProducts.Sum(usedProduct => usedProduct.ItemFinalAmount);

    #endregion
}
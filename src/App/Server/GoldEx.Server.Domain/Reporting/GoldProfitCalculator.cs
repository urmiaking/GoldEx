using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.Reporting;

/// <summary>Historical document values only; never uses the live gold price or current product purity.</summary>
public record GoldProfitSource
{
    public Guid ItemId { get; init; }
    public Guid ProductId { get; init; }
    public Guid InvoiceId { get; init; }
    public long InvoiceNumber { get; init; }
    public DateOnly Date { get; init; }
    public DateTime CreatedAt { get; init; }
    public InvoiceType InvoiceType { get; init; }
    public string CustomerName { get; init; } = "";
    public Guid CustomerId { get; init; }
    public string ProductName { get; init; } = "";
    public string Barcode { get; init; } = "";
    public Guid? CategoryId { get; init; }
    public ProductType ProductType { get; init; }
    public decimal Weight { get; init; }
    public decimal GramPrice { get; init; }
    public decimal RawAmount { get; init; }
    public decimal Amount { get; init; }
    public decimal Tax { get; init; }
    public decimal InvoiceAmount { get; init; }
    public decimal InvoiceDiscount { get; init; }
    public Guid PriceUnitId { get; init; }
    public UnitType? UnitType { get; init; }
    public Guid BasePriceUnitId { get; init; }
    public UnitType? BaseUnitType { get; init; }
    public string BasePriceUnit { get; init; } = "";
    public decimal? ExchangeRate { get; init; }
    public bool IsInstant { get; init; }

    public decimal NetAmount => InvoiceAmount > 0
        ? Amount - Tax - InvoiceDiscount * Amount / InvoiceAmount
        : 0;
}

public record GoldProfitResult(decimal NetSales, decimal PurchaseCost, decimal GrossProfit,
    decimal GoldPriceEffect, decimal ExchangeRateEffect, decimal TradingMargin);

public static class GoldProfitCalculator
{
    public static GoldProfitResult? Calculate(GoldProfitSource sale, GoldProfitSource purchase,
        out string? incompleteReason)
    {
        incompleteReason = null;
        if (sale.Weight <= 0 || purchase.Weight <= 0 || sale.GramPrice <= 0 ||
            purchase.GramPrice <= 0 || sale.RawAmount <= 0 || sale.InvoiceAmount <= 0 ||
            purchase.InvoiceAmount <= 0 || purchase.Amount <= 0 || sale.InvoiceDiscount < 0 ||
            purchase.InvoiceDiscount < 0 || sale.InvoiceDiscount > sale.InvoiceAmount ||
            purchase.InvoiceDiscount > purchase.InvoiceAmount || sale.Amount < 0 || sale.Tax < 0 ||
            sale.Tax > sale.Amount || purchase.Tax < 0 || purchase.Tax > purchase.Amount)
            return Incomplete("مبلغ، وزن یا نرخ تاریخی خرید و فروش کامل نیست.", out incompleteReason);

        if (sale.InvoiceType != InvoiceType.Sell || purchase.InvoiceType != InvoiceType.Purchase ||
            sale.ProductId != purchase.ProductId || purchase.Date > sale.Date || sale.Weight > purchase.Weight)
            return Incomplete("سند خرید با فروش مطابقت ندارد.", out incompleteReason);

        var baseFactor = UnitFactor(purchase.BasePriceUnitId, purchase.BaseUnitType,
            sale.BasePriceUnitId, sale.BaseUnitType);
        var saleRate = HistoricalRate(sale);
        var purchaseRate = HistoricalRate(purchase);
        if (baseFactor is null || saleRate is null || purchaseRate is null)
            return Incomplete("واحد یا نرخ تبدیل تاریخی قابل مقایسه نیست؛ تبدیل با قیمت روز انجام نمی‌شود.", out incompleteReason);

        var equivalentWeight = sale.RawAmount / sale.GramPrice;
        if (IsGold(purchase.UnitType) || IsGold(sale.UnitType))
        {
            // A weight-denominated purchase is valued using its own saved conversion rate,
            // exactly as historical COGS is translated. Never substitute the current gold rate.
            var historicalPurchaseFx = purchaseRate.Value * baseFactor.Value;
            var netSales = sale.NetAmount * saleRate.Value;
            var purchaseCost = purchase.NetAmount * sale.Weight / purchase.Weight * historicalPurchaseFx;
            var goldMovement = equivalentWeight *
                (sale.GramPrice * saleRate.Value - purchase.GramPrice * historicalPurchaseFx);
            var grossProfit = netSales - purchaseCost;
            return new(netSales, purchaseCost, grossProfit, goldMovement, 0m, grossProfit - goldMovement);
        }

        var currencyFactor = UnitFactor(purchase.PriceUnitId, purchase.UnitType, sale.PriceUnitId, sale.UnitType);
        if (currencyFactor is null)
            return Incomplete("واحد یا نرخ تبدیل تاریخی قابل مقایسه نیست؛ تبدیل با قیمت روز انجام نمی‌شود.", out incompleteReason);
        var purchaseCostInSaleCurrency = purchase.NetAmount * sale.Weight / purchase.Weight * currencyFactor.Value;
        var purchaseFx = purchaseRate.Value * baseFactor.Value / currencyFactor.Value;
        var saleNet = sale.NetAmount * saleRate.Value;
        var cost = purchaseCostInSaleCurrency * purchaseFx;
        // Convention: gold movement is translated at sale FX; FX movement applies to acquisition cost.
        var goldEffect = equivalentWeight * (sale.GramPrice - purchase.GramPrice * currencyFactor.Value) * saleRate.Value;
        var fxEffect = purchaseCostInSaleCurrency * (saleRate.Value - purchaseFx);
        var gross = saleNet - cost;
        return new(saleNet, cost, gross, goldEffect, fxEffect, gross - goldEffect - fxEffect);
    }

    private static GoldProfitResult? Incomplete(string reason, out string? incompleteReason)
    {
        incompleteReason = reason;
        return null;
    }

    private static decimal? HistoricalRate(GoldProfitSource source)
    {
        if (IsGold(source.BaseUnitType)) return null;
        if (IsGold(source.UnitType)) return source.ExchangeRate is > 0 ? source.ExchangeRate : null;
        var known = UnitFactor(source.PriceUnitId, source.UnitType, source.BasePriceUnitId, source.BaseUnitType);
        return known ?? (source.ExchangeRate is > 0 ? source.ExchangeRate : null);
    }

    private static decimal? UnitFactor(Guid fromId, UnitType? from, Guid toId, UnitType? to)
    {
        if (IsGold(from) || IsGold(to)) return null;
        if (fromId == toId || (from.HasValue && from == to)) return 1m;
        if (from == UnitType.IRR && to == UnitType.TMN) return 0.1m;
        if (from == UnitType.TMN && to == UnitType.IRR) return 10m;
        return null;
    }

    private static bool IsGold(UnitType? unit) => unit is UnitType.Gold18K or UnitType.Mesghal;
}

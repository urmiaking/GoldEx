using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.Reporting;
using GoldEx.Server.Domain.Reporting;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public class ProfitAnalysisTester
{
    private static readonly Guid Toman = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();
    private static readonly DateOnly BuyDate = new(2026, 10, 1);
    private static readonly DateOnly SellDate = new(2026, 10, 20);

    private static GoldProfitSource Buy() => new()
    {
        ItemId = Guid.NewGuid(), ProductId = Product, InvoiceId = Guid.NewGuid(), InvoiceNumber = 1,
        Date = BuyDate, CreatedAt = BuyDate.ToDateTime(TimeOnly.MinValue), InvoiceType = InvoiceType.Purchase,
        Weight = 10m, GramPrice = 6000000m, RawAmount = 62000000m, Amount = 62000000m, InvoiceAmount = 62000000m,
        PriceUnitId = Toman, UnitType = UnitType.TMN, BasePriceUnitId = Toman, BaseUnitType = UnitType.TMN, BasePriceUnit = "تومان"
    };

    private static GoldProfitSource Sell() => Buy() with
    {
        ItemId = Guid.NewGuid(), InvoiceId = Guid.NewGuid(), InvoiceNumber = 2,
        Date = SellDate, CreatedAt = SellDate.ToDateTime(TimeOnly.MinValue), InvoiceType = InvoiceType.Sell,
        GramPrice = 7000000m, RawAmount = 70000000m, Amount = 75000000m, Tax = 1000000m, InvoiceAmount = 75000000m
    };

    private static ProfitAnalysisRpRequest Request() => new(new(2026, 10, 1), new(2026, 10, 31));

    [Test]
    public void Example_SeparatesGoldAndTrade_AndExcludesTax()
    {
        var result = GoldProfitCalculator.Calculate(Sell(), Buy(), out var reason);
        Assert.That(reason, Is.Null);
        Assert.That(result, Is.EqualTo(new GoldProfitResult(74000000m, 62000000m, 12000000m, 10000000m, 0m, 2000000m)));
    }

    [Test]
    public void PartialSale_AllocatesPurchaseCostAndBothInvoiceDiscounts()
    {
        var buy = Buy() with { InvoiceAmount = 124000000m, InvoiceDiscount = 12400000m };
        var sale = Sell() with { Weight = 4m, RawAmount = 28000000m, Amount = 30000000m,
            Tax = 1000000m, InvoiceAmount = 60000000m, InvoiceDiscount = 6000000m };
        var result = GoldProfitCalculator.Calculate(sale, buy, out _)!;
        Assert.That(result.NetSales, Is.EqualTo(26000000m));
        Assert.That(result.PurchaseCost, Is.EqualTo(22320000m));
        Assert.That(result.GoldPriceEffect, Is.EqualTo(4000000m));
        Assert.That(result.TradingMargin, Is.EqualTo(-320000m));
    }

    [Test]
    public void FallingGoldPrice_PreservesLossAndIdentity()
    {
        var result = GoldProfitCalculator.Calculate(Sell() with { GramPrice = 5000000m,
            RawAmount = 50000000m, Amount = 54000000m, Tax = 0m, InvoiceAmount = 54000000m }, Buy(), out _)!;
        Assert.That(result.GoldPriceEffect, Is.EqualTo(-10000000m));
        Assert.That(result.GrossProfit, Is.EqualTo(-8000000m));
        Assert.That(result.TradingMargin + result.GoldPriceEffect + result.ExchangeRateEffect, Is.EqualTo(result.GrossProfit));
    }

    [Test]
    public void ForeignCurrency_SeparatesHistoricalFxFromGold()
    {
        var usd = Guid.NewGuid();
        var buy = Buy() with { PriceUnitId = usd, UnitType = UnitType.USD, ExchangeRate = 60000m,
            GramPrice = 60m, Amount = 620m, InvoiceAmount = 620m };
        var sale = Sell() with { PriceUnitId = usd, UnitType = UnitType.USD, ExchangeRate = 70000m,
            GramPrice = 70m, RawAmount = 700m, Amount = 740m, Tax = 0m, InvoiceAmount = 740m };
        var result = GoldProfitCalculator.Calculate(sale, buy, out _)!;
        Assert.That(result.GoldPriceEffect, Is.EqualTo(7000000m));
        Assert.That(result.ExchangeRateEffect, Is.EqualTo(6200000m));
        Assert.That(result.TradingMargin, Is.EqualTo(1400000m));
        Assert.That(result.TradingMargin + result.GoldPriceEffect + result.ExchangeRateEffect, Is.EqualTo(result.GrossProfit));
    }

    [Test]
    public void RialPurchaseAndTomanSale_UseExactUnitConversion()
    {
        var rial = Guid.NewGuid();
        var buy = Buy() with { PriceUnitId = rial, UnitType = UnitType.IRR,
            BasePriceUnitId = rial, BaseUnitType = UnitType.IRR, GramPrice = 60000000m,
            Amount = 620000000m, InvoiceAmount = 620000000m };
        var result = GoldProfitCalculator.Calculate(Sell(), buy, out _)!;
        Assert.That(result.PurchaseCost, Is.EqualTo(62000000m));
        Assert.That(result.GoldPriceEffect, Is.EqualTo(10000000m));
        Assert.That(result.ExchangeRateEffect, Is.Zero);
    }

    [Test]
    public void MissingForeignExchangeRate_IsIncompleteRatherThanOne()
    {
        var sale = Sell() with { UnitType = UnitType.USD, PriceUnitId = Guid.NewGuid(), ExchangeRate = null };
        Assert.That(GoldProfitCalculator.Calculate(sale, Buy(), out var reason), Is.Null);
        Assert.That(reason, Is.Not.Empty);
    }

    [Test]
    public void GoldBasedInvoice_IsNotReportedAsMoney()
    {
        var sale = Sell() with { UnitType = UnitType.Gold18K };
        Assert.That(GoldProfitCalculator.Calculate(sale, Buy(), out _), Is.Null);
    }

    [Test]
    public void GramPurchase_UsesRecordedPurchaseDateRate_NotSaleOrLiveRate()
    {
        var purchase = Buy() with { PriceUnitId = Guid.NewGuid(), UnitType = UnitType.Gold18K,
            GramPrice = 1m, Amount = 10.333333333333333333333333333m,
            InvoiceAmount = 10.333333333333333333333333333m, ExchangeRate = 6000000m };
        var result = GoldProfitCalculator.Calculate(Sell(), purchase, out var reason)!;
        Assert.That(reason, Is.Null);
        Assert.That(result.PurchaseCost, Is.EqualTo(62000000m).Within(0.000001m));
        Assert.That(result.GoldPriceEffect, Is.EqualTo(10000000m));
        Assert.That(result.TradingMargin, Is.EqualTo(2000000m).Within(0.000001m));
        Assert.That(result.ExchangeRateEffect, Is.Zero);
    }

    [Test]
    public void GramSaleWithHistoricalRate_IsTranslatedToItsMoneyBase()
    {
        var sale = Sell() with { PriceUnitId = Guid.NewGuid(), UnitType = UnitType.Gold18K,
            GramPrice = 1m, RawAmount = 10m, Amount = 10.5m, Tax = 0m, InvoiceAmount = 10.5m, ExchangeRate = 7000000m };
        var result = GoldProfitCalculator.Calculate(sale, Buy(), out var reason)!;
        Assert.That(reason, Is.Null);
        Assert.That(result.NetSales, Is.EqualTo(73500000m));
        Assert.That(result.GoldPriceEffect, Is.EqualTo(10000000m));
        Assert.That(result.TradingMargin, Is.EqualTo(1500000m));
    }

    [Test]
    public void GramPurchaseWithoutHistoricalRate_RemainsIncomplete()
    {
        var purchase = Buy() with { PriceUnitId = Guid.NewGuid(), UnitType = UnitType.Gold18K,
            GramPrice = 1m, Amount = 11m, InvoiceAmount = 11m, ExchangeRate = null };
        Assert.That(GoldProfitCalculator.Calculate(Sell(), purchase, out _), Is.Null);
    }

    [Test]
    public void DifferentForeignUnits_AreNotConvertedWithLiveRates()
    {
        var sale = Sell() with { UnitType = UnitType.EUR, PriceUnitId = Guid.NewGuid(), ExchangeRate = 70000m };
        var buy = Buy() with { UnitType = UnitType.USD, PriceUnitId = Guid.NewGuid(), ExchangeRate = 60000m };
        Assert.That(GoldProfitCalculator.Calculate(sale, buy, out _), Is.Null);
    }

    [Test]
    public void InvalidCostAndMissingHistoricalGoldRate_AreIncomplete()
    {
        Assert.That(GoldProfitCalculator.Calculate(Sell(), Buy() with { Amount = 0 }, out _), Is.Null);
        Assert.That(GoldProfitCalculator.Calculate(Sell(), Buy() with { GramPrice = 0 }, out _), Is.Null);
        Assert.That(GoldProfitCalculator.Calculate(Sell() with { RawAmount = 0 }, Buy(), out _), Is.Null);
    }

    [Test]
    public void MultiplePurchases_AreFlaggedRatherThanPickingLatestInvoice()
    {
        var result = ProfitAnalysisBuilder.Build([Buy(), Buy(), Sell()], new HashSet<Guid>(), Request());
        Assert.That(result.IncompleteCount, Is.EqualTo(1));
        Assert.That(result.Summaries, Is.Empty);
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public void FuturePurchase_DoesNotBecomeSaleCost()
    {
        var result = ProfitAnalysisBuilder.Build([Buy() with { Date = SellDate.AddDays(1) }, Sell()], new HashSet<Guid>(), Request());
        Assert.That(result.IncompleteCount, Is.EqualTo(1));
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public void SameDayPurchaseCreatedAfterSale_IsNotAnOrigin()
    {
        var sale = Sell();
        var result = ProfitAnalysisBuilder.Build([Buy() with { Date = sale.Date, CreatedAt = sale.CreatedAt.AddHours(1) }, sale], new HashSet<Guid>(), Request());
        Assert.That(result.CalculatedCount, Is.Zero);
    }

    [Test]
    public void PriorSalesOutsideWindowAndCustomerFilter_StillConsumeWeight()
    {
        var customer = Guid.NewGuid();
        var prior = Sell() with { Date = SellDate.AddDays(-2), Weight = 6m, CustomerId = Guid.NewGuid() };
        var sale = Sell() with { Weight = 5m, CustomerId = customer };
        var result = ProfitAnalysisBuilder.Build([Buy(), prior, sale], new HashSet<Guid>(),
            Request() with { FromDate = SellDate.ToDateTime(TimeOnly.MinValue), CustomerId = customer });
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.CalculatedCount, Is.Zero);
    }

    [Test]
    public void TwoPartialSales_AllocateCostOnce()
    {
        var sale1 = Sell() with { Weight = 4m, RawAmount = 28000000m, Amount = 30000000m, Tax = 400000m, InvoiceAmount = 30000000m };
        var sale2 = Sell() with { Weight = 6m, RawAmount = 42000000m, Amount = 45000000m, Tax = 600000m, InvoiceAmount = 45000000m };
        var result = ProfitAnalysisBuilder.Build([Buy(), sale1, sale2], new HashSet<Guid>(), Request());
        Assert.That(result.CalculatedCount, Is.EqualTo(2));
        Assert.That(result.Summaries.Single().PurchaseCost, Is.EqualTo(62000000m));
    }

    [Test]
    public void OtherStockMovements_AreExplicitlyExcluded()
    {
        var result = ProfitAnalysisBuilder.Build([Buy(), Sell()], new HashSet<Guid> { Product }, Request());
        Assert.That(result.IncompleteCount, Is.EqualTo(1));
        Assert.That(result.Summaries, Is.Empty);
        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public void CompleteReportAndSummaryOnly_HaveIdenticalTotals()
    {
        var sale1 = Sell() with { Weight = 5m };
        var sale2 = Sell() with { Weight = 5m };
        var history = new[] { Buy(), sale1, sale2 };
        var report = ProfitAnalysisBuilder.Build(history, new HashSet<Guid>(), Request());
        var summary = ProfitAnalysisBuilder.Build(history, new HashSet<Guid>(), Request() with { SummaryOnly = true });
        Assert.That(report.Items, Has.Count.EqualTo(2));
        Assert.That(summary.Items, Is.Empty);
        Assert.That(report.Summaries, Is.EqualTo(summary.Summaries));
        Assert.That(report.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public void Validator_AllowsUnboundedAndLongRangesButRejectsReversedDatesAndUnsupportedProductType()
    {
        var validator = new ProfitAnalysisRpRequestValidator();
        Assert.That(validator.Validate(Request()).IsValid, Is.True);
        Assert.That(validator.Validate(Request() with { FromDate = new(2026, 11, 1) }).IsValid, Is.False);
        Assert.That(validator.Validate(Request() with { FromDate = new(2024, 1, 1) }).IsValid, Is.True);
        Assert.That(validator.Validate(new ProfitAnalysisRpRequest()).IsValid, Is.True);
        Assert.That(validator.Validate(Request() with { FromDate = null }).IsValid, Is.True);
        Assert.That(validator.Validate(Request() with { ToDate = null }).IsValid, Is.True);
        Assert.That(validator.Validate(Request() with { ToDate = DateTime.MaxValue }).IsValid, Is.True);
        Assert.That(validator.Validate(Request() with { ProductType = ProductType.UsedGold }).IsValid, Is.False);
    }

    [Test]
    public void MissingOrigin_DoesNotPolluteValidTotals()
    {
        var valid = Sell();
        var missing = Sell() with { ProductId = Guid.NewGuid() };
        var report = ProfitAnalysisBuilder.Build([Buy(), valid, missing], new HashSet<Guid>(), Request());
        Assert.That(report.CalculatedCount, Is.EqualTo(1));
        Assert.That(report.IncompleteCount, Is.EqualTo(1));
        Assert.That(report.Summaries.Single().GrossProfit, Is.EqualTo(12000000m));
        Assert.That(report.Items.Single().InvoiceId, Is.EqualTo(valid.InvoiceId));
    }

    [Test]
    public void FullReport_ReturnsEveryCalculatedSaleBeyondFormerPageLimit()
    {
        var history = Enumerable.Range(1, 125).SelectMany(number =>
        {
            var productId = Guid.NewGuid();
            return new[] { Buy() with { ProductId = productId }, Sell() with { ProductId = productId, InvoiceNumber = number } };
        }).ToArray();

        var report = ProfitAnalysisBuilder.Build(history, new HashSet<Guid>(), Request());

        Assert.That(report.Items, Has.Count.EqualTo(125));
        Assert.That(report.Items.Select(x => x.InvoiceNumber), Is.EqualTo(Enumerable.Range(1, 125).Reverse().Select(x => (long)x)));
        Assert.That(report.Items.All(x => x.IncompleteReason is null), Is.True);
        Assert.That(report.CalculatedCount, Is.EqualTo(125));
        Assert.That(report.Summaries.Single().GrossProfit, Is.EqualTo(125 * 12000000m));
    }

    [Test]
    public void InstantSale_IsOmittedEvenWhenPurchaseInformationExists()
    {
        var report = ProfitAnalysisBuilder.Build([Buy(), Sell() with { IsInstant = true }], new HashSet<Guid>(), Request());

        Assert.That(report.Items, Is.Empty);
        Assert.That(report.CalculatedCount, Is.Zero);
        Assert.That(report.IncompleteCount, Is.EqualTo(1));
        Assert.That(report.Summaries, Is.Empty);
    }

    [TestCase(null, null, 2)]
    [TestCase("2026-10-01", null, 1)]
    [TestCase(null, "2024-12-31", 1)]
    [TestCase("2024-01-01", "2026-10-31", 2)]
    public void OptionalDates_IncludeAllHistoryOrOnlyTheSpecifiedBounds(string? start, string? end, int expectedCount)
    {
        var olderProduct = Guid.NewGuid();
        var olderBuy = Buy() with { ProductId = olderProduct, Date = new(2024, 5, 1), CreatedAt = new(2024, 5, 1) };
        var olderSale = Sell() with { ProductId = olderProduct, Date = new(2024, 5, 20), CreatedAt = new(2024, 5, 20) };
        var request = new ProfitAnalysisRpRequest(
            start is null ? null : DateTime.Parse(start, System.Globalization.CultureInfo.InvariantCulture),
            end is null ? null : DateTime.Parse(end, System.Globalization.CultureInfo.InvariantCulture));

        var report = ProfitAnalysisBuilder.Build([olderBuy, olderSale, Buy(), Sell()], new HashSet<Guid>(), request);

        Assert.That(report.Items, Has.Count.EqualTo(expectedCount));
        Assert.That(report.Summaries.Single().GrossProfit, Is.EqualTo(expectedCount * 12000000m));
    }

    [Test]
    public void DistinctBaseCurrencies_AreNeverSummedTogether()
    {
        var foreignProduct = Guid.NewGuid();
        var dollar = Guid.NewGuid();
        var secondBuy = Buy() with { ProductId = foreignProduct, UnitType = UnitType.USD,
            PriceUnitId = dollar, BasePriceUnitId = dollar, BaseUnitType = UnitType.USD, BasePriceUnit = "دلار" };
        var secondSale = Sell() with { ProductId = foreignProduct, UnitType = UnitType.USD,
            PriceUnitId = dollar, BasePriceUnitId = dollar, BaseUnitType = UnitType.USD, BasePriceUnit = "دلار" };
        var result = ProfitAnalysisBuilder.Build([Buy(), Sell(), secondBuy, secondSale], new HashSet<Guid>(), Request());
        Assert.That(result.Summaries, Has.Count.EqualTo(2));
        Assert.That(result.Summaries.All(s => s.GrossProfit == 12000000m), Is.True);
    }

    [Test]
    public void DifferentUnspecifiedCurrencies_AreNotAssumedEquivalent()
    {
        var buy = Buy() with { UnitType = null, PriceUnitId = Guid.NewGuid(), ExchangeRate = 2m };
        var sale = Sell() with { UnitType = null, PriceUnitId = Guid.NewGuid(), ExchangeRate = 3m };
        Assert.That(GoldProfitCalculator.Calculate(sale, buy, out _), Is.Null);
    }
}

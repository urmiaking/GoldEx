using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Tests.Server.Domain;

[TestFixture]
public class InvoiceCoinItemTests
{
    [TestCase(252_000_000, 1, 0, 0, 252_000_000)]
    [TestCase(120_000_000, 2, 5, 12_000_000, 252_000_000)]
    [TestCase(100_000_000, 3, 2.5, 7_500_000, 307_500_000)]
    public void UpdatingCoinRecalculatesAmountsAndInvoiceBalance(
        decimal unitPrice, int quantity, decimal profitPercent, decimal expectedProfit, decimal expectedTotal)
    {
        var invoice = CreateSaleWithCoin(251_335_000);
        var item = invoice.CoinItems.Single();
        var existingId = item.Id;
        var coinId = new CoinInstanceId(Guid.NewGuid());

        item.Update(coinId, unitPrice, quantity, profitPercent);

        Assert.Multiple(() =>
        {
            Assert.That(item.Id, Is.EqualTo(existingId));
            Assert.That(item.CoinInstanceId, Is.EqualTo(coinId));
            Assert.That(item.UnitPrice, Is.EqualTo(unitPrice));
            Assert.That(item.Quantity, Is.EqualTo(quantity));
            Assert.That(item.ProfitPercent, Is.EqualTo(profitPercent));
            Assert.That(item.ItemRawAmount, Is.EqualTo(unitPrice));
            Assert.That(item.ItemProfitAmount, Is.EqualTo(expectedProfit));
            Assert.That(item.ItemFinalAmount, Is.EqualTo(expectedTotal));
            Assert.That(invoice.TotalAmount, Is.EqualTo(expectedTotal));
            Assert.That(invoice.TotalUnpaidAmount, Is.EqualTo(expectedTotal));
        });
    }

    [TestCase(0, 1, 0)]
    [TestCase(100, 0, 0)]
    [TestCase(100, 1, -1)]
    [TestCase(100, 1, 101)]
    public void InvalidCoinUpdateDoesNotChangeStoredAmounts(
        decimal unitPrice, int quantity, decimal profitPercent)
    {
        var invoice = CreateSaleWithCoin(251_335_000);
        var item = invoice.CoinItems.Single();
        var originalCoinId = item.CoinInstanceId;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            item.Update(new CoinInstanceId(Guid.NewGuid()), unitPrice, quantity, profitPercent));

        Assert.Multiple(() =>
        {
            Assert.That(item.CoinInstanceId, Is.EqualTo(originalCoinId));
            Assert.That(item.UnitPrice, Is.EqualTo(251_335_000));
            Assert.That(item.Quantity, Is.EqualTo(1));
            Assert.That(item.ProfitPercent, Is.Zero);
            Assert.That(item.ItemFinalAmount, Is.EqualTo(251_335_000));
            Assert.That(invoice.TotalUnpaidAmount, Is.EqualTo(251_335_000));
        });
    }

    private static Invoice CreateSaleWithCoin(decimal unitPrice)
    {
        var priceUnitId = new PriceUnitId(Guid.NewGuid());
        var invoice = Invoice.Create(
            1817, null, null, InvoiceType.Sell, TradeScale.Retail,
            new CustomerId(Guid.NewGuid()), priceUnitId, priceUnitId, null,
            new DateOnly(2026, 11, 16), null);

        invoice.AddCoinItem(null, new CoinInstanceId(Guid.NewGuid()), unitPrice, 1, 0, true);
        return invoice;
    }
}

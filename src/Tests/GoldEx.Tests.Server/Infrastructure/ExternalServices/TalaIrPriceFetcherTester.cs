using GoldEx.Server.Infrastructure.Services.Price;

namespace GoldEx.Tests.Server.Infrastructure.ExternalServices;

public class TalaIrPriceFetcherTester
{
    private TalaIrPriceFetcher _talaPriceFetcher;

    [SetUp]
    public void Setup()
    {
        _talaPriceFetcher = new TalaIrPriceFetcher(new HttpClient());
    }

    [Test]
    public async Task GetCoinPriceAsync_ShouldReturnCoinPrices()
    {
        // Act
        var actual = await _talaPriceFetcher.GetCoinPriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetGoldPriceAsync_ShouldReturnGoldPrices()
    {
        // Act
        var actual = await _talaPriceFetcher.GetGoldPriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetCurrencyPriceAsync_ShouldReturnCurrencyPrice()
    {
        // Act
        var actual = await _talaPriceFetcher.GetCurrencyPriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetGram18PriceAsync_ShouldReturnGram18Price()
    {
        // Act
        var actual = await _talaPriceFetcher.GetGram18PriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public async Task GetDollarPriceAsync_ShouldReturnDollarPrice()
    {
        // Act
        var actual = await _talaPriceFetcher.GetDollarPriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
    }
}
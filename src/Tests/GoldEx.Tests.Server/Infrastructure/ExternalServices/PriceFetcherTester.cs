using GoldEx.Server.Infrastructure.Services.Price;

namespace GoldEx.Tests.Server.Infrastructure.ExternalServices;

public class PriceFetcherTester
{
    private PriceFetcher _priceFetcher;

    [SetUp]
    public void Setup()
    {
        _priceFetcher = new PriceFetcher(new HttpClient());
    }

    [Test]
    public async Task GetCoinPriceAsync_ShouldReturnCoinPrices()
    {
        // Act
        var actual = await _priceFetcher.GetCoinPriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetGoldPriceAsync_ShouldReturnGoldPrices()
    {
        // Act
        var actual = await _priceFetcher.GetGoldPriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetCurrencyPriceAsync_ShouldReturnCurrencyPrice()
    {
        // Act
        var actual = await _priceFetcher.GetCurrencyPriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetGram18PriceAsync_ShouldReturnGram18Price()
    {
        // Act
        var actual = await _priceFetcher.GetGram18PriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public async Task GetDollarPriceAsync_ShouldReturnDollarPrice()
    {
        // Act
        var actual = await _priceFetcher.GetDollarPriceAsync();
        // Assert
        Assert.That(actual, Is.Not.Null);
    }
}
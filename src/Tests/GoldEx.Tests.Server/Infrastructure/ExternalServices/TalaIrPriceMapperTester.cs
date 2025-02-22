using GoldEx.Server.Infrastructure.Services.Price.DTOs.TalaIr;
using System.Globalization;

namespace GoldEx.Tests.Server.Infrastructure.ExternalServices;

[TestFixture]
public class TalaIrPriceMapperTester
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void MapCoinPrices_ValidInput_ReturnsCorrectPrices()
    {
        // Arrange (Set up test data)
        var responses = new TalaIrApiResponse
        {
            CoinPrice = new Dictionary<string, CoinInfo>
            {
                { "sekke-gad", new CoinInfo { Title = "قديم", Value = "70,500,000", LastUpdate = "2024-07-27", Change = "100,000 (0.14%)" } },
                { "sekke-jad", new CoinInfo { Title = "جديد", Value = "74,300,000", LastUpdate = "2024-07-27", Change = "100,000 (0.13%)" } }
            }
        };

        // Act (Call the method under test)
        var prices = TalaIrApiResponseMapper.MapCoinPrices(responses);

        // Assert (Check the results)
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices.Count, Is.EqualTo(2)); // Check the number of returned prices

        var oldCoin = prices.FirstOrDefault(p => p.Title == "قدیم");
        Assert.That(oldCoin, Is.Not.Null);
        Assert.That(oldCoin.CurrentValue, Is.EqualTo(70500000)); // Check the parsed value
        Assert.That(oldCoin.LastUpdate, Is.EqualTo("2024-07-27"));
        Assert.That(oldCoin.Change, Is.EqualTo("100,000 (0.14%)"));

        var newCoin = prices.FirstOrDefault(p => p.Title == "جدید");
        Assert.That(newCoin, Is.Not.Null);
        Assert.That(newCoin.CurrentValue, Is.EqualTo(74300000)); // Check the parsed value
        Assert.That(newCoin.LastUpdate, Is.EqualTo("2024-07-27"));
        Assert.That(newCoin.Change, Is.EqualTo("100,000 (0.13%)"));
    }

    [Test]
    public void MapGoldPrices_ValidInput_ReturnsCorrectPrices()
    {
        // Arrange (Set up test data for Gold)
        var responses = new TalaIrApiResponse
        {
            GoldPrice = new Dictionary<string, GoldInfo>
            {
                { "gold_ounce", new GoldInfo { Title = "اونس طلا", Value = "2,882.23", LastUpdate = "2024-07-27", Change = "-0.32 (0.01%)" } },
                { "gold_24k", new GoldInfo { Title = "طلای 24 عیار", Value = "8,724,000", LastUpdate = "2024-07-27", Change = "-9,233 (0.11%)" } } // Example of another Gold item
            }
        };

        // Act (Call the method to map Gold prices)
        var prices = TalaIrApiResponseMapper.MapGoldPrices(responses); // Assuming you have a MapGoldPrices method

        // Assert (Check the results)
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices.Count, Is.EqualTo(2)); // Check for the correct number of Gold items

        var goldOunce = prices.FirstOrDefault(p => p.Title == "اونس طلا");
        Assert.That(goldOunce, Is.Not.Null);
        Assert.That(goldOunce.CurrentValue, Is.EqualTo(2882.23));
        Assert.That(goldOunce.LastUpdate, Is.EqualTo("2024-07-27"));
        Assert.That(goldOunce.Change, Is.EqualTo("-0.32 (0.01%)"));

        var gold24K = prices.FirstOrDefault(p => p.Title == "طلای 24 عیار");
        Assert.That(gold24K, Is.Not.Null);
        Assert.That(gold24K.CurrentValue, Is.EqualTo(8724000));
        Assert.That(gold24K.LastUpdate, Is.EqualTo("2024-07-27"));
        Assert.That(gold24K.Change, Is.EqualTo("-9,233 (0.11%)"));
    }

    [Test]
    public void MapCurrencyPrices_ValidInput_ReturnsCorrectPrices()
    {
        // Arrange (Set up test data for Currency)
        var responses = new TalaIrApiResponse
        {
            CurrencyPrice = new Dictionary<string, CurrencyInfo>
                {
                    { "arz_dolar", new CurrencyInfo { Title = "دلار", Value = "1739645575", LastUpdate = "2024-07-27", Change = "-30,440 (100%)", UnitTimeStamp = 1739728180 } },
                    { "arz_euro", new CurrencyInfo { Title = "یورو", Value = "0", LastUpdate = "2024-07-27", Change = "-32,500 (100%)" } } // Example of another currency
                }
        };

        var currencyInfo = responses.CurrencyPrice["arz_dolar"];

        var dateTime = DateTimeOffset.FromUnixTimeSeconds(currencyInfo.UnitTimeStamp).DateTime;

        Assert.That(dateTime, Is.EqualTo(DateTime.Parse("2/16/2025 5:49:40 PM")));

        // Act (Call the method to map Currency prices)
        var prices = TalaIrApiResponseMapper.MapCurrencyPrices(responses); // Assuming you have a MapCurrencyPrices method

        // Assert (Check the results)
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices.Count, Is.EqualTo(2)); // Check for the correct number of currencies

        var dollar = prices.FirstOrDefault(p => p.Title == "دلار");
        Assert.That(dollar, Is.Not.Null);

        // Important: Handle potential parsing issues with large numbers or different formats
        if (double.TryParse(dollar.CurrentValue.ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var dollarValue))
        {
            Assert.That(dollarValue, Is.EqualTo(1739645575));
        }
        else
        {
            Assert.Fail("Could not parse dollar value."); // Or handle the error appropriately
        }

        Assert.That(dollar.LastUpdate, Is.EqualTo("2024-07-27"));
        Assert.That(dollar.Change, Is.EqualTo("-30,440 (100%)"));

        var euro = prices.FirstOrDefault(p => p.Title == "یورو");
        Assert.That(euro, Is.Not.Null);

        if (double.TryParse(euro.CurrentValue.ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var euroValue))
        {
            Assert.That(euroValue, Is.EqualTo(0));
        }
        else
        {
            Assert.Fail("Could not parse euro value.");
        }

        Assert.That(euro.LastUpdate, Is.EqualTo("2024-07-27"));
        Assert.That(euro.Change, Is.EqualTo("-32,500 (100%)"));
    }
}
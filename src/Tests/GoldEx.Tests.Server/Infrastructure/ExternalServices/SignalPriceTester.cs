using GoldEx.Sdk.Common.Definitions;
using GoldEx.Server.Infrastructure.Services.Price.DTOs.Signal;
using Newtonsoft.Json;

namespace GoldEx.Tests.Server.Infrastructure.ExternalServices;

public class SignalPriceTester
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void MapToPriceResponses_ValidSignalResponse_ReturnsListOfPriceResponses()
    {
        // Arrange (Set up the test data)
        const string json = """
                    {
                      "data": {
                        "gold": {
                          "market": "gold",
                          "filterName": "gold",
                          "data": [
                            {
                              "change": 1498038,
                              "close": 69034000,
                              "iconUrl": "https://gw.isignal.ir/service/fileStorage@3/links/f4f9efaf91bf3377b14697ed9906c155",
                              "id": 100011,
                              "index": "0",
                              "jDate": "14031202",
                              "persianName": "طلای 18 عیار (گرم)",
                              "name": "geram18",
                              "time": "16:59:56",
                              "percentChange": 2.17,
                              "unit": "ریال"
                            }
                          ],
                          "totalLength": 1
                        },
                        "coin": {
                          "market": "coin",
                          "filterName": "coin",
                          "data": [
                            {
                              "iconUrl": "https://gw.isignal.ir/service/fileStorage@3/links/5420be560c3a0aac43af1d503599e9c9",
                              "time": "16:59:59",
                              "change": 15329160,
                              "index": 1,
                              "percentChange": 1.96,
                              "persianName": "سکه امامی",
                              "close": 782100000,
                              "jDate": "14031202",
                              "id": 100001,
                              "unit": "ریال"
                            },
                            {
                              "iconUrl": "https://gw.isignal.ir/service/fileStorage@3/links/5420be560c3a0aac43af1d503599e9c9",
                              "time": "16:59:59",
                              "change": 11407500,
                              "index": 2,
                              "percentChange": 1.56,
                              "persianName": "سکه بهار آزادی",
                              "close": 731250000,
                              "jDate": "14031202",
                              "id": 100000,
                              "unit": "ریال"
                            }
                          ],
                          "totalLength": 2
                        },
                        "parsianCoin": {
                          "market": "coin",
                          "filterName": "parsianCoin",
                          "data": [
                            {
                              "iconUrl": "https://gw.isignal.ir/service/fileStorage@3/links/5420be560c3a0aac43af1d503599e9c9",
                              "time": "11:50:10",
                              "change": 280000,
                              "index": 19,
                              "percentChange": 1.91,
                              "persianName": "سکه 200 سوتی",
                              "close": 14950000,
                              "jDate": "14031202",
                              "id": 100018,
                              "unit": "ریال"
                            },
                            {
                              "iconUrl": "https://gw.isignal.ir/service/fileStorage@3/links/5420be560c3a0aac43af1d503599e9c9",
                              "time": "11:50:09",
                              "change": 130000,
                              "index": 20,
                              "percentChange": 1.66,
                              "persianName": "سکه 100 سوتی",
                              "close": 7970000,
                              "jDate": "14031202",
                              "id": 100017,
                              "unit": "ریال"
                            }
                          ],
                          "totalLength": 2
                        },
                        "Currency": {
                          "market": "currency",
                          "filterName": "Currency",
                          "data": [
                            {
                              "change": 13500,
                              "close": 941000,
                              "id": 200000,
                              "index": 1,
                              "jDate": "14031202",
                              "persianName": "دلار",
                              "name": "usDollar",
                              "time": "21:00:19",
                              "percentChange": 1.46,
                              "unit": "ریال",
                              "iconUrl": "https://gw.isignal.ir/service/fileStorage@3/links/b6e53ecaa090885e2b5b2388b774519d"
                            },
                            {
                              "change": 12235,
                              "close": 978800,
                              "id": 200001,
                              "index": 2,
                              "jDate": "14031202",
                              "persianName": "یورو",
                              "name": "euro",
                              "time": "16:59:57",
                              "percentChange": 1.25,
                              "unit": "ریال",
                              "iconUrl": "https://gw.isignal.ir/service/fileStorage@3/links/5a78f348c676e8175dbf4114b6df49d5"
                            }
                          ],
                          "totalLength": 2
                        }
                      },
                      "meta": {
                        "shamsiDate": "14031202221310180",
                        "requestId": "3dcdfc4d-ca66-4101-ae28-b4f016a50699"
                      }
                    }
                    """;

        var marketResponse = JsonConvert.DeserializeObject<SignalApiResponse>(json);

        // Act (Call the method being tested)
        var priceResponses = SignalApiResponseMapper.MapPrices(marketResponse);

        // Assert (Check the results)
        Assert.That(priceResponses, Is.Not.Null);
        Assert.That(priceResponses.Count, Is.EqualTo(7)); // Check the number of returned prices

        var euro = priceResponses.FirstOrDefault(p => p.Title == "یورو");
        Assert.That(euro, Is.Not.Null);
        Assert.That(euro.CurrentValue, Is.EqualTo(978800)); // Check the parsed value
        Assert.That(euro.LastUpdate, Is.EqualTo("1403/12/02 16:59:57"));
        Assert.That(euro.Change, Is.EqualTo("12,235 (1.25%)"));
        Assert.That(euro.IconUrl, Is.EqualTo("https://gw.isignal.ir/service/fileStorage@3/links/5a78f348c676e8175dbf4114b6df49d5"));
        Assert.That(euro.MarketType, Is.EqualTo(MarketType.Currency));
    }
}
using System.Text.Json.Serialization;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs.Signal;

public class SignalPayload
{
    public List<SignalPayloadItem> Items { get; set; } = default!;
}

public class SignalPayloadItem
{
    [JsonPropertyName("property")]
    public string[] Property { get; set; } = default!;

    [JsonPropertyName("filterName")]
    public string? FilterName { get; set; }

    [JsonPropertyName("filterLists")]
    public List<List<Filter>>? FilterLists { get; set; }

    [JsonPropertyName("market")]
    public string Market { get; set; } = default!;

    public static List<SignalPayloadItem> CreateDefaultPayload()
    {
        return
        [
            CreateCoinPayload(),
            CreateCoinBubblePayload(),
            CreateCoinParsianPayload(),
            CreateCurrencyPayload(),
            CreateGoldPayload()
        ];
    }

    public static SignalPayloadItem CreateGoldPayload()
    {
        return new SignalPayloadItem
        {
            Market = "gold",
            FilterName = "gold",
            Property =
            [
                "name",
                "change",
                "close",
                "iconUrl",
                "id",
                "index",
                "jDate",
                "persianName",
                "time",
                "percentChange",
                "unit"
            ]
        };
    }

    public static SignalPayloadItem CreateCurrencyPayload()
    {
        return new SignalPayloadItem
        {
            Market = "currency",
            Property =
            [
                "name",
                "change",
                "close",
                "iconUrl",
                "id",
                "index",
                "jDate",
                "persianName",
                "time",
                "percentChange",
                "unit"
            ],
            FilterName = "freeCurrency",
            FilterLists =
            [
                [
                    new Filter
                    {
                        Field = "subCategory",
                        Include = true,
                        Opt = "e",
                        Values =
                        [
                            "free"
                        ]
                    }
                ]
            ]
        };
    }

    public static SignalPayloadItem CreateCoinBubblePayload()
    {
        return new SignalPayloadItem
        {
            Market = "coin",
            Property =
            [
                "name",
                "change",
                "close",
                "iconUrl",
                "id",
                "index",
                "jDate",
                "persianName",
                "time",
                "percentChange",
                "unit"
            ],
            FilterName = "coinBubble",
            FilterLists =
            [
                [
                    new Filter
                    {
                        Field = "subCategory",
                        Include = true,
                        Opt = "e",
                        Values =
                        [
                            "coinBubble"
                        ]
                    }
                ]
            ]
        };
    }

    public static SignalPayloadItem CreateCoinParsianPayload()
    {
        return new SignalPayloadItem
        {
            Market = "coin",
            Property =
            [
                "name",
                "change",
                "close",
                "iconUrl",
                "id",
                "index",
                "jDate",
                "persianName",
                "time",
                "percentChange",
                "unit"
            ],
            FilterName = "coinParsian",
            FilterLists =
            [
                [
                    new Filter
                    {
                        Field = "subCategory",
                        Include = true,
                        Opt = "e",
                        Values =
                        [
                            "coinParsian"
                        ]
                    }
                ]
            ]
        };
    }

    public static SignalPayloadItem CreateCoinPayload()
    {
        return new SignalPayloadItem
        {
            Market = "coin",
            Property =
            [
                "name",
                "change",
                "close",
                "iconUrl",
                "id",
                "index",
                "jDate",
                "persianName",
                "time",
                "percentChange",
                "unit"
            ],
            FilterName = "coin",
            FilterLists =
            [
                [
                    new Filter
                    {
                        Field = "subCategory",
                        Include = false,
                        Opt = "e",
                        Values =
                        [
                            "coinParsian",
                            "coinBubble"
                        ]
                    }
                ]
            ]
        };
    }
}

public class Filter
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = default!;

    [JsonPropertyName("values")]
    public List<string> Values { get; set; } = default!;

    [JsonPropertyName("opt")]
    public string Opt { get; set; } = default!;

    [JsonPropertyName("include")]
    public bool Include { get; set; }
}
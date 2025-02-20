using System.Text.Json.Serialization;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs.Signal;

public class MarketData
{
    [JsonPropertyName("market")]
    public string Market { get; set; } = default!;

    [JsonPropertyName("filterName")]
    public string FilterName { get; set; } = default!;

    [JsonPropertyName("data")]
    public List<MarketDataItem> Data { get; set; } = default!;

    [JsonPropertyName("totalLength")]
    public int TotalLength { get; set; }
}

public class Data
{
    [JsonPropertyName("gold")]
    public MarketData? Gold { get; set; }

    [JsonPropertyName("coin")]
    public MarketData? Coin { get; set; }

    [JsonPropertyName("coinParsian")]
    public MarketData? ParsianCoin { get; set; }

    [JsonPropertyName("coinBubble")]
    public MarketData? BubbleCoin { get; set; }

    [JsonPropertyName("freeCurrency")]
    public MarketData? Currency { get; set; }
}

public class MarketDataItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("close")]
    public long Close { get; set; }

    [JsonPropertyName("change")]
    public long Change { get; set; }

    [JsonPropertyName("percentChange")]
    public double PercentChange { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = default!;

    [JsonPropertyName("persianName")]
    public string? PersianName { get; set; }

    [JsonPropertyName("jDate")]
    public string JDate { get; set; } = default!;

    [JsonPropertyName("iconUrl")]
    public string IconUrl { get; set; } = default!;

    [JsonPropertyName("time")]
    public string Time { get; set; } = default!;
}

public class MetaData
{
    [JsonPropertyName("shamsiDate")]
    public string ShamsiDate { get; set; } = default!;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = default!;
}

public class SignalApiResponse
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = default!;

    [JsonPropertyName("meta")]
    public MetaData Meta { get; set; } = default!;
}
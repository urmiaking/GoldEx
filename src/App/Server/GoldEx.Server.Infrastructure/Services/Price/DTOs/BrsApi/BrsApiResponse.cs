using System.Text.Json.Serialization;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs.BrsApi;

public class BrsApiResponse
{
    [JsonPropertyName("gold")]
    public List<BrsMarketItem>? Gold { get; set; }

    [JsonPropertyName("currency")]
    public List<BrsMarketItem>? Currency { get; set; }

    [JsonPropertyName("cryptocurrency")]
    public List<BrsCryptoItem>? Cryptocurrency { get; set; }
}

public class BrsMarketItem
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("change_value")]
    public decimal ChangeValue { get; set; }

    [JsonPropertyName("change_percent")]
    public double ChangePercent { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = default!;

    [JsonPropertyName("date")]
    public string Date { get; set; } = default!;

    [JsonPropertyName("time")]
    public string Time { get; set; } = default!;
}

public class BrsCryptoItem
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("price")]
    public string Price { get; set; } = default!;

    [JsonPropertyName("change_percent")]
    public double ChangePercent { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = default!;

    [JsonPropertyName("date")]
    public string Date { get; set; } = default!;

    [JsonPropertyName("time")]
    public string Time { get; set; } = default!;
}
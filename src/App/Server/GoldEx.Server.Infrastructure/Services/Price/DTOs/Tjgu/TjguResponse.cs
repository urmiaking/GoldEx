using System.Text.Json.Serialization;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs.Tjgu;

public record TjguPriceItem
{
    [JsonPropertyName("p")]
    public string P { get; init; } = string.Empty;

    [JsonPropertyName("h")]
    public string H { get; init; } = string.Empty;

    [JsonPropertyName("l")]
    public string L { get; init; } = string.Empty;

    [JsonPropertyName("d")]
    public string D { get; init; } = string.Empty;

    [JsonPropertyName("dp")]
    public double Dp { get; init; }

    [JsonPropertyName("dt")]
    public string Dt { get; init; } = string.Empty;

    [JsonPropertyName("t")]
    public string T { get; init; } = string.Empty;

    [JsonPropertyName("t_en")]
    public string TEn { get; init; } = string.Empty;

    [JsonPropertyName("t-g")]
    public string TG { get; init; } = string.Empty;

    [JsonPropertyName("ts")]
    public string Ts { get; init; } = string.Empty;
}

public class TjguResponse
{
    [JsonPropertyName("current")]
    public Dictionary<string, TjguPriceItem> Current { get; set; } = new();
}
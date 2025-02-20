using System.Text.Json.Serialization;

namespace GoldEx.Server.Infrastructure.Services.Price.DTOs.TalaIr;

public class TalaIrApiResponse
{
    [JsonPropertyName("sekke")]
    public Dictionary<string, CoinInfo>? CoinPrice { get; set; }

    [JsonPropertyName("gold")]
    public Dictionary<string, GoldInfo>? GoldPrice { get; set; }

    [JsonPropertyName("arz")]
    public Dictionary<string, CurrencyInfo>? CurrencyPrice { get; set; }
}

public class CoinInfo
{
    [JsonPropertyName("m")]
    public long UnitTimeStamp { get; set; }

    [JsonPropertyName("jdate")]
    public string LastUpdate { get; set; } = default!;

    [JsonPropertyName("v_fa")]
    public string ValueInPersian { get; set; } = default!;

    [JsonPropertyName("v")]
    public string Value { get; set; } = default!;

    [JsonPropertyName("c")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("min")]
    public string Min { get; set; } = default!;

    [JsonPropertyName("max")]
    public string Max { get; set; } = default!;

    [JsonPropertyName("d")]
    public string Change { get; set; } = default!;
}

public class GoldInfo
{
    [JsonPropertyName("m")]
    public long UnitTimeStamp { get; set; }

    [JsonPropertyName("jdate")]
    public string LastUpdate { get; set; } = default!;

    [JsonPropertyName("v_fa")]
    public string ValueInPersian { get; set; } = default!;

    [JsonPropertyName("v")]
    public string Value { get; set; } = default!;

    [JsonPropertyName("c")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("min")]
    public string Min { get; set; } = default!;

    [JsonPropertyName("max")]
    public string Max { get; set; } = default!;

    [JsonPropertyName("d")]
    public string Change { get; set; } = default!;
}

public class CurrencyInfo
{
    [JsonPropertyName("m")]
    public long UnitTimeStamp { get; set; }

    [JsonPropertyName("jdate")]
    public string LastUpdate { get; set; } = default!;

    [JsonPropertyName("v_fa")]
    public string ValueInPersian { get; set; } = default!;

    [JsonPropertyName("v")]
    public string Value { get; set; } = default!;

    [JsonPropertyName("c")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("d")]
    public string Change { get; set; } = default!;
}
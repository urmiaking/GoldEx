using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoldEx.Shared;

public static class Utilities
{
    public static HttpClient GetHttpClient(string baseAddress)
    {
        return new HttpClient { BaseAddress = new Uri(baseAddress) };
    }

    public static JsonSerializerOptions GetJsonOptions()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        jsonOptions.Converters.Clear();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        return jsonOptions;
    }
}

namespace GoldEx.Sdk.Common.Data;

public record RequestFilter(int? Skip = null, int? Take = null, string? Search = null)
{
    public int? Skip { get; set; } = Skip;
}

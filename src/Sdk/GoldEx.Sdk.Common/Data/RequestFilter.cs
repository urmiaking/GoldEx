using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Sdk.Common.Data;

public record RequestFilter(int? Skip = null, int? Take = null, string? Search = null, string? SortLabel = null, SortDirection? SortDirection = null)
{
    public int? Skip { get; set; } = Skip;
}

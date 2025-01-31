namespace GoldEx.Sdk.Common.Data;

public class PagedList<T>
{
    public int Total { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
    public required List<T> Data { get; init; }
}

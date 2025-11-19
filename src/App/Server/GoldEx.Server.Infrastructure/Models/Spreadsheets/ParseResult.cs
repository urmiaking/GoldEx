namespace GoldEx.Server.Infrastructure.Models.Spreadsheets;

public record ParseResult<T>
{
    public List<T> Items { get; init; } = [];
    public int TotalRows { get; init; }
    public int MappedRows { get; init; }
    public int SkippedRows { get; init; }
    public List<SkippedRowInfo> SkippedRowDetails { get; init; } = [];
    public IReadOnlyDictionary<string, int> HeaderToColumnIndex { get; init; } = new Dictionary<string, int>();
}
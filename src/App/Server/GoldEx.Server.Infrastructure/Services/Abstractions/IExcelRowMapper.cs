namespace GoldEx.Server.Infrastructure.Services.Abstractions;

public interface IExcelRowMapper<T>
{
    /// <summary>
    /// Hints for header detection / matching. The parser will normalize both header cell text and these hints and try 'contains' match.
    /// e.g. "کد", "barcode", "name", "وزن"
    /// </summary>
    IReadOnlyCollection<string> HeaderHints { get; }

    /// <summary>
    /// Try to map a row expressed as dictionary(headerName -> cellValue) into T.
    /// If returns false, reason must explain why it failed (validation, missing required fields, parse error, etc.).
    /// </summary>
    bool TryMap(IReadOnlyDictionary<string, string?> rowByHeader, int rowIndex, out T? item, out string? reason);
}
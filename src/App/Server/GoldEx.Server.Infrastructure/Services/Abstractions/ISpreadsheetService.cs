using GoldEx.Server.Infrastructure.Models.Spreadsheets;

namespace GoldEx.Server.Infrastructure.Services.Abstractions;

public interface ISpreadsheetService
{
    /// <summary>
    /// Parse an Excel file bytes into T using provided mapper.
    /// Service will try to detect header row automatically, normalize header names, do fuzzy contain match with mapper.HeaderHints,
    /// skip empty rows, and report skipped rows and reasons.
    /// </summary>
    Task<ParseResult<T>> ParseAsync<T>(
        byte[] fileBytes,
        IExcelRowMapper<T> mapper,
        int maxHeaderScanRows = 10,
        CancellationToken cancellationToken = default);
}
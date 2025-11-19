using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Models.Spreadsheets;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using OfficeOpenXml;
using System.Globalization;
using System.Text;

namespace GoldEx.Server.Infrastructure.Services;

[ScopedService]
internal class SpreadsheetService : ISpreadsheetService
{
    public SpreadsheetService()
    {
        ExcelPackage.License.SetNonCommercialPersonal("Masoud Khodadadi");
    }

    public async Task<ParseResult<T>> ParseAsync<T>(
        byte[] fileBytes,
        IExcelRowMapper<T> mapper,
        int maxHeaderScanRows = 10,
        CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream(fileBytes);
        using var package = new ExcelPackage();
        await package.LoadAsync(ms, cancellationToken);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault()
                        ?? throw new InvalidOperationException("No worksheet found in Excel file.");

        var rowCount = worksheet.Dimension?.Rows ?? 0;
        var colCount = worksheet.Dimension?.Columns ?? 0;

        var result = new ParseResult<T> { TotalRows = Math.Max(0, rowCount) };

        if (rowCount == 0 || colCount == 0)
            return result;

        // Read first up to maxHeaderScanRows to detect header row
        var headerRowIndex = DetectHeaderRow(worksheet, mapper.HeaderHints, maxHeaderScanRows);

        // Build header names list (normalize and keep original)
        var headers = new List<string>(colCount + 1) { string.Empty }; // 1-based index for convenience
        if (headerRowIndex > 0)
        {
            for (var c = 1; c <= colCount; c++)
            {
                var raw = worksheet.Cells[headerRowIndex, c].GetValue<string>() ?? string.Empty;
                headers.Add(raw);
            }
        }
        else
        {
            // no header detected -> generate placeholder header names
            for (var c = 1; c <= colCount; c++)
                headers.Add($"Column{c}");
        }

        // Build header->col index mapping using fuzzy contain (normalize both)
        var headerToIndex = BuildHeaderMap(headers.Skip(1).ToList(), mapper.HeaderHints);

        // store to result
        result = result with { HeaderToColumnIndex = headerToIndex };

        var startRow = (headerRowIndex > 0) ? headerRowIndex + 1 : 1;

        for (var r = startRow; r <= rowCount; r++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // load row values
            var rowValues = new List<string?>(colCount);
            var allEmpty = true;
            for (var c = 1; c <= colCount; c++)
            {
                var v = worksheet.Cells[r, c].GetValue<object>()?.ToString()?.Trim();
                rowValues.Add(string.IsNullOrWhiteSpace(v) ? null : v);
                if (!string.IsNullOrWhiteSpace(v)) allEmpty = false;
            }

            if (allEmpty)
            {
                result.SkippedRowDetails.Add(new SkippedRowInfo(r, rowValues, "EmptyRow"));
                continue;
            }

            // construct dictionary headerName -> value
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            for (var c = 1; c <= colCount; c++)
            {
                var headerKey = headers[c];

                // If header mapping found a mapped canonical header (i.e., one of mapper hints), prefer that key
                if (headerToIndex.TryGetValue(headerKey, out var idx) && idx != c)
                {
                    // do nothing special — keep headerKey
                }

                dict[headerKey] = rowValues[c - 1];
            }

            // let mapper try map
            if (mapper.TryMap(dict, r, out var item, out var reason))
            {
                result.Items.Add(item!);
            }
            else
            {
                result.SkippedRowDetails.Add(new SkippedRowInfo(r, rowValues, reason ?? "MapperRejected"));
            }
        }

        // finalize counts
        result = result with
        {
            MappedRows = result.Items.Count,
            SkippedRows = result.SkippedRowDetails.Count
        };

        return result;
    }

    // ------ Helper functions ------

    private static int DetectHeaderRow(ExcelWorksheet ws, IReadOnlyCollection<string> hints, int maxRows)
    {
        var rowCount = ws.Dimension.Rows;
        var colCount = ws.Dimension.Columns;
        var scanTo = Math.Min(rowCount, maxRows);

        var bestRow = -1;
        double bestScore = -1;

        var normalizedHints = hints.Select(NormalizeHeader).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

        for (var r = 1; r <= scanTo; r++)
        {
            var matchCount = 0;
            var nonEmptyCount = 0;
            for (var c = 1; c <= colCount; c++)
            {
                var val = ws.Cells[r, c].GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(val)) nonEmptyCount++;
                var nv = NormalizeHeader(val);
                if (string.IsNullOrWhiteSpace(nv)) continue;

                // if any hint is contained in nv => match
                if (normalizedHints.Any(h => nv.Contains(h)))
                    matchCount++;
            }

            // score heuristic: prefer rows with multiple hint matches, but tolerate small hints
            var score = matchCount * 2 + nonEmptyCount * 0.2;
            if (score > bestScore)
            {
                bestScore = score;
                bestRow = r;
            }
        }

        // decide threshold: require at least 1 match OR first row has many non-numeric cells
        if (bestRow > 0 && bestScore >= 2) // at least some hints matched
            return bestRow;

        // fallback: if first row has many non-numeric (likely header) choose it
        var firstRowTextCount = 0;
        for (var c = 1; c <= ws.Dimension.Columns; c++)
        {
            var s = ws.Cells[1, c].GetValue<string>() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(s) && !IsMostlyNumeric(s)) firstRowTextCount++;
        }
        if (firstRowTextCount >= Math.Max(1, ws.Dimension.Columns / 3))
            return 1;

        // no header detected
        return -1;
    }

    private static Dictionary<string, int> BuildHeaderMap(List<string> headers, IReadOnlyCollection<string> hints)
    {
        // normalize headers and hints and try contain-based mapping
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // headerOriginal -> index (1-based)
        var normHeaders = headers.Select((h, i) => new { Original = h, Normal = NormalizeHeader(h), Index = i + 1 }).ToList();
        var normHints = hints.Select(NormalizeHeader).Where(h => !string.IsNullOrWhiteSpace(h)).ToList();

        // for each header, try to find best matching hint
        foreach (var h in normHeaders)
        {
            // find a hint that is contained in header normal or header normal contained in hint
            var matchedHint = normHints.FirstOrDefault(hint =>
                (!string.IsNullOrWhiteSpace(h.Normal) && h.Normal.Contains(hint)) ||
                (!string.IsNullOrWhiteSpace(hint) && hint.Contains(h.Normal))
            );

            if (matchedHint != null)
            {
                // canonical key: use the original header text (keeps user-provided name) but map to column index
                map[h.Original] = h.Index;
            }
            else
            {
                // also include header as-is so caller can access it by its name
                map[h.Original] = h.Index;
            }
        }

        return map;
    }

    private static string NormalizeHeader(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;

        // lower, trim
        var v = s.Trim().ToLowerInvariant();

        // normalize diacritics (NFKD) and remove non-letter/digit characters
        v = v.Normalize(NormalizationForm.FormKD);
        var sb = new StringBuilder();
        foreach (var ch in v)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc == UnicodeCategory.NonSpacingMark) continue; // remove accents
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            // treat whitespace and punctuation as removal
        }

        return sb.ToString();
    }

    private static bool IsMostlyNumeric(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        var digits = s.Count(char.IsDigit);
        var others = s.Length - digits;
        return digits > others;
    }
}
using GoldEx.Shared.DTOs.Products;
using System.Collections.Generic;
using System.Linq;

namespace GoldEx.Shared.Helpers;

public static class StonesHelper
{
    public static string GetStonesSummary(IEnumerable<GetGemStoneResponse>? stones)
    {
        if (stones == null || !stones.Any())
            return string.Empty;

        var grouped = stones
            .GroupBy(s => !string.IsNullOrEmpty(s.StoneTypeSymbol) ? s.StoneTypeSymbol : s.Type)
            .Select(g => $"{g.Key}:{g.Sum(s => s.Carat):G29}");

        return string.Join(" ", grouped);
    }
}

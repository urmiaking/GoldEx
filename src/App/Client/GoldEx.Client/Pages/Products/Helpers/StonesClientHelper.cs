using GoldEx.Client.Pages.Products.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace GoldEx.Client.Pages.Products.Helpers;

public static class StonesClientHelper
{
    public static string GetStonesSummary(IEnumerable<GemStoneVm>? stones)
    {
        if (stones == null || !stones.Any())
            return string.Empty;

        var grouped = stones
            .GroupBy(s => !string.IsNullOrEmpty(s.StoneTypeSymbol) ? s.StoneTypeSymbol : s.Type)
            .Select(g => $"{g.Key}:{g.Sum(s => s.Carat):G29}");

        return string.Join(" ", grouped);
    }
}

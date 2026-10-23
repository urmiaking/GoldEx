using GoldEx.Client.Pages.Reporting.Pages.Reports;
using GoldEx.Shared.Enums;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public class ProfitAnalysisPageTester
{
    private class PageProbe : ProfitAnalysis
    {
        public GoldEx.Shared.DTOs.Reporting.ProfitAnalysisRpRequest ReadDates(DateTime? start, DateTime? end)
        {
            Start = start;
            End = end;
            ReadQueryToFilter();
            return Filters.ToRequest();
        }

        public ProductType? ReadType(string? type)
        {
            Type = type;
            ReadQueryToFilter();
            return Filters.ProductType;
        }
    }

    [TestCase(null, null)]
    [TestCase("", null)]
    [TestCase("Gold", ProductType.Gold)]
    [TestCase("gold", ProductType.Gold)]
    [TestCase("Jewelry", ProductType.Jewelry)]
    [TestCase("UsedGold", null)]
    [TestCase("invalid", null)]
    public void QueryType_IsSupportedStringAndSafelyParsed(string? query, ProductType? expected)
    {
        // Blazor's query value supplier rejects enum properties even when the query is absent.
        Assert.That(typeof(ProfitAnalysis).GetProperty(nameof(ProfitAnalysis.Type))!.PropertyType, Is.EqualTo(typeof(string)));
        Assert.That(new PageProbe().ReadType(query), Is.EqualTo(expected));
    }

    [Test]
    public void EmptyOrClearedDates_RemainUnboundedInsteadOfDefaultingToToday()
    {
        var page = new PageProbe();
        var dated = page.ReadDates(new(2024, 1, 1), new(2026, 10, 23));
        Assert.That(dated.FromDate?.Date, Is.EqualTo(new DateTime(2024, 1, 1)));
        Assert.That(dated.ToDate?.Date, Is.EqualTo(new DateTime(2026, 10, 23)));

        var cleared = page.ReadDates(null, null);
        Assert.That(cleared.FromDate, Is.Null);
        Assert.That(cleared.ToDate, Is.Null);
    }
}

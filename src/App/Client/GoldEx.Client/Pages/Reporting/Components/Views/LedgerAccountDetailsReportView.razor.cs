using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GoldEx.Client.Pages.Reporting.ViewModels.LedgerAccountDetailsFilterVm;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class LedgerAccountDetailsReportView
{
    [Parameter] public List<LedgerAccountStatementRpResponse>? Items { get; set; }
    [Parameter] public EventCallback<(string RefType, Guid? RefId)> OnOpenReference { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private LedgerAccountDetailsReportSummary? _summary;

    protected override void OnParametersSet()
    {
        CalculateSummary();
        base.OnParametersSet();
    }

    private void CalculateSummary()
    {
        if (Items == null || !Items.Any())
        {
            _summary = null;
            return;
        }

        var groupedByPriceUnit = Items
            .GroupBy(x => x.PriceUnitTitle)
            .Select(g => new PriceUnitSummary
            {
                PriceUnitTitle = g.Key,
                TotalDebit = g.Where(x => x.TransactionType == TransactionType.Debit).Sum(x => x.Amount),
                TotalCredit = g.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.Amount),
                TotalBaseCurrency = g.Sum(x => x.BaseCurrencyAmount),
                ExchangeRateInfo = CalculateAverageExchangeRate(g)
            })
            .ToList();

        // Check if all price units are base currency  
        var allPriceUnitsAreBaseCurrency = groupedByPriceUnit.All(x =>
            string.IsNullOrEmpty(x.ExchangeRateInfo));

        _summary = new LedgerAccountDetailsReportSummary
        {
            PriceUnitSummaries = groupedByPriceUnit,
            TotalBaseCurrencyDebit = Items.Where(x => x.TransactionType == TransactionType.Debit).Sum(x => x.BaseCurrencyAmount),
            TotalBaseCurrencyCredit = Items.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.BaseCurrencyAmount),
            ShowTotalBaseCurrencySummary = !allPriceUnitsAreBaseCurrency
        };
    }

    private string? CalculateAverageExchangeRate(IGrouping<string, LedgerAccountStatementRpResponse> group)
    {
        var exchangeRates = group
            .Where(x => x.ExchangeRate.HasValue)
            .Select(x => x.ExchangeRate!.Value)
            .ToList();

        if (!exchangeRates.Any())
            return null;

        var averageRate = exchangeRates.Average();
        return averageRate > 0 ? $"نرخ تبدیل میانگین: {averageRate:N0}" : null;
    }

    private string GetTransactionIcon(LedgerAccountStatementRpResponse context)
    {
        return context.TransactionType is TransactionType.Debit
            ? Icons.Material.Filled.ArrowDownward
            : Icons.Material.Filled.ArrowUpward;
    }

    private Color GetTransactionIconColor(LedgerAccountStatementRpResponse context)
    {
        return context.TransactionType is TransactionType.Debit
            ? Color.Success
            : Color.Error;
    }

    private Color GetRunningBalanceColor(LedgerAccountStatementRpResponse context)
    {
        if (context.RunningBalance == 0)
            return Color.Inherit;

        return context.RunningBalance > 0
            ? Color.Error
            : Color.Success;
    }
}
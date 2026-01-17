using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GoldEx.Client.Pages.Reporting.ViewModels.CustomerTransactionFilterVm;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class CustomerTransactionsReportView
{
    [Parameter] public List<CustomerTransactionRpResponse>? Items { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public EventCallback<(string RefType, Guid? RefId)> OnOpenReference { get; set; }

    private CustomerTransactionsReportSummary? _summary;

    protected override void OnParametersSet()
    {
        CalculateSummary();
        base.OnParametersSet();
    }

    private static decimal Signed(TransactionType type, decimal amount)
        => type == TransactionType.Debit ? amount : -amount;

    private void NormalizeBalances(ref decimal receivableBalance, ref decimal payableBalance)
    {
        if (receivableBalance < 0m)
        {
            payableBalance -= Math.Abs(receivableBalance);
            receivableBalance = 0m;
        }

        if (payableBalance > 0m)
        {
            receivableBalance += payableBalance;
            payableBalance = 0m;
        }
    }

    private void CalculateSummary()
    {
        if (Items == null || Items.Count == 0)
        {
            _summary = null;
            return;
        }

        var priceUnitSummaries = Items
            .GroupBy(x => x.PriceUnitTitle)
            .Select(g =>
            {
                var receivableBalance = g
                    .Where(x => x.Role == LedgerAccountRole.Receivable)
                    .Sum(x => Signed(x.TransactionType, x.Amount));

                var payableBalance = g
                    .Where(x => x.Role == LedgerAccountRole.Payable)
                    .Sum(x => Signed(x.TransactionType, x.Amount));

                NormalizeBalances(ref receivableBalance, ref payableBalance);

                var netBalance = receivableBalance + payableBalance;

                var totalBaseCurrencyAmount = g.Sum(x => x.BaseCurrencyAmount);

                return new PriceUnitSummary
                {
                    PriceUnitTitle = g.Key,
                    ReceivableBalance = receivableBalance,
                    PayableBalance = payableBalance,
                    NetBalance = netBalance,
                    TotalBaseCurrencyAmount = totalBaseCurrencyAmount,
                    ExchangeRateInfo = CalculateAverageExchangeRate(g)
                };
            })
            .OrderBy(x => x.PriceUnitTitle)
            .ToList();

        var totalBaseReceivableRemaining = Items
            .Where(x => x.Role == LedgerAccountRole.Receivable)
            .Sum(x => Signed(x.TransactionType, x.BaseCurrencyAmount));

        var totalBasePayableRemaining = Items
            .Where(x => x.Role == LedgerAccountRole.Payable)
            .Sum(x => Signed(x.TransactionType, x.BaseCurrencyAmount));

        var showBaseSummary = Items.Any(x => x.ExchangeRate is not null);

        _summary = new CustomerTransactionsReportSummary
        {
            PriceUnitSummaries = priceUnitSummaries,

            TotalBaseCurrencyReceivableRemaining = totalBaseReceivableRemaining,
            TotalBaseCurrencyPayableRemaining = totalBasePayableRemaining,

            ShowTotalBaseCurrencySummary = showBaseSummary
        };
    }

    private string? CalculateAverageExchangeRate(IGrouping<string, CustomerTransactionRpResponse> group)
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

    private decimal GetSignedDelta(CustomerTransactionRpResponse x)
        => x.TransactionType == TransactionType.Debit ? x.Amount : -x.Amount;

    private string? GetTransactionIcon(CustomerTransactionRpResponse context)
    {
        if (context.Amount == 0) return null;

        var delta = GetSignedDelta(context);
        return delta > 0
            ? Icons.Material.Filled.ArrowUpward
            : Icons.Material.Filled.ArrowDownward;
    }

    private Color GetTransactionIconColor(CustomerTransactionRpResponse context)
    {
        if (context.Amount == 0) return Color.Inherit;

        var delta = GetSignedDelta(context);
        return delta > 0 ? Color.Error : Color.Success;
    }

    private Color GetRunningBalanceColor(CustomerTransactionRpResponse context)
    {
        if (context.RunningBalance == 0)
            return Color.Inherit;

        return context.RunningBalance > 0
            ? Color.Error
            : Color.Success;
    }
}
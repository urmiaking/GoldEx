using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GoldEx.Client.Pages.Reporting.ViewModels.LedgerAccountDetailsFilterVm;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class LedgerAccountDetailsReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public Guid LedgerAccountId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public Guid? PriceUnitId { get; set; }

    private LedgerAccountDetailsFilterVm _filter = default!;
    private List<LedgerAccountStatementRpResponse>? _items;
    private GetLedgerAccountResponse? _ledgerAccount;
    private ReportSummaryVm? _summary;

    private readonly int _version = new Random().Next(0, 1000);

    protected override void OnInitialized()
    {
        _filter = new LedgerAccountDetailsFilterVm
        {
            LedgerAccountId = LedgerAccountId,
            PriceUnitId = PriceUnitId,
            DateRange = (FromDate.HasValue || ToDate.HasValue)
                ? new DateRange(FromDate, ToDate)
                : null
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadLedgerAccountAsync();
        await LoadReportAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadLedgerAccountAsync()
    {
        await SendRequestAsync<ILedgerAccountService, GetLedgerAccountResponse>(
            action: (s, ct) => s.GetAsync(LedgerAccountId, ct),
            afterSend: response => _ledgerAccount = response,
            createScope: true);
    }

    private async Task LoadReportAsync()
    {
        var request = _filter.ToRequest();

        _items = await SendRequestAsync<IReportingService, List<LedgerAccountStatementRpResponse>>(
            action: (s, ct) => s.GetLedgerAccountStatementsAsync(request, ct),
            createScope: true);

        CalculateSummary();
    }

    private void CalculateSummary()
    {
        if (_items == null || !_items.Any())
        {
            _summary = null;
            return;
        }

        var groupedByPriceUnit = _items
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

        // Skip summary if only one price unit and it's base currency  
        var shouldSkipSummary = groupedByPriceUnit.Count == 1 &&
                               groupedByPriceUnit.First().TotalDebit == groupedByPriceUnit.First().TotalBaseCurrency &&
                               groupedByPriceUnit.First().TotalCredit == groupedByPriceUnit.First().TotalBaseCurrency;

        if (shouldSkipSummary)
        {
            _summary = null;
            return;
        }

        // Check if all price units are base currency  
        var allPriceUnitsAreBaseCurrency = groupedByPriceUnit.All(x => string.IsNullOrEmpty(x.ExchangeRateInfo));

        // Create a section for each price unit  
        var sections = groupedByPriceUnit.Select(pu => new SummarySection
            {
                Title = pu.PriceUnitTitle,
                Info = pu.ExchangeRateInfo ?? string.Empty,
                Items =
                [
                    new SummaryItem
                    {
                        Label = "مجموع بدهکار",
                        Value = pu.TotalDebit.ToCurrencyFormat(pu.PriceUnitTitle),
                        Type = "debit",
                        ShowIcon = true,
                        IconType = "debit-icon"
                    },
                    new SummaryItem
                    {
                        Label = "مجموع بستانکار",
                        Value = pu.TotalCredit.ToCurrencyFormat(pu.PriceUnitTitle),
                        Type = "credit",
                        ShowIcon = true,
                        IconType = "credit-icon"
                    },
                    new SummaryItem
                    {
                        Label = "خالص",
                        Value = $"{Math.Abs(pu.TotalDebit - pu.TotalCredit).ToCurrencyFormat(pu.PriceUnitTitle)} {(pu.TotalDebit >= pu.TotalCredit ? "بدهکار" : "بستانکار")}",
                        Type = "net",
                        ShowIcon = true,
                        IconType = pu.TotalDebit >= pu.TotalCredit ? "positive-icon" : "negative-icon"
                    }
                ]
            })
            .ToList();

        // Add base currency summary if needed  
        if (!allPriceUnitsAreBaseCurrency)
        {
            // Calculate variables BEFORE the object initializer  
            var totalDebit = _items.Where(x => x.TransactionType == TransactionType.Debit).Sum(x => x.BaseCurrencyAmount);
            var totalCredit = _items.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.BaseCurrencyAmount);
            var net = totalDebit - totalCredit;

            sections.Add(new SummarySection
            {
                Title = "جمع کل (ارز پایه)",
                Items =
                [
                    new SummaryItem
                    {
                        Label = "مجموع بدهکار",
                        Value = totalDebit.ToString("N0"),
                        Type = "debit",
                        ShowIcon = true,
                        IconType = "debit-icon"
                    },

                    new SummaryItem
                    {
                        Label = "مجموع بستانکار",
                        Value = totalCredit.ToString("N0"),
                        Type = "credit",
                        ShowIcon = true,
                        IconType = "credit-icon"
                    },

                    new SummaryItem
                    {
                        Label = "خالص",
                        Value = $"{Math.Abs(net):N0} {(net >= 0 ? "بدهکار" : "بستانکار")}",
                        Type = "net",
                        ShowIcon = true,
                        IconType = net >= 0 ? "positive-icon" : "negative-icon"
                    }
                ]
            });
        }

        _summary = new ReportSummaryVm
        {
            Sections = sections
        };
    }

    private string? CalculateAverageExchangeRate(IGrouping<string, LedgerAccountStatementRpResponse> group)
    {
        var exchangeRates = group.Where(x => x.ExchangeRate.HasValue).Select(x => x.ExchangeRate!.Value).ToList();

        if (!exchangeRates.Any())
            return null;

        var averageRate = exchangeRates.Average();
        return averageRate > 0 ? $"نرخ تبدیل میانگین: {averageRate:N0}" : null;
    }

    private string GetTransactionClass(LedgerAccountStatementRpResponse context)
    {
        return context.TransactionType is TransactionType.Debit ? "debit" : "credit";
    }

    private string GetTransactionIconClass(LedgerAccountStatementRpResponse context)
    {
        return context.TransactionType is TransactionType.Debit ? "debit-icon" : "credit-icon";
    }
}
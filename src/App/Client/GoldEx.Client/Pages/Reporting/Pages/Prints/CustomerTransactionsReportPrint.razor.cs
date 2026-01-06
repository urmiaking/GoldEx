using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class CustomerTransactionsReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public Guid CustomerId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public Guid? PriceUnitId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? Role { get; set; }

    private CustomerTransactionFilterVm _filter = default!;
    private List<CustomerTransactionRpResponse>? _items;
    private GetCustomerResponse? _customer;
    private ReportSummaryVm? _summary;

    private readonly int _version = new Random().Next(0, 1000);

    protected override void OnInitialized()
    {
        _filter = new CustomerTransactionFilterVm
        {
            CustomerId = CustomerId,
            PriceUnitId = PriceUnitId,
            DateRange = (FromDate.HasValue || ToDate.HasValue)
                ? new DateRange(FromDate, ToDate)
                : null
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadCustomerAsync();
        await LoadReportAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadCustomerAsync()
    {
        await SendRequestAsync<ICustomerService, GetCustomerResponse>(
            action: (s, ct) => s.GetAsync(CustomerId, ct),
            afterSend: response => _customer = response,
            createScope: true);
    }

    private async Task LoadReportAsync()
    {
        var request = _filter.ToRequest();

        _items = await SendRequestAsync<IReportingService, List<CustomerTransactionRpResponse>>(
            action: (s, ct) => s.GetCustomerTransactionsAsync(request, ct),
            createScope: true);

        CalculateSummary();
    }

    private static decimal Signed(TransactionType type, decimal amount)
        => type == TransactionType.Debit ? amount : -amount;

    private static (string text, string iconType, string valueType) FormatNet(decimal net, string priceUnitTitle)
    {
        var abs = Math.Abs(net).ToCurrencyFormat(priceUnitTitle);
        if (net > 0) return ($"{abs} بدهکار", "positive-icon", "positive");
        if (net < 0) return ($"{abs} بستانکار", "negative-icon", "negative");
        return ($"{0m.ToCurrencyFormat(priceUnitTitle)}", "positive-icon", "positive");
    }

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
        if (_items == null || _items.Count == 0)
        {
            _summary = null;
            return;
        }

        var groupedByPriceUnit = _items
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

                return new CustomerTransactionFilterVm.PriceUnitSummary
                {
                    PriceUnitTitle = g.Key,
                    ReceivableBalance = receivableBalance,
                    PayableBalance = payableBalance,
                    NetBalance = netBalance,

                    TotalBaseCurrencyAmount = g.Sum(x => x.BaseCurrencyAmount),

                    ExchangeRateInfo = CalculateAverageExchangeRate(g)
                };
            })
            .OrderBy(x => x.PriceUnitTitle)
            .ToList();

        var allPriceUnitsAreBaseCurrency = _items.All(x => x.ExchangeRate is null);

        var sections = new List<SummarySection>(capacity: groupedByPriceUnit.Count + 1);

        foreach (var pu in groupedByPriceUnit)
        {
            var receivableOutstanding = pu.ReceivableOutstanding;
            var payableOutstanding = pu.PayableOutstanding;
            var net = pu.NetBalance;

            var (netText, netIcon, netValueType) = FormatNet(net, pu.PriceUnitTitle);

            sections.Add(new SummarySection
            {
                Title = pu.PriceUnitTitle,
                Info = pu.ExchangeRateInfo ?? string.Empty,
                Items =
                [
                    new SummaryItem
                {
                    Label = "مانده دریافتنی",
                    Value = receivableOutstanding.ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = "debit",
                    ValueType = "debit-value",
                    ShowIcon = true,
                    IconType = "debit-icon"
                },
                new SummaryItem
                {
                    Label = "مانده پرداختنی",
                    Value = payableOutstanding.ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = "credit",
                    ValueType = "credit-value",
                    ShowIcon = true,
                    IconType = "credit-icon"
                },
                new SummaryItem
                {
                    Label = "خالص",
                    Value = netText,
                    Type = "net",
                    ValueType = netValueType,
                    ShowIcon = true,
                    IconType = netIcon
                }
                ]
            });
        }

        if (!allPriceUnitsAreBaseCurrency)
        {
            var totalBaseReceivable = _items
                .Where(x => x.Role == LedgerAccountRole.Receivable)
                .Sum(x => Signed(x.TransactionType, x.BaseCurrencyAmount));

            var totalBasePayable = _items
                .Where(x => x.Role == LedgerAccountRole.Payable)
                .Sum(x => Signed(x.TransactionType, x.BaseCurrencyAmount));

            var totalBaseNet = totalBaseReceivable + totalBasePayable;

            var totalBaseReceivableOutstanding = Math.Max(totalBaseReceivable, 0m);
            var totalBasePayableOutstanding = Math.Abs(Math.Min(totalBasePayable, 0m));

            var netText = totalBaseNet > 0
                ? $"{Math.Abs(totalBaseNet):N0} بدهکار"
                : totalBaseNet < 0
                    ? $"{Math.Abs(totalBaseNet):N0} بستانکار"
                    : "0";

            sections.Add(new SummarySection
            {
                Title = "جمع کل (ارز پایه)",
                Items =
                [
                    new SummaryItem
                {
                    Label = "مجموع مانده دریافتنی",
                    Value = totalBaseReceivableOutstanding.ToString("N0"),
                    Type = "debit",
                    ValueType = "debit-value",
                    ShowIcon = true,
                    IconType = "debit-icon"
                },
                new SummaryItem
                {
                    Label = "مجموع مانده پرداختنی",
                    Value = totalBasePayableOutstanding.ToString("N0"),
                    Type = "credit",
                    ValueType = "credit-value",
                    ShowIcon = true,
                    IconType = "credit-icon"
                },
                new SummaryItem
                {
                    Label = "خالص",
                    Value = netText,
                    Type = "net",
                    ValueType = totalBaseNet >= 0 ? "positive" : "negative",
                    ShowIcon = true,
                    IconType = totalBaseNet >= 0 ? "positive-icon" : "negative-icon"
                }
                ]
            });
        }

        _summary = new ReportSummaryVm
        {
            Sections = sections
        };
    }

    private string? CalculateAverageExchangeRate(IGrouping<string, CustomerTransactionRpResponse> group)
    {
        var exchangeRates = group.Where(x => x.ExchangeRate.HasValue).Select(x => x.ExchangeRate!.Value).ToList();

        if (!exchangeRates.Any())
            return null;

        var averageRate = exchangeRates.Average();
        return averageRate > 0 ? $"نرخ تبدیل میانگین: {averageRate:N0}" : null;
    }

    private string GetTransactionClass(CustomerTransactionRpResponse context)
    {
        return context.TransactionType is TransactionType.Credit ? "debit" : "credit";
    }

    private string GetTransactionIconClass(CustomerTransactionRpResponse context)
    {
        return context.TransactionType is TransactionType.Credit ? "debit-icon" : "credit-icon";
    }

    private string GetRunningBalance(CustomerTransactionRpResponse item)
    {
        var abs = Math.Abs(item.RunningBalance).ToCurrencyFormat(item.PriceUnitTitle);

        return item.RunningBalance switch
        {
            > 0 => $"{abs} بدهکار",
            < 0 => $"{abs} بستانکار",
            _ => $"{0m.ToCurrencyFormat(item.PriceUnitTitle)}"
        };
    }
}
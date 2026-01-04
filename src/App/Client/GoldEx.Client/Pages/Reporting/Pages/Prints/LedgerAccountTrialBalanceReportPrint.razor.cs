using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class LedgerAccountTrialBalanceReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public Guid? ParentLedgerId { get; set; }

    public string SelectedParentLedger => _ledgerAccount != null ? $" - ({_ledgerAccount.Title})" : string.Empty;

    private readonly int _version = new Random().Next(0, 1000);
    private LedgerAccountTrialBalanceFilterVm _filter = default!;
    private List<LedgerAccountTrialBalanceRpResponse>? _items;
    private GetLedgerAccountResponse? _ledgerAccount;
    private ReportSummaryVm? _summary;

    protected override void OnInitialized()
    {
        _filter = new LedgerAccountTrialBalanceFilterVm
        {
            ParentLedgerId = ParentLedgerId,
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
        if (ParentLedgerId is null)
            return;

        await SendRequestAsync<ILedgerAccountService, GetLedgerAccountResponse>(
            action: (s, ct) => s.GetAsync(ParentLedgerId.Value, ct),
            afterSend: response => _ledgerAccount = response,
            createScope: true);
    }

    private async Task LoadReportAsync()
    {
        var request = _filter.ToRequest();

        _items = await SendRequestAsync<IReportingService, List<LedgerAccountTrialBalanceRpResponse>>(
            action: (s, ct) => s.GetLedgerAccountTrialBalanceAsync(request, ct),
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

        var sections = new List<SummarySection>();

        var totalDebit = _items.Sum(x => x.DebitAmountBase);
        var totalCredit = _items.Sum(x => x.CreditAmountBase);
        var net = totalDebit - totalCredit;

        sections.Add(new SummarySection
        {
            Title = "جمع کل",
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

        _summary = new ReportSummaryVm
        {
            Sections = sections
        };
    }

    private string GetNetAmount(LedgerAccountTrialBalanceRpResponse item)
    {
        var net = item.DebitAmountBase - item.CreditAmountBase;
        return Math.Abs(net).ToString("N0");
    }

    private string? GetBasePriceTitle()
    {
        var basePriceTitle = _items?.FirstOrDefault()?.BasePriceUnitTitle;
        return !string.IsNullOrEmpty(basePriceTitle) ? $"({basePriceTitle})" : null;
    }
}
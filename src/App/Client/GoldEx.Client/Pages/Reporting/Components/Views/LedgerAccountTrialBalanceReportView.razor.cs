using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GoldEx.Client.Pages.Reporting.ViewModels.LedgerAccountTrialBalanceFilterVm;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class LedgerAccountTrialBalanceReportView
{
    [Parameter] public List<LedgerAccountTrialBalanceRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private LedgerAccountTrialBalanceReportSummary? _summary;

    private List<LedgerAccountTrialBalanceRpResponse> _flattenedItems = [];

    protected override void OnParametersSet()
    {
        CalculateSummary();
        _flattenedItems = FlattenHierarchy(Items);
        base.OnParametersSet();
    }

    private void CalculateSummary()
    {
        if (Items == null || !Items.Any())
        {
            _summary = null;
            return;
        }

        _summary = new LedgerAccountTrialBalanceReportSummary
        {
            TotalDebit = Items.Sum(x => x.DebitAmountBase),
            TotalCredit = Items.Sum(x => x.CreditAmountBase),
        };
    }

    private Color GetAccountTypeColor(LedgerAccountType? type)
    {
        return type switch
        {
            LedgerAccountType.Asset => Color.Primary,
            LedgerAccountType.Liability => Color.Tertiary,
            LedgerAccountType.Equity => Color.Info,
            LedgerAccountType.Revenue => Color.Success,
            LedgerAccountType.Expense => Color.Error,
            _ => Color.Default
        };
    }

    private string GetAccountIcon(LedgerAccountTrialBalanceRpResponse? account)
    {
        return account?.LedgerAccountType switch
        {
            LedgerAccountType.Asset => Icons.Material.Filled.AccountBalance,
            LedgerAccountType.Liability => Icons.Material.Filled.CreditCard,
            LedgerAccountType.Equity => Icons.Material.Filled.AccountBalanceWallet,
            LedgerAccountType.Revenue => Icons.Material.Filled.TrendingUp,
            LedgerAccountType.Expense => Icons.Material.Filled.TrendingDown,
            _ => Icons.Material.Filled.AccountBalance
        };
    }

    private List<LedgerAccountTrialBalanceRpResponse> FlattenHierarchy(List<LedgerAccountTrialBalanceRpResponse>? items)
    {
        var result = new List<LedgerAccountTrialBalanceRpResponse>();

        if (items == null) return result;

        foreach (var item in items)
        {
            // Add the current item  
            result.Add(item);

            // Recursively add all children  
            if (item.SubLedgerAccounts.Any())
            {
                result.AddRange(FlattenHierarchy(item.SubLedgerAccounts));
            }
        }

        return result;
    }

    // Simple single-level grouping - MudTable will handle hierarchy via ParentAccountId  
    private TableGroupDefinition<LedgerAccountTrialBalanceRpResponse> GroupDefinition = new()
    {
        GroupName = "Account",
        Selector = x => x.ParentAccountId ?? Guid.Empty,
        Expandable = true,
        Indentation = true
    };
}
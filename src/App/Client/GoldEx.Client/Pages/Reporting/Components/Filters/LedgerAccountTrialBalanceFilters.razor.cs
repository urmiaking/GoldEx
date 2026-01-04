using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class LedgerAccountTrialBalanceFilters
{
    [Parameter] public LedgerAccountTrialBalanceFilterVm Model { get; set; } = default!;

    private List<GetLedgerAccountResponse> _ledgerAccounts = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadLedgerAccountsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadLedgerAccountsAsync()
    {
        await SendRequestAsync<ILedgerAccountService, List<GetLedgerAccountResponse>>(
            (s, ct) => s.GetParentLedgerAccountsAsync(ct),
            response => _ledgerAccounts = response);
    }

    private List<GetLedgerAccountResponse> GetSortedLedgerAccounts()
    {
        var sorted = new List<GetLedgerAccountResponse>();
        var processed = new HashSet<Guid>();

        // Start with root accounts (no parent)  
        var rootAccounts = _ledgerAccounts.Where(x => x.ParentAccount == null)
            .OrderBy(x => x.Title);

        foreach (var account in rootAccounts)
        {
            AddAccountAndChildren(account, sorted, processed);
        }

        return sorted;
    }

    private void AddAccountAndChildren(GetLedgerAccountResponse account,
        List<GetLedgerAccountResponse> result, HashSet<Guid> processed)
    {
        if (processed.Contains(account.Id)) return;

        result.Add(account);
        processed.Add(account.Id);

        // Add children recursively  
        var children = _ledgerAccounts.Where(x => x.ParentAccount?.Id == account.Id)
            .OrderBy(x => x.Title);
        foreach (var child in children)
        {
            AddAccountAndChildren(child, result, processed);
        }
    }

    private int GetIndentLevel(GetLedgerAccountResponse account)
    {
        var level = 0;
        var current = account.ParentAccount;
        while (current != null)
        {
            level++;
            current = current.ParentAccount;
        }
        return level;
    }

    private string GetAccountTypeIcon(LedgerAccountType type)
    {
        return type switch
        {
            LedgerAccountType.Asset => Icons.Material.Filled.AccountBalance,
            LedgerAccountType.Liability => Icons.Material.Filled.CreditCard,
            LedgerAccountType.Equity => Icons.Material.Filled.AccountBalanceWallet,
            LedgerAccountType.Revenue => Icons.Material.Filled.TrendingUp,
            LedgerAccountType.Expense => Icons.Material.Filled.TrendingDown,
            _ => Icons.Material.Filled.AccountBalance
        };
    }

    private Color GetAccountTypeColor(LedgerAccountType type)
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
}
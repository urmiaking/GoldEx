using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class LedgerAccountDetailsFilters
{
    [Parameter] public LedgerAccountDetailsFilterVm Model { get; set; } = default!;

    private List<GetLedgerAccountResponse> _ledgerAccounts = [];
    private List<GetPriceUnitTitleResponse> _priceUnits = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadLedgerAccountsAsync();
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        // When coming back via navigation with query restored
        if (Model.LedgerAccountId.HasValue && _priceUnits.Count == 0)
        {
            await LoadPriceUnitsAsync();
        }

        await base.OnParametersSetAsync();
    }

    private async Task LoadLedgerAccountsAsync()
    {
        await SendRequestAsync<ILedgerAccountService, List<GetLedgerAccountResponse>>(
            (s, ct) => s.GetActiveLedgerAccountsAsync(ct),
            response => _ledgerAccounts = response);
    }

    private async Task LoadPriceUnitsAsync()
    {
        if (!Model.LedgerAccountId.HasValue)
            return;

        var request = new TransactionFilter(
            null,
            null,
            null,
            Model.LedgerAccountId,
            null,
            null,
            false,
            false);

        await SendRequestAsync<ITransactionService, List<GetPriceUnitTitleResponse>>(
            (s, ct) => s.GetAvailablePriceUnitsAsync(request, ct),
            response =>
            {
                _priceUnits = response;
                StateHasChanged();
            });
    }

    private async Task OnLedgerAccountChanged(Guid? ledgerAccountId)
    {
        Model.LedgerAccountId = ledgerAccountId;
        Model.PriceUnitId = null;
        _priceUnits.Clear();

        if (ledgerAccountId.HasValue)
            await LoadPriceUnitsAsync();
    }

    private async Task<IEnumerable<Guid?>> SearchLedgerAccounts(string value, CancellationToken token)
    {
        await Task.Delay(0, token);

        if (string.IsNullOrEmpty(value))
            return _ledgerAccounts.Select(a => (Guid?)a.Id);

        return _ledgerAccounts
            .Where(a => $"{a.AccountType.GetDisplayName()} - {a.Title}"
                .Contains(value, StringComparison.InvariantCultureIgnoreCase))
            .Select(a => (Guid?)a.Id);
    }

    private string GetLedgerAccountDisplayText(Guid? accountId)
    {
        if (!accountId.HasValue)
            return string.Empty;

        var account = _ledgerAccounts.FirstOrDefault(a => a.Id == accountId.Value);
        return account != null
            ? $"{account.AccountType.GetDisplayName()} - {account.Title}"
            : string.Empty;
    }
}
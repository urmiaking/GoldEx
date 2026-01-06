using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class CustomerBalanceFilters
{
    [Parameter] public CustomerBalanceFilterVm Model { get; set; } = default!;

    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private GetPriceUnitTitleResponse? _selectedPriceUnit;

    protected override async Task OnInitializedAsync()
    {
        await LoadPriceUnitsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        var request = new TransactionFilter(
            null,
            null,
            null,
            null,
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

    private void PriceUnitChanged(Guid? id)
    {
        Model.PriceUnitId = id;

        _selectedPriceUnit = id.HasValue ? _priceUnits.FirstOrDefault(x => x.Id == id.Value) : null;
    }

    private Color GetTransactionColor(TransactionType item)
    {
        return item is TransactionType.Debit
            ? Color.Error
            : Color.Success;
    }

    private string GetTransactionIcon(TransactionType item)
    {
        return item is TransactionType.Debit
            ? Icons.Material.Filled.AccountBalanceWallet
            : Icons.Material.Filled.Payments;
    }
}
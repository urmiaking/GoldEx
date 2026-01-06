using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class CustomerTransactionFilters
{
    [Parameter] public CustomerTransactionFilterVm Model { get; set; } = default!;

    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private List<GetCustomerResponse>? _customers;
    private GetCustomerResponse? _selectedCustomer;

    protected override async Task OnParametersSetAsync()
    {
        if (Model.CustomerId.HasValue && Model.CustomerId != Guid.Empty)
        {
            if (_selectedCustomer?.Id != Model.CustomerId.Value)
            {
                await SendRequestAsync<ICustomerService, GetCustomerResponse>(
                    action: (s, ct) => s.GetAsync(Model.CustomerId.Value, ct),
                    afterSend: response => _selectedCustomer = response,
                    cancelPrevious: true);

                await LoadPriceUnitsAsync();
            }
        }

        await base.OnParametersSetAsync();

    }

    private async Task<IEnumerable<GetCustomerResponse>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, null, ct),
            afterSend: response => _customers = response,
            cancelPrevious: true);

        return _customers;
    }

    private async Task OnCustomerChanged(GetCustomerResponse? customer)
    {
        _selectedCustomer = customer;
        Model.CustomerId = customer?.Id;

        await LoadPriceUnitsAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        if (!Model.CustomerId.HasValue)
            return;

        var request = new TransactionFilter(
            null,
            Model.CustomerId.Value,
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

    private Color GetTransactionColor(LedgerAccountRole item)
    {
        return item is LedgerAccountRole.Receivable
            ? Color.Success
            : Color.Primary;
    }

    private string GetTransactionIcon(LedgerAccountRole item)
    {
        return item == LedgerAccountRole.Receivable
            ? Icons.Material.Filled.AccountBalanceWallet
            : Icons.Material.Filled.Payments;
    }
}
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class PaymentFilters
{
    [Parameter] public PaymentFilterVm Model { get; set; } = default!;

    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private List<GetCustomerResponse>? _customers;
    private GetCustomerResponse? _selectedCustomer;

    protected override async Task OnInitializedAsync()
    {
        if (Model.CustomerId.HasValue && Model.CustomerId != Guid.Empty)
        {
            if (_selectedCustomer?.Id != Model.CustomerId.Value)
            {
                await SendRequestAsync<ICustomerService, GetCustomerResponse>(
                    action: (s, ct) => s.GetAsync(Model.CustomerId.Value, ct),
                    afterSend: response => _selectedCustomer = response,
                    cancelPrevious: true);
            }
        }

        await LoadPriceUnitsAsync();

        await base.OnInitializedAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        var request = new TransactionFilter(
            null,
            Model.CustomerId,
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

    private Color GetSideColor(PaymentSide item)
    {
        return item is PaymentSide.Pay
            ? Color.Error
            : Color.Success;
    }

    private string GetSideIcon(PaymentSide item)
    {
        return item is PaymentSide.Pay
            ? Icons.Material.Filled.ArrowUpward
            : Icons.Material.Filled.ArrowDownward;
    }

    private Color GetTypeColor(PaymentType item)
    {
        return item switch
        {
            PaymentType.InternalCash => Color.Success,
            PaymentType.UsedGoldInventory => Color.Error,
            PaymentType.MoltenGoldInventory => Color.Warning,
            PaymentType.CustomerTransfer => Color.Tertiary,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }

    private string GetTypeIcon(PaymentType item)
    {
        return item switch
        {
            PaymentType.InternalCash => Icons.Material.Filled.Payment,
            PaymentType.UsedGoldInventory => Icons.Material.Filled.Recycling,
            PaymentType.MoltenGoldInventory => Icons.Material.Filled.Whatshot,
            PaymentType.CustomerTransfer => Icons.Material.Filled.SwapHoriz,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }
}
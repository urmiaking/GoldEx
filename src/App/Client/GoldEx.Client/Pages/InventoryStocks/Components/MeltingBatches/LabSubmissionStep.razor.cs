using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components.MeltingBatches;

public partial class LabSubmissionStep
{
    [Parameter] public MeltingBatchVm Model { get; set; } = new();
    [Parameter] public EventCallback<MeltingBatchVm> ModelChanged { get; set; }
    [Parameter] public bool Processing { get; set; }
    [Parameter] public EventCallback OnSendToLab { get; set; }

    private List<GetCustomerNameResponse> _assayers = [];

    private async Task<IEnumerable<GetCustomerNameResponse>?> SearchAssayers(string? assayerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assayerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerNameResponse>>(
            action: (s, ct) => s.GetNamesAsync(assayerName, CustomerType.AssayingLab, ct),
            afterSend: response => _assayers = response,
            cancelPrevious: true);

        return _assayers;
    }

    private async Task OnAddAssayer()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true },
            { x => x.CustomerType, CustomerType.AssayingLab },
            { x => x.ShowFinancialAccounts, false }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("افزودن آزمایشگاه جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            Model.Assayer = new GetCustomerNameResponse(customerVm.Id!.Value, customerVm.FullName);
            StateHasChanged();
        }
    }
}
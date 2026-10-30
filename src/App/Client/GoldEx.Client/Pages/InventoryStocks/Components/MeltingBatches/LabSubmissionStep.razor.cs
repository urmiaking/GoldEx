using GoldEx.Client.Components.Services;
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
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;
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
        var dialog = ResponsiveDialogs.Show<Customers.Components.Editor>("افزودن آزمایشگاه جدید",
            new Dictionary<string, object>
            {
                [nameof(Customers.Components.Editor.ReturnModel)] = true,
                [nameof(Customers.Components.Editor.CustomerType)] = CustomerType.AssayingLab,
                [nameof(Customers.Components.Editor.ShowFinancialAccounts)] = false
            }, wide: true);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            Model.Assayer = new GetCustomerNameResponse(customerVm.Id!.Value, customerVm.FullName);
            StateHasChanged();
        }
    }
}
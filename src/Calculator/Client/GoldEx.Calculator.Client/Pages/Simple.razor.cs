using GoldEx.Calculator.Client.Components;
using GoldEx.Calculator.Client.Services;
using GoldEx.Calculator.Client.ViewModels;
using GoldEx.Client.Components.Calculator.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;

namespace GoldEx.Calculator.Client.Pages;

public partial class Simple
{
    private bool _canInstall;
    private int _basketCount;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private QuickInvoiceBasketStore BasketStore { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _canInstall = await JsRuntime.InvokeAsync<bool>("getPwaState");
                if (_canInstall)
                {
                    StateHasChanged();
                } 
            }
            catch 
            {
                _canInstall = false;
            }

            BasketStore.Changed += OnBasketChanged;
            _basketCount = await BasketStore.GetCountAsync();
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async void OnBasketChanged(object? sender, EventArgs e)
    {
        _basketCount = await BasketStore.GetCountAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task InstallPwa()
    {
        try
        {
            var installed = await JsRuntime.InvokeAsync<bool>("installPwa");
            if (installed)
            {
                _canInstall = false;
                StateHasChanged();
            }
        }
        catch
        {
            AddErrorToast("خطایی در نصب رخ داد");
        }
    }

    private async Task HandleAddToInvoiceAsync(QuickInvoicePayload payload)
    {
        await BasketStore.AddAsync(payload);
        _basketCount = await BasketStore.GetCountAsync();
    }

    private async Task FinalizeInvoiceAsync()
    {
        var items = await BasketStore.GetItemsAsync();
        if (items.Count == 0)
            return;

        var dialog = await DialogService.ShowAsync<QuickInvoiceCustomerDialog>(
            title: "اطلاعات مشتری",
            options: new DialogOptions { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;
        if (result is { Canceled: true })
            return;

        var customer = (QuickInvoiceCustomerVm)result.Data!;

        // Apply customer + company info to all items
        items = items
            .Select(x => x
                .WithCustomer(customer.CustomerName, customer.CustomerPhone)
                .WithCompanyInfo(customer.CompanyName, customer.CompanyPhone, customer.CompanyAddress))
            .ToList();

        var json = JsonSerializer.Serialize(items, QuickInvoicePayload.JsonOptions);
        await JsRuntime.InvokeVoidAsync("quickInvoice.printFromPayload", json);

        await BasketStore.ClearAsync();
        _basketCount = 0;
    }

    private async Task OpenBasketDialogAsync()
    {
        var dialog = await DialogService.ShowAsync<QuickInvoiceBasketDialog>(
            title: "اقلام فاکتور",
            options: new DialogOptions { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Medium });

        await dialog.Result;

        _basketCount = await BasketStore.GetCountAsync();
        StateHasChanged();
    }

    public override ValueTask DisposeAsync()
    {
        BasketStore.Changed -= OnBasketChanged;
        return base.DisposeAsync();
    }
}
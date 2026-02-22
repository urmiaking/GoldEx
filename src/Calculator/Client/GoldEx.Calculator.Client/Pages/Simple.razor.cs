using GoldEx.Calculator.Client.Components;
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

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

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
        }

        await base.OnAfterRenderAsync(firstRender);
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

    private async Task HandleProductSoldAsync(QuickInvoicePayload payload)
    {
        var dialog = await DialogService.ShowAsync<QuickInvoiceCustomerDialog>(
            title: "اطلاعات چاپ فاکتور",
            options: new DialogOptions { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;
        if (result is { Canceled: true })
        {
            return;
        }

        var customer = (QuickInvoiceCustomerVm)result.Data!;

        var payloadForPrint = payload.WithCustomer(customer.CustomerName, customer.CustomerPhone)
                                     .WithCompanyInfo(customer.CompanyName, customer.CompanyPhone, customer.CompanyAddress);

        if (!string.IsNullOrWhiteSpace(customer.ProductName))
        {
            payloadForPrint = payloadForPrint with { ProductName = customer.ProductName };
        }

        var json = JsonSerializer.Serialize(payloadForPrint, QuickInvoicePayload.JsonOptions);

        await JsRuntime.InvokeVoidAsync("quickInvoice.printFromPayload", json);
    }
}
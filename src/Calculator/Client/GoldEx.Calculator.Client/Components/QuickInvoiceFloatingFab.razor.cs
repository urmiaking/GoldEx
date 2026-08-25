using System.Globalization;
using System.Text.Json;
using GoldEx.Calculator.Client.Services;
using GoldEx.Calculator.Client.ViewModels;
using GoldEx.Client.Components.Calculator.ViewModels;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Calculator.Client.Components;

public partial class QuickInvoiceFloatingFab : IAsyncDisposable
{
    private int _basketCount;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private QuickInvoiceBasketStore BasketStore { get; set; } = default!;
    [Inject] private QuickInvoiceStore InvoiceStore { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        BasketStore.Changed += OnBasketChanged;
        _basketCount = await BasketStore.GetCountAsync();
        await base.OnInitializedAsync();
    }

    private async void OnBasketChanged(object? sender, EventArgs e)
    {
        _basketCount = await BasketStore.GetCountAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task FinalizeInvoiceAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (authState.User.Identity?.IsAuthenticated is false)
        {
            Snackbar.Add("لطفاً جهت صدور فاکتور وارد حساب کاربری خود شوید", Severity.Info);
            await Task.Delay(1500);
            Navigation.NavigateTo(ClientRoutes.Accounts.Login, forceLoad: true);
            return;
        }

        var items = await BasketStore.GetItemsAsync();
        if (items.Count == 0)
            return;

        var dialog = await DialogService.ShowAsync<QuickInvoiceCustomerDialog>(
            title: "اطلاعات مشتری",
            options: new DialogOptions { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;
        if (result is { Canceled: true })
            return;

        var customer = (QuickInvoiceCustomerVm)result?.Data!;

        var nowPersian = GetPersianDateTimeNow();
        var invoiceNumber = GenerateInvoiceNumber();

        // Apply customer + company info + generated serial & date to all items
        items = items
            .Select(x => x
                .WithCustomer(customer.CustomerName, customer.CustomerPhone)
                .WithCompanyInfo(customer.CompanyName, customer.CompanyPhone, customer.CompanyAddress) with
                {
                    InvoiceNumber = string.IsNullOrWhiteSpace(x.InvoiceNumber) ? invoiceNumber : x.InvoiceNumber,
                    DateTime = string.IsNullOrWhiteSpace(x.DateTime) ? nowPersian : x.DateTime
                })
            .ToList();

        var invoice = QuickInvoice.Create(items);
        await InvoiceStore.AddInvoiceAsync(invoice);

        var json = JsonSerializer.Serialize(items, QuickInvoicePayload.JsonOptions);
        await JsRuntime.InvokeVoidAsync("quickInvoice.printFromPayload", json);

        await BasketStore.ClearAsync();
        _basketCount = 0;
        Snackbar.Add($"فاکتور شماره {invoiceNumber} با موفقیت صادر شد", Severity.Success);
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

    private static string GetPersianDateTimeNow()
    {
        var pc = new PersianCalendar();
        var now = DateTime.Now;
        return $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00} - {now.Hour:00}:{now.Minute:00}";
    }

    private static string GenerateInvoiceNumber()
    {
        var rand = new Random().Next(1000, 9999);
        return $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000000}{rand}";
    }

    public override async ValueTask DisposeAsync()
    {
        BasketStore.Changed -= OnBasketChanged;
        await base.DisposeAsync();
    }
}

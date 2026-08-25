using GoldEx.Calculator.Client.Components;
using GoldEx.Calculator.Client.Services;
using GoldEx.Calculator.Client.ViewModels;
using GoldEx.Client.Components.Calculator.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;
using GoldEx.Shared.Routings;

namespace GoldEx.Calculator.Client.Pages;

public partial class Simple
{
    private bool _canInstall;
    private bool _isLoggedIn;
    private int _basketCount;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private QuickInvoiceBasketStore BasketStore { get; set; } = default!;
    [Inject] private QuickInvoiceStore InvoiceStore { get; set; } = default!;
    [Inject] private CalculationHistoryStore HistoryStore { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _isLoggedIn = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated == true;

        await base.OnInitializedAsync();
    }

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

        await HistoryStore.AddAsync(new CalculationHistoryItem
        {
            Title = payload.ProductName ?? payload.ProductType,
            Category = "طلا و جواهر",
            SummaryText = $"وزن: {payload.Weight} | عیار: {payload.Fineness} | اجرت: {payload.Wage ?? "—"}",
            ResultValue = payload.FinalPrice,
            Unit = string.Empty
        });
    }

    public override ValueTask DisposeAsync()
    {
        BasketStore.Changed -= OnBasketChanged;
        return base.DisposeAsync();
    }
}
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class CoinItemEditor
{
    [Parameter] public CoinItemVm Model { get; set; } = new() { Quantity = 1 };
    [Parameter] public GetPriceUnitTitleResponse? PriceUnit { get; set; }
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;

    private List<GetCoinResponse> _coins = [];
    private MudForm _form = default!;
    private bool _isProcessing;
    private readonly CoinItemValidator _coinItemValidator = new();

    protected override async Task OnParametersSetAsync()
    {
        await LoadCoinAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadCoinAsync()
    {
        await SendRequestAsync<ICoinService, List<GetCoinResponse>>(
            action: (s, ct) => s.GetListAsync(true, ct),
            afterSend: response =>
            {
                _coins = response;
                StateHasChanged();
            });
    }

    public void Close() => MudDialog.Close();

    private async Task Submit()
    {
        _isProcessing = true;
        await _form.Validate();

        if (!_form.IsValid)
        {
            _isProcessing = false;
            return;
        }

        _isProcessing = false;
        MudDialog.Close(DialogResult.Ok(Model));
    }

    private async Task OnCoinChanged(GetCoinResponse? coin)
    {
        if (coin is null)
            return;

        Model.Coin = coin;

        if (PriceUnit is null)
            return;

        await SendRequestAsync<ICoinService, GetExchangeRateResponse?>(
            action: (s, ct) => s.GetPriceAsync(coin.Id, PriceUnit.IsDefault ? null : PriceUnit.Id, ct),
            afterSend: response =>
            {
                if (response is null)
                    return;

                Model.UnitPrice = response.ExchangeRate ?? 0;

                StateHasChanged();
            });
    }
}
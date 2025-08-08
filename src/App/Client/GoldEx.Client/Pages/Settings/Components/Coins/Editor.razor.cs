using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Coins;

public partial class Editor
{
    [Parameter] public CoinVm Model { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private IEnumerable<GetPriceTitleResponse> _prices = [];

    protected override async Task OnParametersSetAsync()
    {
        await LoadPricesAsync();

        if (Model.Id.HasValue)
        {
            Model.Price = _prices.FirstOrDefault(x => x.Id == Model.PriceId);
            StateHasChanged();
        }

        await base.OnParametersSetAsync();
    }

    private async Task LoadPricesAsync()
    {
        await SendRequestAsync<IPriceService, List<GetPriceTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync([MarketType.Coin, MarketType.ParsianCoin], ct),
            afterSend: response => _prices = response);
    }

    private void Close() => MudDialog.Cancel();

    private async Task Submit()
    {
        if (IsBusy)
            return;

        var request = CoinVm.ToRequest(Model);

        await SendRequestAsync<ICoinService>(
            action: (s, ct) => Model.Id.HasValue ? s.UpdateAsync(Model.Id.Value, request, ct) : s.CreateAsync(request, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private void OnPriceChanged(GetPriceTitleResponse? price)
    {
        Model.PriceId = price?.Id;
    }
}
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class CurrencyItemEditor
{
    [Parameter] public CurrencyItemVm Model { get; set; } = new();
    [Parameter] public GetPriceUnitTitleResponse? PriceUnit { get; set; }
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;

    private List<GetPriceUnitTitleResponse> _currencies = [];
    private MudForm _form = default!;
    private bool _isProcessing;
    private readonly CurrencyItemValidator _currencyItemValidator = new();

    protected override async Task OnParametersSetAsync()
    {
        await LoadCurrenciesAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadCurrenciesAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _currencies = response;
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

    private async Task OnCurrencyChanged(GetPriceUnitTitleResponse? currency)
    {
        if (currency is null)
            return;

        Model.Currency = currency;

        if (PriceUnit is null)
            return;

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(currency.Id, PriceUnit.Id, ct),
            afterSend: response =>
            {
                Model.UnitPrice = response.ExchangeRate ?? 0;

                StateHasChanged();
            });
    }
}
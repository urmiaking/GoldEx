using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class UnpaidPriceSelector
{
    [Parameter, EditorRequired] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public GetPriceUnitTitleResponse? MainPriceUnit { get; set; }

    [Parameter] public decimal TotalUnpaidAmount { get; set; }
    [Parameter] public decimal? TotalUnpaidSecondaryAmount { get; set; }

    [Parameter] public GetPriceUnitTitleResponse? UnpaidPriceUnit { get; set; }
    [Parameter] public EventCallback<GetPriceUnitTitleResponse?> UnpaidPriceUnitChanged { get; set; }

    [Parameter] public decimal? ExchangeRate { get; set; }
    [Parameter] public EventCallback<decimal?> ExchangeRateChanged { get; set; }

    /// <summary>
    /// A computed property that determines the UI mode ("main" or "custom").
    /// </summary>
    private string SelectedMode
    {
        get => UnpaidPriceUnit is null ? "main" : "custom";
        set
        {
            if (value == "main")
            {
                _ = SetUnpaidPriceUnitAsync(null);
                _ = SetExchangeRateAsync(null);
            }
            else
            {
                if (UnpaidPriceUnit is null)
                {
                    var defaultUnit = PriceUnits.LastOrDefault(pu => pu.IsGoldBased) ?? PriceUnits.FirstOrDefault();
                    _ = SetUnpaidPriceUnitAsync(defaultUnit);
                }
            }
        }
    }

    /// <summary>
    /// A property to proxy the @bind-Value of the MudSelect.
    /// When the user selects a new currency, it updates the parent and fetches the new exchange rate.
    /// </summary>
    private GetPriceUnitTitleResponse? BoundUnpaidPriceUnit
    {
        get => UnpaidPriceUnit;
        set => _ = SetUnpaidPriceUnitAsync(value);
    }

    /// <summary>
    /// A property to proxy the @bind-Value of the MudNumericField for the exchange rate.
    /// </summary>
    private decimal? BoundExchangeRate
    {
        get => ExchangeRate;
        set => _ = SetExchangeRateAsync(value);
    }

    private async Task SetUnpaidPriceUnitAsync(GetPriceUnitTitleResponse? newUnit)
    {
        if (UnpaidPriceUnit == newUnit) return;

        UnpaidPriceUnit = newUnit;
        await UnpaidPriceUnitChanged.InvokeAsync(newUnit);

        if (newUnit is not null)
        {
            await LoadExchangeRateAsync();
        }
        else
        {
            await SetExchangeRateAsync(null);
        }
    }

    private async Task SetExchangeRateAsync(decimal? newRate)
    {
        if (ExchangeRate == newRate) return;
        await ExchangeRateChanged.InvokeAsync(newRate);
    }

    private async Task LoadExchangeRateAsync()
    {
        if (UnpaidPriceUnit is not null && MainPriceUnit is not null)
        {
            decimal? fetchedRate = null;
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(MainPriceUnit.Id, UnpaidPriceUnit.Id, ct),
                afterSend: response =>
                {
                    fetchedRate = response.ExchangeRate;
                });

            await SetExchangeRateAsync(fetchedRate);
            StateHasChanged();
        }
    }
}
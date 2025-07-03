using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class ExtraCostList
{
    [Parameter] public List<InvoiceExtraCostVm> Items { get; set; } = [];
    [Parameter] public GetPriceUnitTitleResponse PriceUnit { get; set; } = default!;
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];

    protected override void OnParametersSet()
    {
        if (!Items.Any()) 
            AddItem();

        base.OnParametersSet();
    }

    private void AddItem()
    {
        Items.Add(new InvoiceExtraCostVm
        {
            Amount = 0,
            Description = string.Empty,
            AmountAdornmentText = PriceUnit.Title,
            PriceUnit = PriceUnit   
        });
    }

    private void RemoveItem(InvoiceExtraCostVm item)
    {
        switch (Items.Count)
        {
            case > 1:
                Items.Remove(item);
                break;
            case 1:
                Items.First().Amount = 0;
                Items.First().Description = null;
                break;
        }
    }

    private async Task SelectPriceUnit(GetPriceUnitTitleResponse priceUnit, InvoiceExtraCostVm item)
    {
        item.PriceUnit = priceUnit;
        item.AmountAdornmentText = priceUnit.Title;
        item.ExchangeRateLabel = $"نرخ تبدیل {item.PriceUnit.Title} به {PriceUnit.Title}";

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(priceUnit.Id, PriceUnit.Id, ct),
            afterSend: response =>
            {
                if (response.ExchangeRate.HasValue)
                    item.ExchangeRate = response.ExchangeRate.Value;

                StateHasChanged();
            });
    }
}
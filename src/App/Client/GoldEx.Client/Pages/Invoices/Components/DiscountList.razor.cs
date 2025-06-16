using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class DiscountList
{
    [Parameter] public List<InvoiceDiscountVm> Items { get; set; } = [];
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
        Items.Add(new InvoiceDiscountVm
        {
            Amount = 0,
            Description = string.Empty,
            AmountAdornmentText = PriceUnit.Title,
            PriceUnit = PriceUnit
        });
    }

    private void RemoveItem(InvoiceDiscountVm item)
    {
        if (Items.Count > 1)
            Items.Remove(item);
    }

    private async Task SelectPriceUnit(GetPriceUnitTitleResponse priceUnit, InvoiceDiscountVm item)
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
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
    [Parameter] public decimal TotalRemainingAmount { get; set; }

    private decimal TotalRemainingCalculated => TotalRemainingAmount - GetTotalPaid();

    private decimal GetTotalPaid()
    {
        return Items.Sum(x => x.Amount * (x.ExchangeRate ?? 1));
    }

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

    private void OnTotalRemainingClicked()
    {
        var remaining = TotalRemainingCalculated;
        if (remaining <= 0)
            return;

        if (Items.Count == 1 && Items.First().Amount == 0)
        {
            var item = Items.First();

            item.Amount = remaining;
            item.AmountAdornmentText = PriceUnit.Title;
            item.PriceUnit = PriceUnit;
        }
        else if (Items.Count > 1 && Items.Last().Amount == 0)
        {
            var item = Items.Last();

            item.Amount = remaining;
            item.AmountAdornmentText = PriceUnit.Title;
            item.PriceUnit = PriceUnit;
        }
        else
        {
            Items.Add(new InvoiceDiscountVm
            {
                Amount = remaining,
                AmountAdornmentText = PriceUnit.Title,
                PriceUnit = PriceUnit
            });
        }
    }
}
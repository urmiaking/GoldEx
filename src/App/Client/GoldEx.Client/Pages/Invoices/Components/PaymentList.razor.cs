using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Settings.Components.PaymentMethods;
using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class PaymentList
{
    [Parameter] public List<InvoicePaymentVm> Items { get; set; } = [];
    [Parameter] public GetPriceUnitTitleResponse PriceUnit { get; set; } = default!;
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public decimal TotalInvoiceAmount { get; set; }

    private List<GetPaymentMethodResponse> _paymentMethods = [];

    private decimal GetTotalPaid()
    {
        return Items.Sum(x => x.Amount * (x.ExchangeRate ?? 1));
    }

    private decimal TotalRemainingCalculated => TotalInvoiceAmount - GetTotalPaid();

    protected override async Task OnParametersSetAsync()
    {
        if (!_paymentMethods.Any()) 
            await LoadPaymentMethodsAsync();

        await base.OnParametersSetAsync();
    }

    protected override void OnInitialized()
    {
        if (!Items.Any())
            AddItem();

        base.OnInitialized();
    }

    private async Task LoadPaymentMethodsAsync()
    {
        await SendRequestAsync<IPaymentMethodService, List<GetPaymentMethodResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _paymentMethods = response;
                StateHasChanged();
            });
    }

    private void AddItem()
    {
        Items.Add(new InvoicePaymentVm
        {
            Amount = 0m,
            Note = string.Empty,
            PaymentDate = DateTime.Now,
            PriceUnit = PriceUnit,
            AmountAdornmentText = PriceUnit.Title
        });
    }

    private void RemoveItem(InvoicePaymentVm item)
    {
        switch (Items.Count)
        {
            case > 1:
                Items.Remove(item);
                break;
            case 1:
                Items.First().Amount = 0;
                Items.First().Note = null;
                Items.First().ReferenceNumber = null;
                break;
        }
    }

    private void OnAmountChanged(decimal? amount, InvoicePaymentVm item)
    {
        if (amount.HasValue)
            item.Amount = amount.Value;
    }

    private async Task SelectPriceUnit(GetPriceUnitTitleResponse priceUnit, InvoicePaymentVm item)
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
            Items.Add(new InvoicePaymentVm
            {
                Amount = remaining,
                AmountAdornmentText = PriceUnit.Title,
                PriceUnit = PriceUnit,
                PaymentDate = DateTime.Now
            });
        }
    }

    private async Task OnAddPaymentMethod()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var dialog = await DialogService.ShowAsync<Editor>("افزودن روش پرداخت جدید", dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: true })
        {
            await LoadPaymentMethodsAsync();
            StateHasChanged();
        }
    }
}
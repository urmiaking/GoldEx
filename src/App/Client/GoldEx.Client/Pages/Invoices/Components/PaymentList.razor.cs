using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class PaymentList
{
    [Parameter] public List<InvoicePaymentVm> Items { get; set; } = [];
    [Parameter] public GetPriceUnitTitleResponse PriceUnit { get; set; } = default!;
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];

    private List<GetPaymentMethodResponse> _paymentMethods = [];

    protected override async Task OnParametersSetAsync()
    {
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
        if (Items.Count > 1)
            Items.Remove(item);
    }

    private void OnAmountChanged(decimal? amount, InvoicePaymentVm item)
    {
        if (amount.HasValue)
            item.Amount = amount.Value;
    }

    private void SelectPriceUnit(GetPriceUnitTitleResponse priceUnit, InvoicePaymentVm item)
    {
        item.PriceUnit = priceUnit;
        item.AmountAdornmentText = priceUnit.Title;
        item.ExchangeRateLabel = $"نرخ تبدیل {item.PriceUnit.Title} به {PriceUnit.Title}";
    }
}
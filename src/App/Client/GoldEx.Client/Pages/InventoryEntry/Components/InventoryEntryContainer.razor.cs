using GoldEx.Client.Pages.InventoryEntry.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.InventoryEntry.Components;

public partial class InventoryEntryContainer
{
    private readonly InventoryEntryVm _model = new();
    private GetPriceUnitResponse? _priceUnit;
    private decimal? _gramPrice;
    private readonly string _jsVersion = new Random().Next(1, 1000).ToString();

    [Parameter] public string? Class { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadDefaultPriceUnitAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadDefaultPriceUnitAsync()
    {
        await SendRequestAsync<IPriceUnitService, GetPriceUnitResponse?>(
            action: (s, ct) => s.GetDefaultAsync(ct),
            afterSend: response => _priceUnit = response);
    }

    private async Task OnSubmitAsync()
    {
        if (_priceUnit is null)
        {
            AddErrorToast("واحد قیمت پیش فرض بارگذاری نشده است.");
            return;
        }

        try
        {
            _model.ToRequest(_gramPrice ?? 0);
        }
        catch(Exception ex)
        {
            AddErrorToast(ex.Message);
            return;
        }

        var parts = new List<string>();

        if (_model.ProductItems.Count > 0)
            parts.Add($"{_model.ProductItems.Count} جنس");

        if (_model.CoinItems.Count > 0)
            parts.Add($"{_model.CoinItems.Count} سکه");

        if (_model.CurrencyItems.Count > 0)
            parts.Add($"{_model.CurrencyItems.Count} ارز");

        var promptDescription = $"آیا از ثبت ورود موجودی {string.Join("، ", parts)} اطمینان دارید؟";

        var result = await DialogService.ShowMessageBox(
            "تأیید نهایی",
            promptDescription,
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            var request = _model.ToRequest(_gramPrice ?? 0);
            await SendRequestAsync<IInventoryEntryService>(
                action: (s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    AddSuccessToast("ورود موجودی با موفقیت انجام شد.");
                    Navigation.NavigateTo(ClientRoutes.InventoryStocks.List);
                    return Task.CompletedTask;
                });
        }
    }

    private void OnGramPriceChanged(decimal gramPrice)
    {
        _gramPrice = gramPrice;
    }
}
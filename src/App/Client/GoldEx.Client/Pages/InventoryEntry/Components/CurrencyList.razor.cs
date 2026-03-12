using GoldEx.Client.Pages.InventoryEntry.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryEntry.Components;

public partial class CurrencyList
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

    [Parameter, EditorRequired] public GetPriceUnitResponse? PriceUnit { get; set; }
    [Parameter, EditorRequired] public InventoryEntryVm Model { get; set; } = default!;

    private async Task RemoveItem(CurrencyItemVm context)
    {
        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار",
            $"آیا برای حذف ارز {context.Currency.Title} مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            Model.CurrencyItems.Remove(context);
            StateHasChanged();
        }
    }

    private async Task AddItem()
    {
        if (PriceUnit is null)
            return;

        var parameters = new DialogParameters<CurrencyItemEditor>
        {
            { x => x.InvoiceType, InvoiceType.Purchase },
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, false, false) }
        };

        var dialog = await DialogService.ShowAsync<CurrencyItemEditor>("افزودن ارز جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CurrencyItemVm currencyItem })
        {
            currencyItem.RecalculateAmounts();
            currencyItem.Index = GetLastItemIndexNumber() + 1;
            Model.CurrencyItems.Add(currencyItem);

            StateHasChanged();
        }
    }

    public int GetLastItemIndexNumber()
    {
        return Model.CurrencyItems.Count > 0 ? Model.CurrencyItems.Max(i => i.Index) : 0;
    }

    private async Task EditItem(CurrencyItemVm context)
    {
        if (PriceUnit is null)
            return;

        var parameters = new DialogParameters<CurrencyItemEditor>
        {
            { x => x.Model, context },
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, false, false) }
        };

        var dialog = await DialogService.ShowAsync<CurrencyItemEditor>("ویرایش سکه", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CurrencyItemVm resultItem })
        {
            context.UpdateFrom(resultItem);

            StateHasChanged();
        }
    }
}
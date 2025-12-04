using GoldEx.Client.Pages.InventoryEntry.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryEntry.Components;

public partial class CoinList
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };

    [Parameter, EditorRequired] public GetPriceUnitResponse? PriceUnit { get; set; }
    [Parameter, EditorRequired] public InventoryEntryVm Model { get; set; } = default!;

    private async Task RemoveItem(CoinItemVm context)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            $"آیا برای حذف سکه {context.Coin.Title} مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            Model.CoinItems.Remove(context);
            StateHasChanged();
        }
    }

    private async Task AddItem()
    {
        if (PriceUnit is null)
            return;

        var parameters = new DialogParameters<CoinItemEditor>
        {
            { x => x.InvoiceType, InvoiceType.Purchase },
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, false, false) }
        };

        var dialog = await DialogService.ShowAsync<CoinItemEditor>("افزودن سکه جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CoinItemVm coinItem })
        {
            coinItem.RecalculateAmounts();
            coinItem.Index = GetLastItemIndexNumber() + 1;
            Model.CoinItems.Add(coinItem);

            StateHasChanged();
        }
    }

    public int GetLastItemIndexNumber()
    {
        return Model.CoinItems.Count > 0 ? Model.CoinItems.Max(i => i.Index) : 0;
    }

    private async Task EditItem(CoinItemVm context)
    {
        if (PriceUnit is null)
            return;

        var parameters = new DialogParameters<CoinItemEditor>
        {
            { x => x.Model, context },
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, false, false) }
        };

        var dialog = await DialogService.ShowAsync<CoinItemEditor>("ویرایش سکه", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CoinItemVm resultItem })
        {
            context.UpdateFrom(resultItem);

            StateHasChanged();
        }
    }
}
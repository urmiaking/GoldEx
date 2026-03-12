using GoldEx.Client.Pages.InventoryExits.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryExits.Components;

public partial class CurrencyList
{
    [Parameter, EditorRequired] public InventoryExitVm Model { get; set; } = null!;
    [Parameter, EditorRequired] public GetPriceUnitResponse? PriceUnit { get; set; }

    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

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
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, true, false) }
        };

        var dialog = await DialogService.ShowAsync<CurrencyItemEditor>("ویرایش سکه", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CurrencyItemVm resultItem })
        {
            context.UpdateFrom(resultItem);
            StateHasChanged();
        }
    }

    private async Task AddItem()
    {
        if (PriceUnit is null)
            return;

        var parameters = new DialogParameters<InventoryItemSelector>
        {
            { x => x.GramPrice, 1 },
            { x => x.TaxPercent, 0 },
            { x => x.GoldProfitPercent, 0 },
            { x => x.JewelryProfitPercent, 0 },
            { x => x.SelectableTypes, [ItemType.Currency]},
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, true, false) },
            { x => x.ItemStatus, ItemStatus.Available }
        };

        var dialog = await DialogService.ShowAsync<InventoryItemSelector>("انتخاب ارز از انبار", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<CurrencyItemVm> productItems })
        {
            foreach (var item in productItems.Where(item => Model.CurrencyItems.All(x => x.Currency.Id != item.Currency.Id)))
            {
                item.Index = GetLastItemIndexNumber() + 1;
                Model.CurrencyItems.Add(item);
            }

            StateHasChanged();
        }
    }
}
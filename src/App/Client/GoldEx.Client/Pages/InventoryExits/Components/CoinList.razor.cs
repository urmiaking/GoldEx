using GoldEx.Client.Pages.InventoryExits.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.InventoryExits.Components;

public partial class CoinList
{
    [Parameter, EditorRequired] public InventoryExitVm Model { get; set; } = null!;
    [Parameter, EditorRequired] public GetPriceUnitResponse? PriceUnit { get; set; }

    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Large };

    private async Task RemoveItem(CoinItemVm context)
    {
        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار",
            $"آیا برای حذف سکه {context.CoinInstance.Coin?.Title} مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            Model.CoinItems.Remove(context);
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
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, true, false) }
        };

        var dialog = await DialogService.ShowAsync<CoinItemEditor>("ویرایش سکه", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CoinItemVm resultItem })
        {
            context.UpdateFrom(resultItem);
            StateHasChanged();
        }
    }

    private string GetWeight(CoinItemVm context)
    {
        var weight = context.CoinInstance.Weight?.ToWeightFormat(GoldUnitType.Gram);

        var vacuumedWeight = context.CoinInstance.CoinPackage?.VacuumedWeight?.ToWeightFormat(GoldUnitType.Gram);

        return vacuumedWeight is not null
            ? $"{weight} ({vacuumedWeight} با وکیوم)"
            : weight ?? "-";
    }

    private string GetMintYear(CoinItemVm coinItem)
    {
        var mintYear = coinItem.CoinInstance.MintYear;

        if (!mintYear.HasValue)
            return "نامشخص";

        var persianYear = new PersianCalendar().GetYear(mintYear.Value);

        return persianYear.ToString();
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
            { x => x.SelectableTypes, [ItemType.Coin]},
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, true, false) },
            { x => x.ItemStatus, ItemStatus.Available }
        };

        var dialog = await DialogService.ShowAsync<InventoryItemSelector>("انتخاب سکه از انبار", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<CoinItemVm> productItems })
        {
            foreach (var item in productItems.Where(item => Model.CoinItems.All(x => x.CoinInstance.Id != item.CoinInstance.Id)))
            {
                item.Index = GetLastItemIndexNumber() + 1;
                Model.CoinItems.Add(item);
            }

            StateHasChanged();
        }
    }
}
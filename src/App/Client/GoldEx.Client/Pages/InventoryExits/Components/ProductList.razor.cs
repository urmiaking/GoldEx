using GoldEx.Client.Pages.InventoryExits.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryExits.Components;

public partial class ProductList
{
    private GetPriceUnitTitleResponse? _gramPriceUnit;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Large };

    [Parameter, EditorRequired] public GetPriceUnitResponse? PriceUnit { get; set; }
    [Parameter, EditorRequired] public InventoryExitVm Model { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadPriceUnitsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _priceUnits = response;
                _gramPriceUnit = _priceUnits.FirstOrDefault(pu => pu.IsGoldBased);
            });
    }

    private async Task RemoveItem(ProductItemVm context)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            $"آیا برای حذف جنس {context.Product.Name} مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            Model.ProductItems.Remove(context);
            StateHasChanged();
        }
    }

    private async Task EditItem(ProductItemVm context)
    {
        var parameters = new DialogParameters<ProductItemEditor>
        {
            { x => x.Model, context },
            { x => x.PriceUnits, _priceUnits },
            { x => x.PriceUnit, _gramPriceUnit }
        };

        var dialog = await DialogService.ShowAsync<ProductItemEditor>("ویرایش جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: ProductItemVm resultItem })
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
            { x => x.SelectableTypes, [ItemType.Product, ItemType.MoltenGold ]},
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, true, false) },
            { x => x.ItemStatus, ItemStatus.Available }
        };

        var dialog = await DialogService.ShowAsync<InventoryItemSelector>("انتخاب جنس از انبار", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<ProductItemVm> productItems })
        {
            foreach (var item in productItems.Where(item => Model.ProductItems.All(x => x.Product.Id != item.Product.Id)))
            {
                item.Index = GetLastItemIndexNumber() + 1;
                Model.ProductItems.Add(item);
            }

            StateHasChanged();
        }
    }

    public int GetLastItemIndexNumber()
    {
        return Model.ProductItems.Count > 0 ? Model.ProductItems.Max(i => i.Index) : 0;
    }
}
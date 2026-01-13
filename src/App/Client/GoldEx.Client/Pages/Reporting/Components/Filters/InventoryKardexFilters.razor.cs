using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Client.Pages.Reporting.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class InventoryKardexFilters
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Large };
    private string? _selectedItem;

    [Parameter] public InventoryKardexFilterVm Model { get; set; } = default!;

    private async Task OpenInventorySelector()
    {
        var dialog = await DialogService.ShowAsync<InventoryKardexSelector>("انتخاب جنس از انبار", _dialogOptions with { MaxWidth = MaxWidth.Large });

        var result = await dialog.Result;

        Model.ProductId = null;
        Model.CoinInstanceId = null;
        Model.CurrencyId = null;

        if (result is { Canceled: false, Data: InventoryStockVm inventoryStockItem })
        {
            if (inventoryStockItem.Product is not null)
            {
                Model.ProductId = inventoryStockItem.Product.Id;
                _selectedItem = $"{inventoryStockItem.Product.Name} ({inventoryStockItem.Product.Barcode})";
            }
            else if (inventoryStockItem.Coin is not null)
            {
                Model.CoinInstanceId = inventoryStockItem.Coin.Id;
                _selectedItem = $"{inventoryStockItem.Coin.Coin?.Title} ({inventoryStockItem.Coin.Barcode})";
            }
            else if (inventoryStockItem.Currency is not null)
            {
                Model.CurrencyId = inventoryStockItem.Currency.Id;
                _selectedItem = inventoryStockItem.Currency.Title;
            }

            StateHasChanged();
        }
    }
}
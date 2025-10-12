using System.Globalization;
using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class InventoryStockList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;

    [Parameter] public ItemType ItemType { get; set; } = ItemType.Product;
    [Parameter] public ItemStatus ItemStatus { get; set; } = ItemStatus.Available;
    [Parameter] public EventCallback<HashSet<InventoryStockVm>?> SelectedItemsChanged { get; set; }

    [Parameter] public bool Selectable { get; set; }

    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private string _jsVersion = new Random().Next(1, 1000).ToString();
    private MudTable<InventoryStockVm> _table = default!;

    private string? _searchString;
    private DateRange _filterDateRange = new();
    private WarehouseActionType _actionType = WarehouseActionType.In;

    private string DateRangeFilterLabel => ItemStatus switch
    {
        ItemStatus.Available => "تاریخ ثبت جنس",
        ItemStatus.Sold => "تاریخ فروش جنس",
        ItemStatus.Melted => "تاریخ ذوب جنس",
        _ => throw new ArgumentOutOfRangeException()
    };

    private string ItemStatusIcon => ItemStatus switch
    {
        ItemStatus.Available => Icons.Material.Filled.Warehouse,
        ItemStatus.Sold => Icons.Material.Filled.ShoppingBasket,
        ItemStatus.Melted => Icons.Material.Filled.Whatshot,
        _ => throw new ArgumentOutOfRangeException()
    };

    public string? ItemTypeIcon => ItemType switch
    {
        ItemType.Product => Icons.Material.Filled.Diamond,
        ItemType.Coin => Icons.Material.Filled.MonetizationOn,
        ItemType.Currency => Icons.Material.Filled.AttachMoney,
        ItemType.MoltenGold => Icons.Material.Filled.Whatshot,
        ItemType.UsedProduct => Icons.Material.Filled.DiscFull,
        _ => null
    };

    public Color ItemTypeColor => ItemType switch
    {
        ItemType.Product => Color.Info,
        ItemType.Coin => Color.Secondary,
        ItemType.Currency => Color.Tertiary,
        ItemType.MoltenGold => Color.Warning,
        ItemType.UsedProduct => Color.Error,
        _ => Color.Default
    };

    public Color ItemStatusColor => ItemStatus switch
    {
        ItemStatus.Available => Color.Success,
        ItemStatus.Sold => Color.Error,
        ItemStatus.Melted => Color.Warning,
        _ => Color.Default
    };

    private async Task<TableData<InventoryStockVm>> LoadInventoryAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<InventoryStockVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        var inventoryFilter = new InventoryFilter(_actionType, ItemType, _filterDateRange.Start, _filterDateRange.End);

        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
            action: (s, token) => s.GetListAsync(filter, inventoryFilter, token),
            afterSend: response =>
            {
                var items = response.Data.Select(InventoryStockVm.CreateFrom).ToList();

                result = new TableData<InventoryStockVm>
                {
                    TotalItems = response.Total,
                    Items = items
                };
            },
            createScope: true,
            cancelPrevious: true
        );

        return result;
    }

    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private async Task OnDateRangeChanged(DateRange dateRange)
    {
        _filterDateRange = dateRange;
        await RefreshAsync();
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;
        await RefreshAsync();
    }

    private void PageChanged(int i)
    {
        _table.NavigateTo(i - 1);
    }

    private async Task OnPrintBarcode(ProductVm product)
    {
        var labelData = new
        {
            text = product.Barcode,
            name = product.Name,
            weight = "وزن: " + product.Weight?.ToString("G29") + $"{(product.GoldUnitType is GoldUnitType.Gram ? "g" : "m")}",
            wage = "اجرت: " + product.WageType switch
            {
                WageType.Fixed => $"{product.Wage?.ToCurrencyFormat(product.WagePriceUnitTitle)}",
                WageType.Percent => product.Wage?.ToString("G29") + "%",
                _ => "ندارد"
            }
        };

        await JsRuntime.InvokeVoidAsync("printBarcode", labelData);
    }

    private async Task SetStatusFilterText(ItemStatus filterType)
    {
        ItemStatus = filterType;
        _actionType = filterType == ItemStatus.Available
            ? WarehouseActionType.In
            : WarehouseActionType.Out;

        await RefreshAsync();
    }

    private async Task SetItemTypeFilterText(ItemType itemType)
    {
        ItemType = itemType;

        if (ItemType is not ItemType.MoltenGold && ItemStatus is ItemStatus.Melted)
        {
            ItemStatus = ItemStatus.Available;
        }

        await RefreshAsync();
    }

    private void OnViewInvoice(Guid? invoiceId)
    {
        Navigation.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = invoiceId }));
    }

    private string GetBarcodeTooltipText(InventoryStockVm context)
    {
        return context.DateTime.ToString(CultureInfo.CurrentCulture);
    }

    private string? GetWageTooltipText(InventoryStockVm context)
    {
        if (context.SaleWage.HasValue)
        {
            return "اجرت خرید: " + context.Product?.WageType switch
            {
                WageType.Fixed => $"{context.Product.Wage?.ToCurrencyFormat(context.Product.WagePriceUnitTitle)}",
                WageType.Percent => context.Product.Wage?.ToString("G29") + " درصد",
                _ => "ندارد"
            };
        }

        return null;
    }
}
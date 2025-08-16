using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
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
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private string _jsVersion = new Random().Next(1, 1000).ToString();
    private MudTable<InventoryStockVm> _table = default!;

    private string? _searchString;
    private DateRange _filterDateRange = new();
    private WarehouseActionType _actionType = WarehouseActionType.In;
    private ItemType _itemType = ItemType.Product;
    private ItemStatus _itemStatus = ItemStatus.Available;

    private string DateRangeFilterLabel => _itemStatus == ItemStatus.Available ? "تاریخ ثبت جنس" : "تاریخ فروش جنس";
    private string ItemStatusIcon => _itemStatus == ItemStatus.Available ? Icons.Material.Filled.Warehouse : Icons.Material.Filled.ShoppingBasket;
    public string? ItemTypeIcon => _itemType switch
    {
        ItemType.Product => Icons.Material.Filled.Diamond,
        ItemType.Coin => Icons.Material.Filled.MonetizationOn,
        ItemType.Currency => Icons.Material.Filled.AttachMoney,
        _ => null
    };

    public Color ItemTypeColor => _itemType switch
    {
        ItemType.Product => Color.Info,
        ItemType.Coin => Color.Secondary,
        ItemType.Currency => Color.Tertiary,
        _ => Color.Default
    };

    public Color ItemStatusColor => _itemStatus switch
    {
        ItemStatus.Available => Color.Success,
        ItemStatus.Sold => Color.Error,
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

        var inventoryFilter = new InventoryFilter(_actionType, _itemType, _filterDateRange.Start, _filterDateRange.End);

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
            createScope: true
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
            weight = "وزن: " + product.Weight?.ToString("G29") + "g",
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
        _itemStatus = filterType;
        _actionType = filterType == ItemStatus.Available
            ? WarehouseActionType.In
            : WarehouseActionType.Out;

        await RefreshAsync();
    }

    private async Task SetItemTypeFilterText(ItemType itemType)
    {
        _itemType = itemType;
        await RefreshAsync();
    }

    private void OnViewInvoice(Guid? invoiceId)
    {
        Navigation.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = invoiceId }));
    }
}
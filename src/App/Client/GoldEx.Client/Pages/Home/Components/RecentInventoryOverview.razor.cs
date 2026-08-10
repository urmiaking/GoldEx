using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Home.Components;

public partial class RecentInventoryOverview
{
    [Inject] private IInventoryStockService InventoryStockService { get; set; } = default!;

    private List<GetInventoryStockResponse> _items = [];

    private int TotalItemsCount => _items.Count;
    private decimal TotalGoldWeight => _items.Where(x => x.Product != null).Sum(x => x.CurrentAmount);

    private decimal ManufacturedGoldWeight => _items
        .Where(x => x.Product?.ProductType == ProductType.Gold || x.Product?.ProductType == ProductType.Jewelry)
        .Sum(x => x.CurrentAmount);

    private int ManufacturedGoldCount => _items
        .Count(x => x.Product?.ProductType == ProductType.Gold || x.Product?.ProductType == ProductType.Jewelry);

    private decimal MoltenGoldWeight => _items
        .Where(x => x.Product?.ProductType == ProductType.MoltenGold)
        .Sum(x => x.CurrentAmount);

    private int MoltenGoldCount => _items
        .Count(x => x.Product?.ProductType == ProductType.MoltenGold);

    private decimal UsedGoldWeight => _items
        .Where(x => x.Product?.ProductType == ProductType.UsedGold)
        .Sum(x => x.CurrentAmount);

    private int UsedGoldCount => _items
        .Count(x => x.Product?.ProductType == ProductType.UsedGold);

    private int CoinsCount => _items.Count(x => x.Coin != null);
    private int CurrenciesCount => _items.Count(x => x.Currency != null);

    private decimal AverageGoldWeightPerItem => ManufacturedGoldCount > 0 ? ManufacturedGoldWeight / ManufacturedGoldCount : 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadSummaryInventoryAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSummaryInventoryAsync()
    {
        var filter = new RequestFilter(0, 100, null, null, Sdk.Common.Definitions.SortDirection.Descending);
        var inventoryFilter = new InventoryFilter(WarehouseActionType.In, null, null, null, null, null, null);

        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
            action: (service, token) => service.GetListAsync(filter, inventoryFilter, token),
            afterSend: response => _items = response.Data,
            createScope: true
        );
    }
}

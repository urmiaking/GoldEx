using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Home.Components;

public partial class RecentInventoryOverview
{
    [Inject] private IInventoryStockService InventoryStockService { get; set; } = default!;

    private List<GetInventoryStockResponse> _manufacturedItems = [];
    private List<GetInventoryStockResponse> _moltenItems = [];
    private List<GetInventoryStockResponse> _usedItems = [];
    private List<GetInventoryStockResponse> _currencyItems = [];
    private List<CurrencyStockSummary> _currencySummaries = [];
    private int _coinsCount;
    private int _currenciesCount;

    private int CoinsCount => _coinsCount;
    private int CurrenciesCount => _currenciesCount;

    private decimal ManufacturedGoldWeight => _manufacturedItems.Sum(x => x.CurrentAmount);
    private int ManufacturedGoldCount => _manufacturedItems.Count;

    private decimal MoltenGoldWeight => _moltenItems.Sum(x => x.CurrentAmount);
    private int MoltenGoldCount => _moltenItems.Count;

    private decimal UsedGoldWeight => _usedItems.Sum(x => x.CurrentAmount);
    private int UsedGoldCount => _usedItems.Count;

    private decimal TotalGoldWeight => ManufacturedGoldWeight + MoltenGoldWeight + UsedGoldWeight;
    private int TotalItemsCount => ManufacturedGoldCount + MoltenGoldCount + UsedGoldCount + CoinsCount + CurrenciesCount;

    private decimal AverageGoldWeightPerItem => ManufacturedGoldCount > 0 ? ManufacturedGoldWeight / ManufacturedGoldCount : 0;

    private bool _isLoaded;

    protected override async Task OnInitializedAsync()
    {
        await LoadSummaryInventoryAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSummaryInventoryAsync()
    {
        var filter = new RequestFilter(0, 200, null, null, Sdk.Common.Definitions.SortDirection.Descending);

        // 1. Fetch Manufactured Gold (طلا و جواهر)
        var productFilter = new InventoryFilter(WarehouseActionType.In, ItemType.Product, null, null, null, null, null);
        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
            action: (service, token) => service.GetListAsync(filter, productFilter, token),
            afterSend: response => _manufacturedItems = response.Data,
            createScope: true
        );

        // 2. Fetch Molten Gold (طلای آبشده)
        var moltenFilter = new InventoryFilter(WarehouseActionType.In, ItemType.MoltenGold, null, null, null, null, null);
        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
            action: (service, token) => service.GetListAsync(filter, moltenFilter, token),
            afterSend: response => _moltenItems = response.Data,
            createScope: true
        );

        // 3. Fetch Used Gold (طلای مستعمل)
        var usedFilter = new InventoryFilter(WarehouseActionType.In, ItemType.UsedProduct, null, null, null, null, null);
        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
            action: (service, token) => service.GetListAsync(filter, usedFilter, token),
            afterSend: response => _usedItems = response.Data,
            createScope: true
        );

        // 4. Fetch Coins (سکه)
        var coinFilter = new InventoryFilter(WarehouseActionType.In, ItemType.Coin, null, null, null, null, null);
        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
            action: (service, token) => service.GetListAsync(filter, coinFilter, token),
            afterSend: response => _coinsCount = response.Total,
            createScope: true
        );

        // 5. Fetch Currencies (ارز) & Group by Currency Title
        var currencyFilter = new InventoryFilter(WarehouseActionType.In, ItemType.Currency, null, null, null, null, null);
        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
            action: (service, token) => service.GetListAsync(filter, currencyFilter, token),
            afterSend: response =>
            {
                _currencyItems = response.Data;
                _currenciesCount = response.Total;
                _currencySummaries = response.Data
                    .Where(x => x.Currency != null && x.CurrentAmount > 0)
                    .GroupBy(x => x.Currency!.Title)
                    .Select(g => new CurrencyStockSummary
                    {
                        Title = g.Key,
                        Amount = g.Sum(x => x.CurrentAmount)
                    })
                    .OrderByDescending(x => x.Amount)
                    .ToList();
            },
            createScope: true
        );

        _isLoaded = true;
    }
}

public class CurrencyStockSummary
{
    public string Title { get; set; } = default!;
    public decimal Amount { get; set; }
}

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

    private GetInventoryOverviewResponse _overview = new();

    private decimal ManufacturedGoldWeight => _overview.ManufacturedGoldWeight;
    private int ManufacturedGoldCount => _overview.ManufacturedGoldCount;

    private decimal MoltenGoldWeight => _overview.MoltenGoldWeight;
    private int MoltenGoldCount => _overview.MoltenGoldCount;

    private decimal UsedGoldWeight => _overview.UsedGoldWeight;
    private int UsedGoldCount => _overview.UsedGoldCount;

    private int CoinsCount => _overview.CoinsCount;
    private decimal CoinsTotalQuantity => _overview.CoinsTotalQuantity;

    private int CurrenciesCount => _overview.CurrenciesCount;
    private List<CurrencyStockSummaryDto> _currencySummaries => _overview.CurrencySummaries;

    private decimal TotalGoldWeight => ManufacturedGoldWeight + MoltenGoldWeight + UsedGoldWeight;
    private int TotalItemsCount => ManufacturedGoldCount + MoltenGoldCount + UsedGoldCount + CoinsCount;

    private decimal AverageGoldWeightPerItem => ManufacturedGoldCount > 0 ? ManufacturedGoldWeight / ManufacturedGoldCount : 0;

    private bool _isLoaded;

    protected override async Task OnInitializedAsync()
    {
        await LoadSummaryInventoryAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSummaryInventoryAsync()
    {
        await SendRequestAsync<IInventoryStockService, GetInventoryOverviewResponse>(
            action: (service, token) => service.GetInventoryOverviewAsync(token),
            afterSend: response =>
            {
                _overview = response;
            },
            createScope: true
        );

        _isLoaded = true;
    }
}

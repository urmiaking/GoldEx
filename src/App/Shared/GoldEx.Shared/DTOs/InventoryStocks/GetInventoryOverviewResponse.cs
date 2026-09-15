namespace GoldEx.Shared.DTOs.InventoryStocks;

public class GetInventoryOverviewResponse
{
    public decimal ManufacturedGoldWeight { get; set; }
    public int ManufacturedGoldCount { get; set; }

    public decimal MoltenGoldWeight { get; set; }
    public int MoltenGoldCount { get; set; }

    public decimal UsedGoldWeight { get; set; }
    public int UsedGoldCount { get; set; }

    public int CoinsCount { get; set; }
    public decimal CoinsTotalQuantity { get; set; }

    public int CurrenciesCount { get; set; }
    public List<CurrencyStockSummaryDto> CurrencySummaries { get; set; } = [];
}

public class CurrencyStockSummaryDto
{
    public string Title { get; set; } = default!;
    public decimal Amount { get; set; }
}

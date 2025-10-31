using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.InventoryStocks.ViewModels;

public class InventoryStockVm
{
    public decimal CurrentAmount { get; set; }
    public decimal SoldAmount { get; set; }
    public decimal? SaleWage { get; init; }
    public WageType? SaleWageType { get; init; }
    public string? SaleWagePriceUnitTitle { get; init; }

    public ProductVm? Product { get; set; }
    public CoinVm? Coin { get; set; }
    public PriceUnitVm? Currency { get; set; }

    public DateTime DateTime { get; set; }

    public bool ShowDetails { get; set; }

    public static InventoryStockVm CreateFrom(GetInventoryStockResponse response)
    {
        return new InventoryStockVm
        {
            CurrentAmount = response.CurrentAmount,
            SoldAmount = response.SoldAmount,
            DateTime = response.DateTime,
            SaleWage = response.SaleWage,
            SaleWageType = response.SaleWageType,
            SaleWagePriceUnitTitle = response.SaleWagePriceUnitTitle,
            Product = response.Product != null ? ProductVm.CreateFrom(response.Product) : null,
            Coin = response.Coin != null ? CoinVm.CreateFrom(response.Coin) : null,
            Currency = response.Currency != null ? PriceUnitVm.CreateFromTitleResponse(response.Currency) : null
        };
    }
}
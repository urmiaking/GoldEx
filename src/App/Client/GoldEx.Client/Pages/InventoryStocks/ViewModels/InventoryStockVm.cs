using GoldEx.Client.Pages.Invoices.ViewModels;
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

    public decimal? PurchaseWage { get; init; }
    public WageType? PurchaseWageType { get; init; }
    public string? PurchaseWagePriceUnitTitle { get; init; }

    public ProductVm? Product { get; set; }
    public CoinInstanceVm? Coin { get; set; }
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
            PurchaseWage = response.PurchaseWage,
            PurchaseWageType = response.PurchaseWageType,
            PurchaseWagePriceUnitTitle = response.PurchaseWagePriceUnitTitle,
            Product = response.Product != null ? ProductVm.CreateFrom(response.Product) : null,
            Coin = response.Coin != null ? CoinInstanceVm.CreateFrom(response.Coin) : null,
            Currency = response.Currency != null ? PriceUnitVm.CreateFromTitleResponse(response.Currency) : null
        };
    }
}
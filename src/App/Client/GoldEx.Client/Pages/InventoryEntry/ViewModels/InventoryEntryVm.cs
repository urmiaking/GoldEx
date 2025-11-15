using GoldEx.Client.Pages.Invoices.ViewModels;

namespace GoldEx.Client.Pages.InventoryEntry.ViewModels;

public class InventoryEntryVm
{
    public List<ProductItemVm> ProductItems { get; set; } = [];
    public List<CoinItemVm> CoinItems { get; set; } = [];
    public List<CurrencyItemVm> CurrencyItems { get; set; } = [];
}
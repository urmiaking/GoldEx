using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.InventoryEntries;

namespace GoldEx.Client.Pages.InventoryEntry.ViewModels;

public class InventoryEntryVm
{
    public List<ProductItemVm> ProductItems { get; set; } = [];
    public List<CoinItemVm> CoinItems { get; set; } = [];
    public List<CurrencyItemVm> CurrencyItems { get; set; } = [];

    public CreateInventoryEntryRequest ToRequest(decimal gramPrice)
    {
        List<CreateProductItemEntryRequest> productItems = [];
        List<CreateCoinItemEntryRequest> coinItems = [];
        List<CreateCurrencyItemEntryRequest> currencyItems = [];

        productItems.AddRange(ProductItems.Select(productItemVm => ProductItemVm.ToInventoryEntryRequest(productItemVm, gramPrice)));
        coinItems.AddRange(CoinItems.Select(CoinItemVm.ToInventoryEntryRequest));
        currencyItems.AddRange(CurrencyItems.Select(CurrencyItemVm.ToInventoryEntryRequest));

        return new CreateInventoryEntryRequest(productItems, currencyItems, coinItems);
    }
}
using GoldEx.Shared.DTOs.InventoryEntries;

namespace GoldEx.Client.Pages.InventoryEntry.ViewModels;

public class InventoryEntryListVm
{
    public Guid Id { get; set; }
    public DateTime OperationDate { get; set; }
    public decimal ProductsAmount { get; set; }
    public decimal CoinsAmount { get; set; }
    public decimal CurrenciesAmount { get; set; }

    public bool ShowDetails { get; set; }

    public static InventoryEntryListVm CreateFrom(InventoryEntryResponse response)
    {
        return new InventoryEntryListVm
        {
            Id = response.Id,
            OperationDate = response.OperationDate,
            ProductsAmount = response.ProductsAmount,
            CoinsAmount = response.CoinsAmount,
            CurrenciesAmount = response.CurrenciesAmount
        };
    }
}
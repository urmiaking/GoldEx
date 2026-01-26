using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.InventoryExits.ViewModels;

public class InventoryExitListVm
{
    public Guid Id { get; set; }
    public DateTime OperationDate { get; set; }
    public DateTime ExitDate { get; set; }
    public ExitReason ExitReason { get; set; }
    public string? Description { get; set; }
    public decimal ProductsAmount { get; set; }
    public decimal CoinsAmount { get; set; }
    public decimal CurrenciesAmount { get; set; }

    public bool ShowDetails { get; set; }

    public static InventoryExitListVm CreateFrom(InventoryExitResponse response)
    {
        return new InventoryExitListVm
        {
            Id = response.Id,
            OperationDate = response.OperationDate,
            ProductsAmount = response.ProductsAmount,
            CoinsAmount = response.CoinsAmount,
            CurrenciesAmount = response.CurrenciesAmount,
            ExitDate = response.ExitDate,
            ExitReason = response.ExitReason,
            Description = response.Description
        };
    }
}
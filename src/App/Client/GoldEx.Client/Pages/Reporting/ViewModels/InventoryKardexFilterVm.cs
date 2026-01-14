using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class InventoryKardexFilterVm : ReportFilterVmBase
{
    [Display(Name = "جنس")]
    public Guid? ProductId { get; set; }

    [Display(Name = "سکه")]
    public Guid? CoinInstanceId { get; set; }

    [Display(Name = "ارز")]
    public Guid? CurrencyId { get; set; }

    public InventoryKardexRpRequest ToRequest()
    {
        return new InventoryKardexRpRequest(ProductId,
            CoinInstanceId,
            CurrencyId,
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd());
    }

    public class InventoryKardexReportSummary
    {
        public decimal TotalRemaining => TotalIn - TotalOut;
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public string? Unit { get; set; }
    }

    public ItemType GetItemType()
    {
        if (ProductId.HasValue)
            return ItemType.Product;

        if (CoinInstanceId.HasValue)
            return ItemType.Coin;

        if (CurrencyId.HasValue)
            return ItemType.Currency;

        throw new ArgumentOutOfRangeException();
    }

    public Guid GetItemId()
    {
        if (ProductId.HasValue)
            return ProductId.Value;

        if (CoinInstanceId.HasValue)
            return CoinInstanceId.Value;

        if (CurrencyId.HasValue)
            return CurrencyId.Value;

        throw new ArgumentOutOfRangeException();
    }
}
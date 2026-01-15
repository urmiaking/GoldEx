using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class CoinInventoryFilterVm : ReportFilterVmBase
{
    [Display(Name = "وضعیت سکه")]
    public ItemStatus? ItemStatus { get; set; }

    [Display(Name = "نوع سکه")]
    public Guid? CoinId { get; set; }

    public CoinInventoryRpRequest ToRequest()
    {
        return new CoinInventoryRpRequest(ItemStatus,
            CoinId,
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd());
    }

    public class CoinInventoryReportSummary
    {
        public decimal TotalSoldAmount { get; set; }
        public decimal TotalAvailableAmount { get; set; }
        public decimal TotalAmount => TotalAvailableAmount + TotalSoldAmount;
    }
}
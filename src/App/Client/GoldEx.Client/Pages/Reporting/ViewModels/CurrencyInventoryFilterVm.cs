using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class CurrencyInventoryFilterVm : ReportFilterVmBase
{
    [Display(Name = "وضعیت ارز")]
    public ItemStatus? ItemStatus { get; set; }

    public CurrencyInventoryRpRequest ToRequest()
    {
        return new CurrencyInventoryRpRequest(ItemStatus,
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd());
    }

    public class CurrencyInventoryReportSummary
    {
        public decimal TotalSoldAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalRemainingAmount => TotalAmount - TotalSoldAmount;
    }
}
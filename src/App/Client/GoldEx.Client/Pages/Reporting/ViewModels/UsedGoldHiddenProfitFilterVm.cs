using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class UsedGoldHiddenProfitFilterVm : ReportFilterVmBase
{
    [Display(Name = "مشتری")]
    public Guid? CustomerId { get; set; }

    [Display(Name = "واحد ارزی")]
    public Guid? PriceUnitId { get; set; }

    public UsedGoldHiddenProfitRpRequest ToRequest()
    {
        return new UsedGoldHiddenProfitRpRequest(
            CustomerId,
            PriceUnitId,
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd());
    }

    public class UsedGoldHiddenProfitReportSummary
    {
        public List<PriceUnitSummary> PriceUnitSummaries { get; set; } = [];
    }

    public class PriceUnitSummary
    {
        public string PriceUnitTitle { get; set; } = string.Empty;
        public decimal TotalWeight { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalRealValue { get; set; }
        public decimal TotalHiddenProfit { get; set; }
    }
}

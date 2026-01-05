using System.ComponentModel.DataAnnotations;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class CustomerBalanceFilterVm : ReportFilterVmBase
{
    [Display(Name = "واحد ارزی")]
    public Guid? PriceUnitId { get; set; }

    [Display(Name = "حداقل مبلغ از")]
    public decimal? MinimumThreshold { get; set; }

    [Display(Name = "جستجو")]
    public string? SearchQuery { get; set; }

    [Display(Name = "نوع گزارش")]
    public TransactionType? Type { get; set; }

    public CustomerRemainingBalanceRpRequest ToRequest()
    {
        return new CustomerRemainingBalanceRpRequest(
            DateRange?.End?.GetDayEnd(),
            PriceUnitId,
            MinimumThreshold,
            SearchQuery,
            Type
        );
    }

    public class CustomerBalanceReportSummary
    {
        public List<PriceUnitSummary> PriceUnitSummaries { get; set; } = [];
    }

    public class PriceUnitSummary
    {
        public string PriceUnitTitle { get; set; } = string.Empty;
        public decimal TotalPayable { get; set; }
        public decimal TotalReceivable { get; set; }
        public decimal NetAmount => TotalReceivable - TotalPayable;
    }
}
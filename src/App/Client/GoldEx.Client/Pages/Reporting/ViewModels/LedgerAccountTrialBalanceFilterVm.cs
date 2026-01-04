using GoldEx.Shared.DTOs.Reporting;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class LedgerAccountTrialBalanceFilterVm : ReportFilterVmBase
{
    [Display(Name = "نوع حساب")]
    public Guid? ParentLedgerId { get; set; }

    public LedgerAccountTrialBalanceRpRequest ToRequest()
    {
        return new LedgerAccountTrialBalanceRpRequest(
            ParentLedgerId,
            DateRange?.Start != null
            ? new DateTime(DateRange.Start.Value.Year,
                DateRange.Start.Value.Month,
                DateRange.Start.Value.Day,
                0,
                0,
                0)
            : null,
            DateRange?.End != null
            ? new DateTime(DateRange.End.Value.Year,
                DateRange.End.Value.Month,
                DateRange.End.Value.Day,
                23,
                59,
                59)
            : null);
    }

    public class LedgerAccountTrialBalanceReportSummary
    {
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal NetAmount => TotalDebit - TotalCredit;
    }
}
using GoldEx.Shared.DTOs.Reporting;
using System.ComponentModel.DataAnnotations;
using GoldEx.Sdk.Common.Extensions;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class LedgerAccountTrialBalanceFilterVm : ReportFilterVmBase
{
    [Display(Name = "نوع حساب")]
    public Guid? ParentLedgerId { get; set; }

    public LedgerAccountTrialBalanceRpRequest ToRequest()
    {
        return new LedgerAccountTrialBalanceRpRequest(
            ParentLedgerId,
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd());
    }

    public class LedgerAccountTrialBalanceReportSummary
    {
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal NetAmount => TotalDebit - TotalCredit;
    }
}
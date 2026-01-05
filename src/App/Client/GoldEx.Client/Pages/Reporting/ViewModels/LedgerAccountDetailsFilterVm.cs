using System.ComponentModel.DataAnnotations;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public sealed class LedgerAccountDetailsFilterVm : ReportFilterVmBase
{
    [Display(Name = "حساب دفتری")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید.")]
    public Guid? LedgerAccountId { get; set; }

    [Display(Name = "واحد ارزی")]
    public Guid? PriceUnitId { get; set; }

    public LedgerAccountStatementRpRequest ToRequest()
    {
        if (!LedgerAccountId.HasValue)
            throw new InvalidOperationException("LedgerAccountId is required.");

        return new LedgerAccountStatementRpRequest(LedgerAccountId.Value, 
            DateRange?.Start?.GetDayStart(), 
            DateRange?.End?.GetDayEnd(),
            PriceUnitId);
    }

    public class LedgerAccountDetailsReportSummary
    {
        public List<PriceUnitSummary> PriceUnitSummaries { get; set; } = [];
        public decimal TotalBaseCurrencyDebit { get; set; }
        public decimal TotalBaseCurrencyCredit { get; set; }
        public decimal TotalNetBaseCurrency => TotalBaseCurrencyDebit - TotalBaseCurrencyCredit;
        public bool ShowTotalBaseCurrencySummary { get; set; } = true;
    }

    public class PriceUnitSummary
    {
        public string PriceUnitTitle { get; set; } = string.Empty;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalBaseCurrency { get; set; }
        public decimal NetAmount => TotalDebit - TotalCredit;
        public string? ExchangeRateInfo { get; set; }
    }
}
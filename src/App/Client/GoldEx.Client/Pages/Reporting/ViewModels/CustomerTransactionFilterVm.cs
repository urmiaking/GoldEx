using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class CustomerTransactionFilterVm : ReportFilterVmBase
{
    [Display(Name = "طرف حساب")]
    [Required(ErrorMessage = "لطفا طرف حساب را انتخاب کنید.")]
    public Guid? CustomerId { get; set; }

    [Display(Name = "واحد ارزی")]
    public Guid? PriceUnitId { get; set; }

    [Display(Name = "نوع گزارش")] 
    public LedgerAccountRole? Role { get; set; }

    public CustomerTransactionRpRequest ToRequest()
    {
        if (CustomerId is null)
            throw new InvalidOperationException("Customer is required.");

        return new CustomerTransactionRpRequest(
            CustomerId.Value,
            PriceUnitId,
            Role,
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd()
        );
    }

    public class CustomerTransactionsReportSummary
    {
        public List<PriceUnitSummary> PriceUnitSummaries { get; set; } = [];
        public decimal TotalBaseCurrencyPayableRemaining { get; set; }
        public decimal TotalBaseCurrencyReceivableRemaining { get; set; }
        public decimal TotalNetBaseCurrency => TotalBaseCurrencyReceivableRemaining + TotalBaseCurrencyPayableRemaining;
        public bool ShowTotalBaseCurrencySummary { get; set; } = true;
    }

    public class PriceUnitSummary
    {
        public string PriceUnitTitle { get; set; } = default!;
        public decimal ReceivableBalance { get; set; }
        public decimal PayableBalance { get; set; }
        public decimal NetBalance { get; set; }
        public string? ExchangeRateInfo { get; set; }
        public decimal TotalBaseCurrencyAmount { get; set; }

        public decimal ReceivableOutstanding => Math.Max(ReceivableBalance, 0m);
        public decimal PayableOutstanding => Math.Abs(Math.Min(PayableBalance, 0m));
    }
}
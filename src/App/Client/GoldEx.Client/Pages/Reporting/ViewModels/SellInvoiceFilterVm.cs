using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class SellInvoiceFilterVm : ReportFilterVmBase
{
    [Display(Name = "واحد ارزی")]
    public Guid? PriceUnitId { get; set; }

    [Display(Name = "وضعیت فاکتور")] 
    public InvoicePaymentStatus? PaymentStatus { get; set; }

    [Display(Name = "مشتری")]
    public Guid? CustomerId { get; set; }

    public SellInvoiceRpRequest ToRequest()
    {
        return new SellInvoiceRpRequest(
            PaymentStatus,
            PriceUnitId,
            CustomerId,
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd());
    }

    public class SellInvoiceReportSummary
    {
        public List<PriceUnitSummary> PriceUnitSummaries { get; set; } = [];
    }

    public class PriceUnitSummary
    {
        public string PriceUnitTitle { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalWage { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalRemaining { get; set; }
    }
}
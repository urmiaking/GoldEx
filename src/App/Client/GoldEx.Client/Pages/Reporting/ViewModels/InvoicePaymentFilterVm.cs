using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class InvoicePaymentFilterVm : ReportFilterVmBase
{
    [Display(Name = "نوع فاکتور")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public InvoiceType? InvoiceType { get; set; } = Shared.Enums.InvoiceType.Sell;

    [Display(Name = "شماره فاکتور")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public long? InvoiceNumber { get; set; }

    public InvoicePaymentRpRequest ToRequest()
    {
        if (!InvoiceNumber.HasValue || !InvoiceType.HasValue)
            throw new ArgumentException();

        return new InvoicePaymentRpRequest(InvoiceNumber.Value, InvoiceType.Value);
    }

    public class InvoicePaymentReportSummary
    {
        public decimal InvoiceRemainingPrice { get; set; }
        public string PriceUnit { get; set; } = default!;

        public decimal TotalPaidAmount { get; set; }
        public decimal TotalReceivedAmount { get; set; }
    }
}
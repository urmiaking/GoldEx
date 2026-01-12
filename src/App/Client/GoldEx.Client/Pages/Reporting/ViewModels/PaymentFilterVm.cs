using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class PaymentFilterVm : ReportFilterVmBase
{
    [Display(Name = "طرف حساب")]
    public Guid? CustomerId { get; set; }

    [Display(Name = "واحد ارزی")]
    public Guid? PriceUnitId { get; set; }

    [Display(Name = "نوع پرداخت")]
    public PaymentType? PaymentType { get; set; }

    [Display(Name = "نوع گزارش")]
    public PaymentSide? PaymentSide { get; set; }

    public PaymentRpRequest ToRequest()
    {
        return new PaymentRpRequest(PaymentType,
            PaymentSide,
            PriceUnitId,
            CustomerId,
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd());
    }

    public class PaymentReportSummary
    {
        public List<PriceUnitSummary> PriceUnitSummaries { get; set; } = [];
    }

    public class PriceUnitSummary
    {
        public string PriceUnitTitle { get; set; } = string.Empty;
        public decimal TotalReceived { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal NetAmount => TotalReceived - TotalPaid;
        public decimal? AvgExchangeRate { get; set; }
    }
}
using GoldEx.Shared.Enums;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Sdk.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class ProfitAnalysisFilterVm : ReportFilterVmBase
{
    [Display(Name = "نوع کالا")] public ProductType? ProductType { get; set; }
    [Display(Name = "دسته کالا")] public Guid? CategoryId { get; set; }
    [Display(Name = "مشتری")] public Guid? CustomerId { get; set; }

    public ProfitAnalysisRpRequest ToRequest()
        => new(DateRange?.Start?.GetDayStart(), DateRange?.End?.GetDayEnd(), CustomerId, CategoryId, ProductType);
}

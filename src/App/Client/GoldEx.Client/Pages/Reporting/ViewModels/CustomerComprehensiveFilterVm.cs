using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public sealed class CustomerComprehensiveFilterVm : ReportFilterVmBase
{
    [Display(Name = "طرف حساب")]
    [Required(ErrorMessage = "لطفاً مشتری را انتخاب کنید.")]
    public Guid? CustomerId { get; set; }

    [Display(Name = "نمایش جزئیات")]
    public bool IncludeDetails { get; set; } = true;

    public CustomerComprehensiveRpRequest ToRequest() =>
        new(CustomerId ?? throw new InvalidOperationException("Customer is required."),
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd(),
            IncludeDetails);
}

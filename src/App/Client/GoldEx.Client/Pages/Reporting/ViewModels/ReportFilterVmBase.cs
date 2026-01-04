using System.ComponentModel.DataAnnotations;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class ReportFilterVmBase
{
    [Display(Name = "تاریخ")]
    public DateRange? DateRange { get; set; }
}
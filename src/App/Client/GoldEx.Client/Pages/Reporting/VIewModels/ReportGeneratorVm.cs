using GoldEx.Shared.DTOs.Customers;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.VIewModels;

public class ReportGeneratorVm
{
    public DateRange? DateRange { get; set; }

    [Display(Name = "نام طرف حساب")]
    public GetCustomerResponse? Customer { get; set; }
}
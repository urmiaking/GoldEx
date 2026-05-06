using System.ComponentModel.DataAnnotations;

namespace GoldEx.Calculator.Client.ViewModels;

public class QuickInvoiceCustomerVm
{
    [Required(ErrorMessage = "نام مشتری را وارد کنید")]
    public string CustomerName { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }

    [Required(ErrorMessage = "نام فروشگاه را وارد کنید")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره تماس فروشگاه را وارد کنید")]
    public string CompanyPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "آدرس فروشگاه را وارد کنید")]
    public string CompanyAddress { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Customers.ViewModels;

public class InternationalBankAccountVm
{
    [Display(Name = "نام صاحب حساب")]
    public string? AccountHolderName { get; set; }

    [Display(Name = "نام بانک")]
    public string? BankName { get; set; }

    [Display(Name = "شماره حساب")]
    public string? AccountNumber { get; set; }

    [Display(Name = "کد سوئیفت")]
    public string? SwiftBicCode { get; set; }

    [Display(Name = "شماره IBAN")]
    public string? IbanNumber { get; set; }
}
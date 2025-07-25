using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Customers.ViewModels;

public class LocalBankAccountVm
{
    [Display(Name = "نام صاحب حساب")]
    public string? AccountHolderName { get; set; }

    [Display(Name = "نام بانک")]
    public string? BankName { get; set; }

    [Display(Name = "شماره کارت")]
    public string? CardNumber { get; set; }

    [Display(Name = "شماره شبا")]
    public string? ShabaNumber { get; set; }

    [Display(Name = "شماره حساب")]
    public string? AccountNumber { get; set; }
}
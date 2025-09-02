using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.FinancialAccounts.ViewModels;

public class LocalBankAccountVm
{
    [Display(Name = "شماره کارت")]
    public string? CardNumber { get; set; }

    [Display(Name = "شماره شبا")]
    public string? ShabaNumber { get; set; }

    [Display(Name = "شماره حساب")]
    public string? AccountNumber { get; set; }
}
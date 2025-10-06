using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.FinancialAccounts.ViewModels;

public class InternationalBankAccountVm
{
    [Display(Name = "شماره حساب")]
    public string? AccountNumber { get; set; }

    [Display(Name = "کد سوئیفت")]
    public string? SwiftBicCode { get; set; }

    [Display(Name = "شماره IBAN")]
    public string? IbanNumber { get; set; }
}
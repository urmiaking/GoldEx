using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.BankAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Customers.ViewModels;

public class BankAccountVm
{
    public Guid? Id { get; set; }

    [Display(Name = "نوع حساب بانکی")]
    public BankAccountType BankAccountType { get; set; }

    [Display(Name = "نام صاحب حساب")]
    public string? AccountHolderName { get; set; }

    [Display(Name = "نام بانک")]
    public string? BankName { get; set; }

    [Display(Name = "واحد ارزی")]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Display(Name = "شماره کارت")]
    public string? CardNumber { get; set; }

    [Display(Name = "شماره شبا")]
    public string? ShabaNumber { get; set; }

    [Display(Name = "شماره حساب")]
    public string? LocalAccountNumber { get; set; }
    
    [Display(Name = "شماره حساب")]
    public string? InternationalAccountNumber { get; set; }

    [Display(Name = "کد سوئیفت")]
    public string? SwiftBicCode { get; set; }

    [Display(Name = "شماره IBAN")]
    public string? IbanNumber { get; set; }

    public static BankAccountVm CreateFrom(GetBankAccountResponse response)
    {
        return new BankAccountVm
        {
            Id = response.Id,
            AccountHolderName = response.AccountHolderName,
            BankAccountType = response.BankAccountType,
            PriceUnit = response.PriceUnit,
            BankName = response.BankName,
            CardNumber = response.LocalBankAccount?.CardNumber,
            ShabaNumber = response.LocalBankAccount?.ShabaNumber,
            LocalAccountNumber = response.LocalBankAccount?.AccountNumber,
            SwiftBicCode = response.InternationalBankAccount?.SwiftBicCode,
            IbanNumber = response.InternationalBankAccount?.IbanNumber,
            InternationalAccountNumber = response.InternationalBankAccount?.AccountNumber
        };
    }

    public BankAccountRequestDto ToRequest()
    {
        return new BankAccountRequestDto(Id,
            BankAccountType,
            AccountHolderName!,
            BankName!,
            PriceUnit!.Id,
            BankAccountType is BankAccountType.Local
                ? new LocalBankAccountRequestDto(CardNumber!,
                    ShabaNumber!,
                    LocalAccountNumber!)
                : null,
            BankAccountType is BankAccountType.International
                ? new InternationalBankAccountRequestDto(SwiftBicCode!,
                    IbanNumber!,
                    InternationalAccountNumber!)
                : null);
    }
}
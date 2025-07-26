using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Customers.ViewModels;

public class FinancialAccountVm
{
    public Guid? Id { get; set; }

    [Display(Name = "نوع حساب مالی")]
    public FinancialAccountType FinancialAccountType { get; set; }

    [Display(Name = "واحد ارزی")]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    public int Index { get; set; }

    public LocalBankAccountVm? LocalBankAccount { get; set; }
    public InternationalBankAccountVm? InternationalBankAccount { get; set; } 

    public static FinancialAccountVm CreateFrom(GetFinancialAccountResponse response)
    {
        return new FinancialAccountVm
        {
            Id = response.Id,
            FinancialAccountType = response.FinancialAccountType,
            PriceUnit = response.PriceUnit,
            InternationalBankAccount = response.InternationalBankAccount != null
                ? new InternationalBankAccountVm
                {
                    AccountHolderName = response.InternationalBankAccount.AccountHolderName,
                    BankName = response.InternationalBankAccount.BankName,
                    SwiftBicCode = response.InternationalBankAccount.SwiftBicCode,
                    IbanNumber = response.InternationalBankAccount.IbanNumber,
                    AccountNumber = response.InternationalBankAccount.AccountNumber
                }
                : null,
            LocalBankAccount = response.LocalBankAccount != null
                ? new LocalBankAccountVm
                {
                    AccountHolderName = response.LocalBankAccount.AccountHolderName,
                    BankName = response.LocalBankAccount.BankName,
                    CardNumber = response.LocalBankAccount.CardNumber,
                    ShabaNumber = response.LocalBankAccount.ShabaNumber,
                    AccountNumber = response.LocalBankAccount.AccountNumber
                }
                : null
        };
    }

    public FinancialAccountRequestDto ToRequest()
    {
        return new FinancialAccountRequestDto(Id,
            FinancialAccountType,
            PriceUnit!.Id,
            FinancialAccountType is FinancialAccountType.LocalBankAccount && LocalBankAccount != null
                ? new LocalBankAccountRequestDto(
                    LocalBankAccount.AccountHolderName!,
                    LocalBankAccount.BankName!, 
                    LocalBankAccount.CardNumber!,
                    LocalBankAccount.ShabaNumber!,
                    LocalBankAccount.AccountNumber!)
                : null,
            FinancialAccountType is FinancialAccountType.InternationalBankAccount && InternationalBankAccount != null
                ? new InternationalBankAccountRequestDto(
                    InternationalBankAccount.AccountHolderName!,
                    InternationalBankAccount.BankName!, 
                    InternationalBankAccount.SwiftBicCode!,
                    InternationalBankAccount.IbanNumber!,
                    InternationalBankAccount.AccountNumber!)
                : null);
    }
}
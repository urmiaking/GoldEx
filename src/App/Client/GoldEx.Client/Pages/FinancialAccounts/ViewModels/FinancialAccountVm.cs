using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.FinancialAccounts.ViewModels;

public class FinancialAccountVm
{
    public Guid? Id { get; set; }

    [Display(Name = "نوع حساب مالی")]
    public FinancialAccountType FinancialAccountType { get; set; }

    [Display(Name = "نام صاحب حساب")]

    public string? HolderName { get; set; }

    [Display(Name = "نام بانک/کارگزار")]
    public string? BrokerName { get; set; }

    [Display(Name = "واحد ارزی")]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    public int Index { get; set; }
    public bool IsSystemAccount { get; set; }

    public Guid? CustomerId { get; set; }

    public LocalBankAccountVm? LocalBankAccount { get; set; }
    public InternationalBankAccountVm? InternationalBankAccount { get; set; }
    public CashAccountVm? CashAccount { get; set; }

    public static FinancialAccountVm CreateFrom(GetFinancialAccountResponse response)
    {
        return new FinancialAccountVm
        {
            Id = response.Id,
            FinancialAccountType = response.FinancialAccountType,
            BrokerName = response.BrokerName,
            HolderName = response.HolderName,
            PriceUnit = response.PriceUnit,
            InternationalBankAccount = response.InternationalBankAccount != null
                ? new InternationalBankAccountVm
                {
                    SwiftBicCode = response.InternationalBankAccount.SwiftBicCode,
                    IbanNumber = response.InternationalBankAccount.IbanNumber,
                    AccountNumber = response.InternationalBankAccount.AccountNumber
                }
                : null,
            LocalBankAccount = response.LocalBankAccount != null
                ? new LocalBankAccountVm
                {
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
            HolderName,
            BrokerName,
            PriceUnit!.Id,
            CustomerId,
            IsSystemAccount,
            FinancialAccountType is FinancialAccountType.LocalBankAccount && LocalBankAccount != null
                ? new LocalBankAccountRequestDto(
                    LocalBankAccount.CardNumber!,
                    LocalBankAccount.ShabaNumber!,
                    LocalBankAccount.AccountNumber!)
                : null,
            FinancialAccountType is FinancialAccountType.InternationalBankAccount && InternationalBankAccount != null
                ? new InternationalBankAccountRequestDto(
                    InternationalBankAccount.SwiftBicCode!,
                    InternationalBankAccount.IbanNumber!,
                    InternationalBankAccount.AccountNumber!)
                : null,
            FinancialAccountType is FinancialAccountType.Cash && CashAccount != null 
                ? new CashAccountRequestDto(CashAccount.AccountType) 
                : null);
    }
}
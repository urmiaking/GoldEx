using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.BankAccounts;

public record GetBankAccountResponse(
    Guid Id,
    BankAccountType BankAccountType,
    string AccountHolderName,
    string BankName,
    Guid CustomerId,
    GetPriceUnitTitleResponse PriceUnit,
    GetLocalBankAccountResponse? LocalBankAccount,
    GetInternationalBankAccountResponse? InternationalBankAccount);
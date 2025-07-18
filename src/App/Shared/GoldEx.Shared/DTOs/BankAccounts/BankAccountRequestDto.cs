using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.BankAccounts;

public record BankAccountRequestDto(Guid? Id,
    BankAccountType BankAccountType,
    string AccountHolderName, 
    string BankName,
    Guid PriceUnitId,
    LocalBankAccountRequestDto? LocalBankAccount,
    InternationalBankAccountRequestDto? InternationalBankAccount);
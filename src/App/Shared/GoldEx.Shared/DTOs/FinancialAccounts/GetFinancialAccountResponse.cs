using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record GetFinancialAccountResponse(
    Guid Id,
    FinancialAccountType FinancialAccountType,
    string SupplierFullName,
    string SupplierPhoneNumber,
    GetLedgerAccountResponse? LedgerAccount,
    GetPriceUnitTitleResponse PriceUnit,
    GetLocalBankAccountResponse? LocalBankAccount,
    GetInternationalBankAccountResponse? InternationalBankAccount);
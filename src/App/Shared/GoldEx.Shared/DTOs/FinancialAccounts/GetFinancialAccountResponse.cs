using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record GetFinancialAccountResponse(
    Guid Id,
    FinancialAccountType FinancialAccountType,
    string SupplierFullName,
    string SupplierPhoneNumber,
    GetPriceUnitTitleResponse PriceUnit,
    GetLocalBankAccountResponse? LocalBankAccount,
    GetInternationalBankAccountResponse? InternationalBankAccount);
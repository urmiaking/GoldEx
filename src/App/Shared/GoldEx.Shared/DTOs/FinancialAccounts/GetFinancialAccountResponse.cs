using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.FinancialAccounts;

public record GetFinancialAccountResponse(
    Guid Id,
    FinancialAccountType FinancialAccountType,
    GetCustomerResponse? Customer,
    GetPriceUnitTitleResponse PriceUnit,
    GetLocalBankAccountResponse? LocalBankAccount,
    GetInternationalBankAccountResponse? InternationalBankAccount);
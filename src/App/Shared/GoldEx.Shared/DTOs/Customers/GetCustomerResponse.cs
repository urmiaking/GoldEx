using GoldEx.Shared.DTOs.BankAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Customers;

public record GetCustomerResponse(Guid Id,
    string FullName,
    string NationalId,
    string PhoneNumber,
    string? Address,
    decimal? CreditLimit,
    GetPriceUnitTitleResponse? CreditLimitPriceUnit,
    DateTime CreatedAt,
    CustomerType CustomerType,
    List<GetBankAccountResponse>? BankAccounts);
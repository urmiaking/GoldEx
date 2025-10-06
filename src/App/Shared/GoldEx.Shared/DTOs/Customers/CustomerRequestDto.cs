using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Customers;

/// <summary>
/// Represents a request to create or update a customer.
/// </summary>
/// <param name="Id"></param>
/// <param name="FullName"></param>
/// <param name="NationalId"></param>
/// <param name="PhoneNumber"></param>
/// <param name="Address"></param>
/// <param name="CreditLimit"></param>
/// <param name="CreditLimitPriceUnitId"></param>
/// <param name="CustomerType"></param>
/// <param name="FinancialAccounts"></param>
public record CustomerRequestDto(Guid? Id,
    string FullName,
    string NationalId,
    string PhoneNumber,
    string? Address,
    decimal? CreditLimit,
    Guid? CreditLimitPriceUnitId,
    CustomerType CustomerType,
    List<FinancialAccountRequestDto>? FinancialAccounts = null);
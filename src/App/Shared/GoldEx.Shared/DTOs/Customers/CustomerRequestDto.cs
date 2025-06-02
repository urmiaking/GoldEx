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
/// <param name="CreditLimitUnit"></param>
/// <param name="CustomerType"></param>
public record CustomerRequestDto(Guid? Id,
    string FullName,
    string NationalId,
    string PhoneNumber,
    string? Address,
    decimal? CreditLimit,
    UnitType? CreditLimitUnit,
    CustomerType CustomerType);
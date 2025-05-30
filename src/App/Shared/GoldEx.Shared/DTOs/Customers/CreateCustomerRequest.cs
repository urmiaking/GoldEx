using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Customers;

public record CreateCustomerRequest(Guid Id,
    string FullName,
    string NationalId,
    string PhoneNumber,
    string? Address,
    decimal? CreditLimit,
    UnitType? CreditLimitUnit,
    CustomerType CustomerType);
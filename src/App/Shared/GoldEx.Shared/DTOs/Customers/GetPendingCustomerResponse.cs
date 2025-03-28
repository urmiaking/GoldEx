using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Customers;

public record GetPendingCustomerResponse(Guid Id,
    string FullName,
    string NationalId,
    string PhoneNumber,
    string? Address,
    double? CreditLimit,
    UnitType? CreditLimitUnit,
    CustomerType CustomerType,
    DateTime LastModifiedDate,
    ModifyStatus? Status,
    bool? IsDeleted);
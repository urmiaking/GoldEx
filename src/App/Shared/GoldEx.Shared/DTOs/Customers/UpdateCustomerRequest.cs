﻿using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Customers;

public record UpdateCustomerRequest(string FullName,
    string NationalId,
    string PhoneNumber,
    string? Address,
    double? CreditLimit,
    UnitType? CreditLimitUnit,
    CustomerType CustomerType);
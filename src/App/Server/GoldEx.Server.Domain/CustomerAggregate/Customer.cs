﻿using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.CustomerAggregate;

public class Customer : CustomerBase, ISoftDeleteEntity
{
    public Customer(CustomerType customerType,
        string fullName,
        string nationalId,
        string? phoneNumber,
        string? address,
        double? creditLimit,
        UnitType? creditLimitUnit) : base(customerType,
        fullName,
        nationalId,
        phoneNumber,
        address,
        creditLimit,
        creditLimitUnit) { }

    public Customer(CustomerId id,
        CustomerType customerType,
        string fullName,
        string nationalId,
        string? phoneNumber,
        string? address,
        double? creditLimit,
        UnitType? creditLimitUnit) : base(
        customerType,
        fullName,
        nationalId,
        phoneNumber,
        address,
        creditLimit,
        creditLimitUnit)
    {
        Id = id;
    }

    private Customer() { }

    public bool IsDeleted { get; private set; }
    public void SetDeleted() => IsDeleted = true;
}

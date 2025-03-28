using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Offline.Domain.CustomerAggregate;

public class Customer : CustomerBase, ITrackableEntity
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
        creditLimitUnit)
    {
    }

    public Customer(CustomerId id,
        CustomerType customerType,
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
        creditLimitUnit)
    {
        Id = id;
    }

    private Customer()
    {

    }

    public ModifyStatus Status { get; private set; } = ModifyStatus.Created;
    public void SetStatus(ModifyStatus status)
    {
        Status = status;
    }
}
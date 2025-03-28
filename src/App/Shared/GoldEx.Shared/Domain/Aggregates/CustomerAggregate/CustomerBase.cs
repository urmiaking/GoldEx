using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Domain.Aggregates.CustomerAggregate;

public readonly record struct CustomerId(Guid Value);
public class CustomerBase : EntityBase<CustomerId>, ISyncableEntity
{
    public CustomerBase(CustomerType customerType, string fullName, string nationalId,
        string? phoneNumber, string? address, double? creditLimit, UnitType? creditLimitUnit)
        : this(new CustomerId(Guid.NewGuid()),
            customerType,
            fullName,
            nationalId,
            phoneNumber,
            address,
            creditLimit,
            creditLimitUnit)
    {
    }

    public CustomerBase(CustomerId id, CustomerType customerType, string fullName, string nationalId,
        string? phoneNumber, string? address, double? creditLimit, UnitType? creditLimitUnit) : base(id)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        NationalId = nationalId;
        Address = address;
        CustomerType = customerType;
        CreditLimit = creditLimit;
        CreditLimitUnit = creditLimitUnit;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected CustomerBase()
    { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string FullName { get; private set; }
    public CustomerType CustomerType { get; private set; }
    public string NationalId { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public double? CreditLimit { get; private set; }
    public UnitType? CreditLimitUnit { get; private set; }

    public DateTime LastModifiedDate { get; private set; }
    public void SetLastModifiedDate() => LastModifiedDate = DateTime.UtcNow;
    public void SetCustomerType(CustomerType customerType) => CustomerType = customerType;
    public void SetFullName(string fullName) => FullName = fullName;
    public void SetNationalId(string nationalId) => NationalId = nationalId;
    public void SetPhoneNumber(string phoneNumber) => PhoneNumber = phoneNumber;
    public void SetAddress(string? address) => Address = address;
    public void SetCreditLimit(double? creditLimit, UnitType? creditLimitUnit)
    {
        CreditLimit = creditLimit;
        CreditLimitUnit = creditLimitUnit;
    }
}

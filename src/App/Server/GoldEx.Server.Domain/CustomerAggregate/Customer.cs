using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.CustomerAggregate;

public readonly record struct CustomerId(Guid Value);

public class Customer : EntityBase<CustomerId>
{
    public Customer(string firstName, string lastName, string mobileNumber, Gender gender, string? nationalId = null)
    {
        FirstName = firstName;
        LastName = lastName;
        MobileNumber = mobileNumber;
        Gender = gender;
        NationalId = nationalId;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Customer() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? NationalId { get; private set; }
    public string MobileNumber { get; private set; }
    public Gender Gender { get; set; }

    public void SetFirstName(string firstName) => FirstName = firstName;
    public void SetLastName(string lastName) => LastName = lastName;
    public void SetNationalId(string? nationalId) => NationalId = nationalId;
    public void SetGender(Gender gender) => Gender = gender;
    public void SetMobileNumber(string mobileNumber) => MobileNumber = mobileNumber;
}

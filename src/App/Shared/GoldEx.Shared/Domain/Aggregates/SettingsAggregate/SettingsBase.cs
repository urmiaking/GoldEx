using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Domain.Aggregates.SettingsAggregate;


public readonly record struct SettingsId(Guid Value);

public class SettingsBase : EntityBase<SettingsId>, ISyncableEntity
{
    public SettingsBase(string institutionName,
        string address,
        string phoneNumber,
        double tax,
        double profit) : base(new SettingsId(Guid.NewGuid()))
    {
        InstitutionName = institutionName;
        Address = address;
        PhoneNumber = phoneNumber;
        Tax = tax;
        Profit = profit;
    }

    public SettingsBase(SettingsId id,
        string institutionName,
        string address,
        string phoneNumber,
        double tax,
        double profit) : base(id)
    {
        InstitutionName = institutionName;
        Address = address;
        PhoneNumber = phoneNumber;
        Tax = tax;
        Profit = profit;
    }

    public string InstitutionName { get; private set; }
    public string Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public double Tax { get; private set; }
    public double Profit { get; private set; }

    public void SetInstitutionName(string institutionName) => InstitutionName = institutionName;
    public void SetAddress(string address) => Address = address;
    public void SetPhoneNumber(string phoneNumber) => PhoneNumber = phoneNumber;
    public void SetTax(double tax) => Tax = tax;
    public void SetProfit(double profit) => Profit = profit;

    public DateTime LastModifiedDate { get; private set; }
    public void SetLastModifiedDate() => LastModifiedDate = DateTime.UtcNow;
}
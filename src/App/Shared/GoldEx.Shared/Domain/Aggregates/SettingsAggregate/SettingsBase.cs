using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Domain.Aggregates.SettingsAggregate;

public readonly record struct SettingsId(Guid Value);

public class SettingsBase : EntityBase<SettingsId>, ISyncableEntity
{
    public SettingsBase(string institutionName,
        string address,
        string phoneNumber,
        double tax,
        double goldProfit,
        double jewelryProfit) : base(new SettingsId(Guid.NewGuid()))
    {
        InstitutionName = institutionName;
        Address = address;
        PhoneNumber = phoneNumber;
        Tax = tax;
        GoldProfit = goldProfit;
        JewelryProfit = jewelryProfit;
    }

    public SettingsBase(SettingsId id,
        string institutionName,
        string address,
        string phoneNumber,
        double tax,
        double goldProfit,
        double jewelryProfit) : base(id)
    {
        InstitutionName = institutionName;
        Address = address;
        PhoneNumber = phoneNumber;
        Tax = tax;
        GoldProfit = goldProfit;
        JewelryProfit = jewelryProfit;
    }

    public string InstitutionName { get; private set; }
    public string Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public double Tax { get; private set; }
    public double GoldProfit { get; private set; }
    public double JewelryProfit { get; private set; }

    public void SetInstitutionName(string institutionName) => InstitutionName = institutionName;
    public void SetAddress(string address) => Address = address;
    public void SetPhoneNumber(string phoneNumber) => PhoneNumber = phoneNumber;
    public void SetTax(double tax) => Tax = tax;
    public void SetGoldProfit(double profit) => GoldProfit = profit;
    public void SetJewelryProfit(double profit) => JewelryProfit = profit; 

    public DateTime LastModifiedDate { get; private set; }
    public void SetLastModifiedDate() => LastModifiedDate = DateTime.UtcNow;
}
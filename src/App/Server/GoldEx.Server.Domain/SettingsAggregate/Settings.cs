using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SettingsAggregate;

public readonly record struct SettingsId(Guid Value);
public class Settings : EntityBase<SettingsId>
{
    public static Settings Create(string institutionName,
        string address,
        string phoneNumber,
        double tax,
        double goldProfit,
        double jewelryProfit)
    {
        return new Settings
        {
            Id = new SettingsId(Guid.NewGuid()),
            InstitutionName = institutionName,
            Address = address,
            PhoneNumber = phoneNumber,
            Tax = tax,
            GoldProfit = goldProfit,
            JewelryProfit = jewelryProfit
        };
    }

#pragma warning disable CS8618 
    private Settings() { }
#pragma warning restore CS8618

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
}
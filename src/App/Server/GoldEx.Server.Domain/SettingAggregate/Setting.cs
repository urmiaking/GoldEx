using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SettingAggregate;

public readonly record struct SettingsId(Guid Value);
public class Setting : EntityBase<SettingsId>
{
    public static Setting Create(string institutionName,
        string address,
        string phoneNumber,
        float taxPercent,
        float goldProfitPercent,
        float jewelryProfitPercent,
        TimeSpan priceUpdateInterval)
    {
        return new Setting
        {
            Id = new SettingsId(Guid.NewGuid()),
            InstitutionName = institutionName,
            Address = address,
            PhoneNumber = phoneNumber,
            TaxPercent = taxPercent,
            GoldProfitPercent = goldProfitPercent,
            JewelryProfitPercent = jewelryProfitPercent,
            PriceUpdateInterval = priceUpdateInterval
        };
    }

#pragma warning disable CS8618 
    private Setting() { }
#pragma warning restore CS8618

    public string InstitutionName { get; private set; }
    public string Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public float TaxPercent { get; private set; }
    public float GoldProfitPercent { get; private set; }
    public float JewelryProfitPercent { get; private set; }
    public TimeSpan PriceUpdateInterval { get; private set; }

    public void SetInstitutionName(string institutionName) => InstitutionName = institutionName;
    public void SetAddress(string address) => Address = address;
    public void SetPhoneNumber(string phoneNumber) => PhoneNumber = phoneNumber;
    public void SetTax(float taxPercent) => TaxPercent = taxPercent;
    public void SetGoldProfit(float profitPercent) => GoldProfitPercent = profitPercent;
    public void SetJewelryProfit(float profitPercent) => JewelryProfitPercent = profitPercent;
    public void SetPriceUpdateInterval(TimeSpan interval) => PriceUpdateInterval = interval;
}
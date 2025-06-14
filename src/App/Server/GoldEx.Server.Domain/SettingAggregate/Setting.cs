using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SettingAggregate;

public readonly record struct SettingsId(Guid Value);
public class Setting : EntityBase<SettingsId>
{
    public static Setting Create(string institutionName,
        string address,
        string phoneNumber,
        decimal taxPercent,
        decimal goldProfitPercent,
        decimal jewelryProfitPercent,
        decimal goldSafetyMarginPercent,
        decimal oldGoldCarat,
        TimeSpan priceUpdateInterval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(goldProfitPercent, 0, nameof(goldProfitPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(jewelryProfitPercent, 0, nameof(jewelryProfitPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(goldSafetyMarginPercent, 0, nameof(goldSafetyMarginPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(oldGoldCarat, 0, nameof(oldGoldCarat));

        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxPercent, 100, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(goldProfitPercent, 100, nameof(goldProfitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(jewelryProfitPercent, 100, nameof(jewelryProfitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(goldSafetyMarginPercent, 100, nameof(goldSafetyMarginPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(oldGoldCarat, 1000, nameof(oldGoldCarat));

        return new Setting
        {
            Id = new SettingsId(Guid.NewGuid()),
            InstitutionName = institutionName,
            Address = address,
            PhoneNumber = phoneNumber,
            TaxPercent = taxPercent,
            GoldProfitPercent = goldProfitPercent,
            JewelryProfitPercent = jewelryProfitPercent,
            GoldSafetyMarginPercent = goldSafetyMarginPercent,
            OldGoldCarat = oldGoldCarat,
            PriceUpdateInterval = priceUpdateInterval
        };
    }

#pragma warning disable CS8618 
    private Setting() { }
#pragma warning restore CS8618

    public string InstitutionName { get; private set; }
    public string Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal GoldProfitPercent { get; private set; }
    public decimal GoldSafetyMarginPercent { get; private set; }
    public decimal JewelryProfitPercent { get; private set; }
    public decimal OldGoldCarat { get; private set; }
    public TimeSpan PriceUpdateInterval { get; private set; }

    public void SetInstitutionName(string institutionName) => InstitutionName = institutionName;
    public void SetAddress(string address) => Address = address;
    public void SetPhoneNumber(string phoneNumber) => PhoneNumber = phoneNumber;
    public void SetTax(decimal taxPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxPercent, 100, nameof(taxPercent));

        TaxPercent = taxPercent;
    }

    public void SetGoldProfit(decimal profitPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100, nameof(profitPercent));

        GoldProfitPercent = profitPercent;
    }

    public void SetJewelryProfit(decimal profitPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100, nameof(profitPercent));

        JewelryProfitPercent = profitPercent;
    }

    public void SetGoldSafetyMargin(decimal safetyMargin) => GoldSafetyMarginPercent = safetyMargin;
    public void SetPriceUpdateInterval(TimeSpan interval) => PriceUpdateInterval = interval;
    public void SetOldGoldCarat(decimal oldGoldCarat)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(oldGoldCarat, 0, nameof(oldGoldCarat));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(oldGoldCarat, 1000, nameof(oldGoldCarat));
        OldGoldCarat = oldGoldCarat;
    }
}
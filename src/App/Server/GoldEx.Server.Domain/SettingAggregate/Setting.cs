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
        decimal moltenGoldCommissionPercent,
        decimal goldSafetyMarginPercent,
        decimal usedGoldFinenessDeductionRate,
        decimal gramPerMesghal,
        TimeSpan priceUpdateInterval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(goldProfitPercent, 0, nameof(goldProfitPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(jewelryProfitPercent, 0, nameof(jewelryProfitPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(moltenGoldCommissionPercent, 0, nameof(moltenGoldCommissionPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(goldSafetyMarginPercent, 0, nameof(goldSafetyMarginPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(usedGoldFinenessDeductionRate, 0, nameof(usedGoldFinenessDeductionRate));
        ArgumentOutOfRangeException.ThrowIfLessThan(gramPerMesghal, 0, nameof(gramPerMesghal));

        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxPercent, 100, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(goldProfitPercent, 100, nameof(goldProfitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(jewelryProfitPercent, 100, nameof(jewelryProfitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(moltenGoldCommissionPercent, 100, nameof(moltenGoldCommissionPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(goldSafetyMarginPercent, 100, nameof(goldSafetyMarginPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(usedGoldFinenessDeductionRate, 1000, nameof(usedGoldFinenessDeductionRate));

        ArgumentNullException.ThrowIfNull(institutionName, nameof(institutionName));
        ArgumentNullException.ThrowIfNull(address, nameof(address));
        ArgumentNullException.ThrowIfNull(phoneNumber, nameof(phoneNumber));

        return new Setting
        {
            Id = new SettingsId(Guid.NewGuid()),
            InstitutionName = institutionName,
            Address = address,
            PhoneNumber = phoneNumber,
            TaxPercent = taxPercent,
            GoldProfitPercent = goldProfitPercent,
            JewelryProfitPercent = jewelryProfitPercent,
            MoltenGoldCommissionPercent = moltenGoldCommissionPercent,
            GoldSafetyMarginPercent = goldSafetyMarginPercent,
            UsedGoldFinenessDeductionRate = usedGoldFinenessDeductionRate,
            GramPerMesghal = gramPerMesghal,
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
    public decimal MoltenGoldCommissionPercent { get; private set; }
    public decimal UsedGoldFinenessDeductionRate { get; private set; }
    public decimal GramPerMesghal { get; private set; }
    public TimeSpan PriceUpdateInterval { get; private set; }
    public BarcodePrintSettings? BarcodePrintSettings { get; private set; }

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

    public void SetMoltenGoldCommission(decimal commissionPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(commissionPercent, 0, nameof(commissionPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(commissionPercent, 100, nameof(commissionPercent));

        MoltenGoldCommissionPercent = commissionPercent;
    }

    public void SetGoldSafetyMargin(decimal safetyMargin) => GoldSafetyMarginPercent = safetyMargin;
    public void SetPriceUpdateInterval(TimeSpan interval) => PriceUpdateInterval = interval;
    public void SetUsedGoldFinenessDeduction(decimal deductionRate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(deductionRate, -250, nameof(deductionRate));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(deductionRate, 750, nameof(deductionRate));
        UsedGoldFinenessDeductionRate = deductionRate;
    }

    public void SetGramPerMesghal(decimal gramPerMesghal)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(gramPerMesghal, 0, nameof(gramPerMesghal));
        GramPerMesghal = gramPerMesghal;
    }

    public void UpdateBarcodePrintSettings(BarcodePrintSettings barcodePrintSettings)
    {
        BarcodePrintSettings = barcodePrintSettings ?? throw new ArgumentNullException(nameof(barcodePrintSettings));
    }

    public void InitializeBarcodePrintSettings()
    {
        BarcodePrintSettings ??= BarcodePrintSettings.CreateDefault();
    }
}
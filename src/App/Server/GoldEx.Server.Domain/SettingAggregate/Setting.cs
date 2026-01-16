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
        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(goldProfitPercent, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(jewelryProfitPercent, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(moltenGoldCommissionPercent, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(goldSafetyMarginPercent, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(usedGoldFinenessDeductionRate, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(gramPerMesghal, 0);

        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxPercent, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(goldProfitPercent, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(jewelryProfitPercent, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(moltenGoldCommissionPercent, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(goldSafetyMarginPercent, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(usedGoldFinenessDeductionRate, 1000);

        ArgumentNullException.ThrowIfNull(institutionName);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(phoneNumber);

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
        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxPercent, 100);

        TaxPercent = taxPercent;
    }

    public void SetGoldProfit(decimal profitPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100);

        GoldProfitPercent = profitPercent;
    }

    public void SetJewelryProfit(decimal profitPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100);

        JewelryProfitPercent = profitPercent;
    }

    public void SetMoltenGoldCommission(decimal commissionPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(commissionPercent, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(commissionPercent, 100);

        MoltenGoldCommissionPercent = commissionPercent;
    }

    public void SetGoldSafetyMargin(decimal safetyMargin) => GoldSafetyMarginPercent = safetyMargin;
    public void SetPriceUpdateInterval(TimeSpan interval) => PriceUpdateInterval = interval;
    public void SetUsedGoldFinenessDeduction(decimal deductionRate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(deductionRate, -250);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(deductionRate, 750);
        UsedGoldFinenessDeductionRate = deductionRate;
    }

    public void SetGramPerMesghal(decimal gramPerMesghal)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(gramPerMesghal, 0);
        GramPerMesghal = gramPerMesghal;
    }

    public void UpdateBarcodePrintSettings(BarcodePrintSettings barcodePrintSettings)
    {
        BarcodePrintSettings = barcodePrintSettings ?? throw new ArgumentNullException(nameof(barcodePrintSettings));
    }
}
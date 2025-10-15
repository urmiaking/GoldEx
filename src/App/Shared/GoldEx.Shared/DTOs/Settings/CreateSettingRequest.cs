namespace GoldEx.Shared.DTOs.Settings;

public record CreateSettingRequest(
    string InstitutionName,
    string Address,
    string PhoneNumber,
    decimal TaxPercent,
    decimal GoldProfitPercent,
    decimal JewelryProfitPercent,
    decimal MoltenGoldCommissionPercent,
    decimal GoldSafetyMarginPercent,
    decimal UsedGoldFinenessDeductionRate,
    decimal GramPerMesghal,
    TimeSpan PriceUpdateInterval);
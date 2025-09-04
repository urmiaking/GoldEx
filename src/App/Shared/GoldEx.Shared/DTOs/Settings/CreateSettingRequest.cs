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
    decimal OldGoldCarat,
    TimeSpan PriceUpdateInterval);
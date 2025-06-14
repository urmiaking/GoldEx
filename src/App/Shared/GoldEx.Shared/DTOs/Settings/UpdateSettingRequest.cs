namespace GoldEx.Shared.DTOs.Settings;

public record UpdateSettingRequest(
    string InstitutionName,
    string Address,
    string PhoneNumber,
    decimal TaxPercent,
    decimal GoldProfitPercent,
    decimal JewelryProfitPercent,
    TimeSpan PriceUpdateInterval,
    decimal GoldSafetyMarginPercent,
    decimal OldGoldCarat,
    byte[]? IconContent);
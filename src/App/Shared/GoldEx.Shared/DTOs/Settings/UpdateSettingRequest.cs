namespace GoldEx.Shared.DTOs.Settings;

public record UpdateSettingRequest(
    string InstitutionName,
    string Address,
    string PhoneNumber,
    decimal TaxPercent,
    decimal GoldProfitPercent,
    decimal JewelryProfitPercent,
    decimal MoltenGoldCommissionPercent,
    TimeSpan PriceUpdateInterval,
    decimal GoldSafetyMarginPercent,
    decimal UsedGoldFineness,
    decimal GramPerMesghal,
    byte[]? IconContent);
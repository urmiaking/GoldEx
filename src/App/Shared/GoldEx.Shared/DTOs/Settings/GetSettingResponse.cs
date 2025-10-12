namespace GoldEx.Shared.DTOs.Settings;

public record GetSettingResponse(
    Guid Id,
    string InstitutionName,
    string Address,
    string PhoneNumber,
    decimal TaxPercent,
    decimal GoldProfitPercent,
    decimal JewelryProfitPercent,
    decimal MoltenGoldCommissionPercent,
    decimal GoldSafetyMarginPercent,
    decimal UsedGoldFineness,
    decimal GramPerMesghal,
    TimeSpan PriceUpdateInterval,
    bool HasIcon);
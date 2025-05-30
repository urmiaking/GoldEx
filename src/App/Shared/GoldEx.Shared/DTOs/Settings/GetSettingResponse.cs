namespace GoldEx.Shared.DTOs.Settings;

public record GetSettingResponse(
    Guid Id,
    string InstitutionName,
    string Address,
    string PhoneNumber,
    float TaxPercent,
    float GoldProfitPercent,
    float JewelryProfitPercent,
    TimeSpan PriceUpdateInterval);
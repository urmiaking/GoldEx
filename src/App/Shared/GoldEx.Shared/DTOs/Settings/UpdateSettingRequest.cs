namespace GoldEx.Shared.DTOs.Settings;

public record UpdateSettingRequest(
    string InstitutionName,
    string Address,
    string PhoneNumber,
    float TaxPercent,
    float GoldProfitPercent,
    float JewelryProfitPercent,
    TimeSpan PriceUpdateInterval);
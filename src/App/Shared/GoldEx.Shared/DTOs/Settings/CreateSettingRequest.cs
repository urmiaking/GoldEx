namespace GoldEx.Shared.DTOs.Settings;

public record CreateSettingRequest(
    string InstitutionName,
    string Address,
    string PhoneNumber,
    float TaxPercent,
    float GoldProfitPercent,
    float JewelryProfitPercent);
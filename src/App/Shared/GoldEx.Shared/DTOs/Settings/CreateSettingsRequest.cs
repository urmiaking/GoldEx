namespace GoldEx.Shared.DTOs.Settings;

public record CreateSettingsRequest(
    Guid Id,
    string InstitutionName,
    string Address,
    string PhoneNumber,
    double Tax,
    double GoldProfit,
    double JewelryProfit);
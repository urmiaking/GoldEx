namespace GoldEx.Shared.DTOs.Settings;

public record UpdateSettingsRequest(string InstitutionName, string Address, string PhoneNumber, double Tax, double Profit);
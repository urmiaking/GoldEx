namespace GoldEx.Shared.DTOs.Settings;

public record GetSettingsResponse(Guid Id, string InstitutionName, string Address, string PhoneNumber, double Tax, double Profit);
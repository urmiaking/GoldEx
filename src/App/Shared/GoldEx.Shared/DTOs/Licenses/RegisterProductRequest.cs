namespace GoldEx.Shared.DTOs.Licenses;

public record RegisterProductRequest(string InstitutionName, string Address, string PhoneNumber, int Token, byte[]? IconContent);
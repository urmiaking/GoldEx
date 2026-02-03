namespace GoldEx.Shared.DTOs.Licenses;

public record RegisterProductRequest(
    string InstitutionName,
    string Address,
    string InstitutionPhoneNumber,
    string PhoneNumber,
    string Token,
    byte[]? IconContent);
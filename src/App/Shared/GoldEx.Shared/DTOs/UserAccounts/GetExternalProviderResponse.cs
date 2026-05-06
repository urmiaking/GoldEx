namespace GoldEx.Shared.DTOs.UserAccounts;

public record ExternalProviderDto(string Name, string? DisplayName);
public record LinkedLoginDto(string Provider, string? ProviderDisplayName, string ProviderKey);

public record GetExternalProviderResponse(
    List<LinkedLoginDto> CurrentLogins,
    List<ExternalProviderDto> OtherProviders,
    bool CanRemoveLogins);
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.UserAccounts;

namespace GoldEx.Shared.Services.Abstractions;

public interface IUserAccountService
{
    #region User Profile

    Task<GetUserAccountResponse> GetCurrentUserInfoAsync(CancellationToken cancellationToken = default);
    Task UpdateUserFullNameAsync(UpdateUserFullNameRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserPhoneNumberAsync(UpdateUserPhoneNumberRequest request, CancellationToken cancellationToken = default);
    Task SendVerificationTokenAsync(SendVerificationCodeRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserEmailAsync(UpdateUserEmailRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserPasswordAsync(UpdateUserPasswordRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region 2FA

    Task<GetTwoFactorAuthStatusResponse> GetUser2FaStatusAsync(CancellationToken cancellationToken = default);
    Task<GetAuthenticatorKeyResponse> GetAuthenticatorKeyAsync(CancellationToken cancellationToken = default);
    Task ForgetDeviceAsync(CancellationToken cancellationToken = default);
    Task Disable2FaAsync(CancellationToken cancellationToken = default);
    Task<EnableTwoFactorAuthResponse> Enable2FaAsync(EnableTwoFactorAuthRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region External Login

    Task<GetExternalProviderResponse> GetExternalProvidersAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Passkeys

    Task<List<GetPasskeyResponse>> GetPasskeysAsync(CancellationToken cancellationToken = default);
    Task<string> GetPasskeyCreationOptionsAsync(CancellationToken cancellationToken = default);
    Task<string> GetPasskeyRequestOptionsAsync(string? userName, CancellationToken cancellationToken = default);
    Task AddPasskeyAsync(CreatePasskeyRequest request, CancellationToken cancellationToken = default);
    Task RemovePasskeyAsync(string credentialId, CancellationToken cancellationToken = default);

    #endregion

    #region Users

    Task<List<GetUserAccountResponse>> GetAccountsListAsync(CancellationToken cancellationToken = default);
    Task LockUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task UnlockUserAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion
}
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Common.Data;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal sealed class UserAccountService(HttpClient client, JsonSerializerOptions jsonOptions) : IUserAccountService
{
    public async Task<GetUserAccountResponse> GetCurrentUserInfoAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserAccounts.GetCurrentUser(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetUserAccountResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task UpdateUserFullNameAsync(UpdateUserFullNameRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.UserAccounts.UpdateFullName(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateUserPhoneNumberAsync(UpdateUserPhoneNumberRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.UserAccounts.UpdatePhoneNumber(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task SendVerificationTokenAsync(SendVerificationCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.UserAccounts.SendVerificationToken(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateUserEmailAsync(UpdateUserEmailRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.UserAccounts.UpdateEmail(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateUserPasswordAsync(UpdateUserPasswordRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.UserAccounts.UpdatePassword(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<GetTwoFactorAuthStatusResponse> GetUser2FaStatusAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserAccounts.Get2FaStatus(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetTwoFactorAuthStatusResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task ForgetDeviceAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.UserAccounts.ForgetDevice(), jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task Disable2FaAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.UserAccounts.Disable2Fa(), jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<GetAuthenticatorKeyResponse> GetAuthenticatorKeyAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserAccounts.GetAuthenticatorKey(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetAuthenticatorKeyResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<EnableTwoFactorAuthResponse> Enable2FaAsync(EnableTwoFactorAuthRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.UserAccounts.Enable2Fa(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<EnableTwoFactorAuthResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetExternalProviderResponse> GetExternalProvidersAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserAccounts.GetExternalProviders(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetExternalProviderResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetPasskeyResponse>> GetPasskeysAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserAccounts.GetPasskeys(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetPasskeyResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<string> GetPasskeyCreationOptionsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserAccounts.GetPasskeyCreationOptions(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<string> GetPasskeyRequestOptionsAsync(string? userName, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserAccounts.GetPasskeyRequestOptions(userName), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task AddPasskeyAsync(CreatePasskeyRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.UserAccounts.AddPasskey(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task RemovePasskeyAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.UserAccounts.RemovePasskey(credentialId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<List<GetUserAccountResponse>> GetAccountsListAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserAccounts.GetAccountsList(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetUserAccountResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task LockUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(ApiUrls.UserAccounts.LockUser(id), null, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UnlockUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(ApiUrls.UserAccounts.UnlockUser(id), null, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task CreateAccountAsync(UserAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.UserAccounts.CreateAccount(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateAccountAsync(Guid id, UserAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.UserAccounts.UpdateAccount(id), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.UserAccounts.DeleteAccount(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}
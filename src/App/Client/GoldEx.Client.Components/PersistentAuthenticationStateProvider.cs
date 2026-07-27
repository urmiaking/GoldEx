using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace GoldEx.Client.Components;

// This is a client-side AuthenticationStateProvider that determines the user's authentication state by:
// 1. Looking for data persisted in the page when it was rendered on the server (prerender mode).
// 2. Falling back to an API call to fetch auth state (no-prerender mode).
//
// This dual approach ensures the migration between render modes is reversible —
// switching the render mode in App.razor.cs is the only change needed.
//
// This only provides a username and email for display purposes. It does not actually include any tokens
// that authenticate to the server when making subsequent requests. That works separately using a
// cookie that will be included on HttpClient requests to the server.
public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> DefaultUnauthenticatedTask =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private readonly Task<AuthenticationState> _authenticationStateTask;

    public PersistentAuthenticationStateProvider(
        PersistentComponentState state,
        HttpClient httpClient,
        JsonSerializerOptions jsonOptions)
    {
        // Strategy 1: Try PersistentComponentState (available when prerendering is enabled)
        if (state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) && userInfo is not null)
        {
            _authenticationStateTask = Task.FromResult(
                new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(userInfo.Claims,
                    authenticationType: nameof(PersistentAuthenticationStateProvider)))));
            return;
        }

        // Strategy 2: Fetch from API endpoint (used when prerendering is disabled)
        _authenticationStateTask = FetchAuthStateAsync(httpClient, jsonOptions);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authenticationStateTask;

    private static async Task<AuthenticationState> FetchAuthStateAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonOptions)
    {
        try
        {
            var response = await httpClient.GetAsync(ApiUrls.Account.AuthState());

            if (!response.IsSuccessStatusCode)
                return await DefaultUnauthenticatedTask;

            var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>(jsonOptions);

            if (userInfo?.UserClaims is { Count: > 0 })
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(userInfo.Claims,
                    authenticationType: nameof(PersistentAuthenticationStateProvider))));
            }
        }
        catch
        {
            // Silently fall back to unauthenticated on any network/parsing error
        }

        return await DefaultUnauthenticatedTask;
    }
}
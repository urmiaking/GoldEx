using Blazored.LocalStorage;
using GoldEx.Client.Abstractions.Common;
using GoldEx.Client.Components.Constants;
using GoldEx.Sdk.Common.DependencyInjections;

namespace GoldEx.Client.Components.Services;

[ScopedService]
public sealed class VersionReleaseStore(ILocalStorageService localStorage) : IVersionReleaseStore
{
    public async Task<string?> GetLastSeenVersionAsync()
        => await localStorage.GetItemAsync<string>(LocalStorageKeys.LastSeenReleaseVersion);

    public async Task SetLastSeenVersionAsync(string version)
        => await localStorage.SetItemAsync(LocalStorageKeys.LastSeenReleaseVersion, version);
}
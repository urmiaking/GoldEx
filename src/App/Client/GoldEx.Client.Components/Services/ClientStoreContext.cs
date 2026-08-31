using System;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Components.Services;

[ScopedService]
public class ClientStoreContext : IStoreContext
{
    private readonly NavigationManager _navigation;

    public ClientStoreContext(NavigationManager navigation)
    {
        _navigation = navigation;
    }

    public Guid? StoreId { get; set; }

    private string? _storeSlug;
    public string? StoreSlug
    {
        get => _storeSlug;
        set => _storeSlug = value;
    }

    public string? CustomDomain { get; set; }

    public bool IsCustomDomain
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(CustomDomain))
                return true;

            try
            {
                var absUri = _navigation.ToAbsoluteUri(_navigation.Uri);
                var host = absUri.Host?.ToLowerInvariant();
                return !ClientRoutes.Vitrine.IsPlatformDomain(host);
            }
            catch
            {
                try
                {
                    var baseUri = _navigation.BaseUri;
                    if (!string.IsNullOrWhiteSpace(baseUri))
                    {
                        var clean = baseUri.Replace("https://", "").Replace("http://", "").TrimEnd('/');
                        var hostPart = clean.Split(':')[0].Split('/')[0].ToLowerInvariant();
                        return !ClientRoutes.Vitrine.IsPlatformDomain(hostPart);
                    }
                }
                catch
                {
                    // ignore
                }
            }

            return false;
        }
    }

    public void SetStore(Guid storeId, string slug, string? customDomain = null)
    {
        StoreId = storeId;
        _storeSlug = slug;
        CustomDomain = customDomain;
    }
}

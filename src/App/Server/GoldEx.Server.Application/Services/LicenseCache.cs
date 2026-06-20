using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.Licenses;
using System;
using System.Collections.Concurrent;
using GoldEx.Sdk.Common.DependencyInjections;

namespace GoldEx.Server.Application.Services;

[SingletonService]
public class LicenseCache : ILicenseCache
{
    private readonly ConcurrentDictionary<Guid, GetLicenseResponse> _cache = new();

    public GetLicenseResponse? Get(Guid storeId)
    {
        return _cache.TryGetValue(storeId, out var license) ? license : null;
    }

    public void Set(Guid storeId, GetLicenseResponse license)
    {
        _cache[storeId] = license;
    }

    public void Remove(Guid storeId)
    {
        _cache.TryRemove(storeId, out _);
    }

    public void Clear()
    {
        _cache.Clear();
    }
}

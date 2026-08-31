using System;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Server.Application.Services;

public class StoreContext : IStoreContext
{
    public Guid? StoreId { get; private set; }
    public string? StoreSlug { get; private set; }
    public string? CustomDomain { get; private set; }
    public bool IsCustomDomain => !string.IsNullOrWhiteSpace(CustomDomain);

    public void SetStore(Guid storeId, string slug, string? customDomain = null)
    {
        StoreId = storeId;
        StoreSlug = slug;
        CustomDomain = customDomain;
    }
}

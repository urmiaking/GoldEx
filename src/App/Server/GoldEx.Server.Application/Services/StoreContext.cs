using System;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Server.Application.Services;

public class StoreContext : IStoreContext
{
    public Guid? StoreId { get; private set; }
    public string? StoreSlug { get; private set; }

    public void SetStore(Guid storeId, string slug)
    {
        StoreId = storeId;
        StoreSlug = slug;
    }
}

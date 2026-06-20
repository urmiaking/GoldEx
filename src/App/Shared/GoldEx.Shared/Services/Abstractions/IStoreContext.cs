using System;

namespace GoldEx.Shared.Services.Abstractions;

public interface IStoreContext
{
    Guid? StoreId { get; }
    string? StoreSlug { get; }
}

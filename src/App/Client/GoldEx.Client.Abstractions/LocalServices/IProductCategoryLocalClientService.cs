using GoldEx.Shared.DTOs.Categories;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Abstractions.LocalServices;

public interface IProductCategoryLocalClientService : IProductCategoryClientService
{
    Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsSyncedAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsSyncAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
}
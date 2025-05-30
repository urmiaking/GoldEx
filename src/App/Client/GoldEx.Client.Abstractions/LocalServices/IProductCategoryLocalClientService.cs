using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Abstractions.LocalServices;

public interface IProductCategoryLocalClientService : IProductCategoryService
{
    Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsSyncedAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsSyncAsync(Guid id, UpdateProductCategoryRequest request, CancellationToken cancellationToken = default);
}
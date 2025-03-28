using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Abstractions.LocalServices;

public interface IProductLocalClientService : IProductClientService
{
    Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsSyncedAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsSyncAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
}
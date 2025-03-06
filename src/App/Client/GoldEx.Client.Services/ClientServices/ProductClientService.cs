using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Services.ClientServices;

[ScopedService]
public class ProductClientService(
    IProductLocalClientService localService,
    IProductSyncService syncService)
    : IProductClientService
{
    public async Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetListAsync(filter, cancellationToken);
    }

    public async Task<GetProductResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(id, cancellationToken);
    }

    public async Task<GetProductResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(barcode, cancellationToken);
    }

    public async Task CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        await localService.CreateAsync(request, cancellationToken);

        await syncService.SynchronizeAsync(cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        await localService.UpdateAsync(id, request, cancellationToken);

        await syncService.SynchronizeAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        await localService.DeleteAsync(id, false, cancellationToken);

        await syncService.SynchronizeAsync(cancellationToken);
    }

    public Task<List<GetPendingProductResponse>> GetPendingsAsync(DateTime checkpointDate,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
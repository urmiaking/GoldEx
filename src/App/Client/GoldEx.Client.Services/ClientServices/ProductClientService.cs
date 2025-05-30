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
    IProductSyncService syncService,
    IProductCategorySyncService categorySyncService)
    : IProductService
{
    public async Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        await categorySyncService.SynchronizeAsync(cancellationToken);
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetListAsync(filter, cancellationToken);
    }

    public async Task<GetProductResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await categorySyncService.SynchronizeAsync(cancellationToken);
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(id, cancellationToken);
    }

    public async Task<GetProductResponse> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        await categorySyncService.SynchronizeAsync(cancellationToken);
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(barcode, cancellationToken);
    }

    public async Task<bool> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        await localService.CreateAsync(request, cancellationToken);

        await categorySyncService.SynchronizeAsync(cancellationToken);
        await syncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await localService.UpdateAsync(id, request, cancellationToken);

        await categorySyncService.SynchronizeAsync(cancellationToken);
        await syncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false,
        CancellationToken cancellationToken = default)
    {
        await localService.DeleteAsync(id, false, cancellationToken);

        await categorySyncService.SynchronizeAsync(cancellationToken);
        await syncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public Task<List<GetPendingProductResponse>> GetPendingsAsync(DateTime checkpointDate,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
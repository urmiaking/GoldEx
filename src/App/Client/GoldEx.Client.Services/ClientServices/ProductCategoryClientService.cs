using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Categories;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Services.ClientServices;

[ScopedService]
public class ProductCategoryClientService(
    IProductCategoryLocalClientService localService,
    IProductCategorySyncService syncService) 
    : IProductCategoryClientService
{
    public async Task<List<GetCategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAllAsync(cancellationToken);
    }

    public async Task<GetCategoryResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(id, cancellationToken);
    }

    public async Task<bool> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await localService.CreateAsync(request, cancellationToken);

        await syncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        await localService.UpdateAsync(id, request, cancellationToken);

        await syncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false,
        CancellationToken cancellationToken = default)
    {
        await localService.DeleteAsync(id, false, cancellationToken);

        await syncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public Task<List<GetPendingCategoryResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
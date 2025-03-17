using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.DTOs.Categories;
using GoldEx.Shared.Services;
using MapsterMapper;

namespace GoldEx.Server.ClientServices;

[ScopedService]
public class ProductCategoryClientService(IMapper mapper, IProductCategoryService<ProductCategory> service) : IProductCategoryClientService
{
    public async Task<List<GetCategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetAllAsync(cancellationToken);

        return mapper.Map<List<GetCategoryResponse>>(list);
    }

    public async Task<GetCategoryResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await service.GetAsync(new ProductCategoryId(id), cancellationToken);

        return item is null ? null : mapper.Map<GetCategoryResponse>(item);
    }

    public async Task CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new ProductCategory(new ProductCategoryId(request.Id), request.Title);

        await service.CreateAsync(category, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await service.GetAsync(new ProductCategoryId(id), cancellationToken) ??
                       throw new NotFoundException();

        category.SetTitle(request.Title);

        await service.UpdateAsync(category, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        var category = await service.GetAsync(new ProductCategoryId(id), cancellationToken) ?? throw new NotFoundException();

        await service.DeleteAsync(category, deletePermanently, cancellationToken);
    }

    public async Task<List<GetPendingCategoryResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        var items = await service.GetPendingItemsAsync(checkpointDate, cancellationToken);

        return mapper.Map<List<GetPendingCategoryResponse>>(items);
    }
}
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Shared.Services;

public interface IProductClientService
{
    Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<GetProductResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetProductResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default);
    Task<List<GetPendingProductResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}
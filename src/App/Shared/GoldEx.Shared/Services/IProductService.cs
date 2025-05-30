using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Shared.Services;

public interface IProductService
{
    Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<GetProductResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetProductResponse> GetAsync(string barcode, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
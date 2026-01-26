using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

public interface IProductService
{
    Task<List<GetProductResponse>> GetListAsync(string name, ProductType productType, CancellationToken cancellationToken = default);
    Task<GetProductResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, ProductRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
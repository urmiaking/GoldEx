using GoldEx.Shared.DTOs.ProductCategories;

namespace GoldEx.Shared.Services;

public interface IProductCategoryService
{
    Task<List<GetProductCategoryResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task<GetProductCategoryResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
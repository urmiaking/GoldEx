using GoldEx.Shared.DTOs.ProductAttributes;
using GoldEx.Shared.DTOs.ProductCategories;

namespace GoldEx.Shared.Services.Abstractions;

public interface IProductAttributeService
{
    Task<List<ProductAttributeDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<ProductAttributeDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateProductAttributeRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductAttributeRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<CategoryAttributeDto>> GetCategoryAttributesAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task SetCategoryAttributesAsync(SetCategoryAttributesRequest request, CancellationToken cancellationToken = default);
}
